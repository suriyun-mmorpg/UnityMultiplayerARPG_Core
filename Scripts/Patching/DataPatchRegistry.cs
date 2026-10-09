using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MultiplayerARPG
{
    public sealed class DataPatchRegistry
    {
        public readonly Dictionary<string, ScriptableObject> Targets = new Dictionary<string, ScriptableObject>();
        private readonly Dictionary<string, ScriptableObject> sharedRegistryIdentities = new Dictionary<string, ScriptableObject>();
        private readonly HashSet<object> visited = new HashSet<object>(ReferenceComparer.Instance);
        private readonly bool useGameInstance;
        private readonly Action<Exception, object> reportError;

        public DataPatchRegistry(bool useGameInstance = true, Action<Exception, object> reportError = null)
        {
            this.useGameInstance = useGameInstance;
            this.reportError = reportError;
        }

        public static int DataId(ScriptableObject asset)
        {
            if (asset is IPatchableData data)
                return data.DataId;
            throw new InvalidOperationException("Unsupported patch asset: " + asset.GetType().Name);
        }

        public static string Key(string type, int id) => type + ":" + id;

        public void AddGraph(object value)
        {
            try
            {
                AddGraphCore(value);
            }
            catch (Exception ex)
            {
                if (reportError == null)
                    throw;
                reportError(ex, value);
            }
        }

        private void AddGraphCore(object value)
        {
            if (value == null || value is UnityEngine.Object obj && obj == null || !visited.Add(value))
                return;
            Type type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
                return;
            if (value is IPatchableData data && value is ScriptableObject asset)
                Add(asset, data.DataId);
            else if (value is UnityEngine.Object && !(value is GameDatabase))
                return;
            if (value is IList list)
            {
                foreach (object entry in list)
                    AddGraph(entry);
                return;
            }

            // Only serialized data: no properties, delegates, caches or native Unity state.
            foreach (FieldInfo field in DataPatchSerializer.Fields(type, false))
            {
                if (typeof(Delegate).IsAssignableFrom(field.FieldType) || field.FieldType.Namespace?.StartsWith("UnityEngine.AddressableAssets", StringComparison.Ordinal) == true)
                    continue;
                AddGraph(field.GetValue(value));
            }
        }

        private void Add(ScriptableObject asset, int id)
        {
            // Subclasses still share one GameInstance registry and must not alias an ID.
            string family = asset is BaseItem ? nameof(BaseItem) : asset is BaseCharacter ? nameof(BaseCharacter) : asset.GetType().Name;
            string identity = Key(family, id);
            if (sharedRegistryIdentities.TryGetValue(identity, out ScriptableObject duplicate) && !ReferenceEquals(duplicate, asset))
                throw new InvalidOperationException($"Duplicate registry identity or hash collision: {identity} ({duplicate.name}, {asset.name}).");
            sharedRegistryIdentities[identity] = asset;
            string key = Key(asset.GetType().Name, id);
            if (Targets.TryGetValue(key, out ScriptableObject previous) && !ReferenceEquals(previous, asset))
                throw new InvalidOperationException($"Duplicate identity or hash collision: {key} ({previous.name}, {asset.name}).");
            Targets[key] = asset;
        }

        public static DataPatchRegistry Runtime(GameDatabase database, ScriptableObject[] additionalData = null)
        {
            var registry = new DataPatchRegistry();
            registry.AddGraph(database);
            if (additionalData != null)
                foreach (var asset in additionalData)
                    registry.AddGraph(asset);
            // Catalog assets may exist only behind prefab/Addressable references at startup.
            foreach (var asset in registry.Targets.Values)
            {
                if (!(asset is IGameData data))
                    continue;
                if (asset is BaseItem item)
                {
                    if (GameInstance.Items.TryGetValue(data.DataId, out BaseItem existing) && !ReferenceEquals(existing, asset))
                        throw new InvalidOperationException($"Conflicting item ID {data.DataId}.");
                    if (existing == null)
                        GameInstance.AddItems(item);
                    continue;
                }

                if (asset is BaseCharacter character)
                {
                    if (GameInstance.Characters.TryGetValue(data.DataId, out BaseCharacter existing) && !ReferenceEquals(existing, asset))
                        throw new InvalidOperationException($"Conflicting character ID {data.DataId}.");
                    if (existing == null)
                        GameInstance.AddCharacters(character);
                    continue;
                }

                foreach (FieldInfo field in typeof(GameInstance).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    Type fieldType = field.FieldType;
                    if (!fieldType.IsGenericType || fieldType.GetGenericTypeDefinition() != typeof(Dictionary<, >))
                        continue;
                    Type[] args = fieldType.GetGenericArguments();
                    if (args[0] != typeof(int) || !args[1].IsInstanceOfType(asset) || !(field.GetValue(null)is IDictionary dictionary))
                        continue;
                    if (dictionary.Contains(data.DataId) && !ReferenceEquals(dictionary[data.DataId], asset))
                        throw new InvalidOperationException($"Conflicting runtime asset {asset.name}, ID {data.DataId}.");
                    if (!dictionary.Contains(data.DataId))
                    {
                        data.Validate();
                        if (asset is Attribute attribute)
                            RuntimeGameDataSlots.Register(attribute);
                        else if (asset is DamageElement damageElement)
                            RuntimeGameDataSlots.Register(damageElement);
                        else if (asset is Currency currency)
                            RuntimeGameDataSlots.Register(currency);
                        dictionary.Add(data.DataId, asset);
                        data.PrepareRelatesData();
                    }
                }
            }

            foreach (FieldInfo field in typeof(GameInstance).GetFields(BindingFlags.Public | BindingFlags.Static))
                if (field.GetValue(null)is IDictionary dict)
                    foreach (object value in dict.Values)
                        if (value is IPatchableData)
                            registry.AddGraph(value);
            return registry;
        }

        public object Resolve(Type expected, int id)
        {
            if (!useGameInstance)
            {
                var found = new List<ScriptableObject>();
                foreach (var entry in Targets)
                    if (expected.IsInstanceOfType(entry.Value) && DataId(entry.Value) == id)
                        found.Add(entry.Value);
                if (found.Count != 1)
                    throw new InvalidOperationException($"Missing or ambiguous {expected.Name} ID {id}.");
                return found[0];
            }

            // Resolve through the actual GameInstance registry, preserving asset identity.
            if (typeof(BaseItem).IsAssignableFrom(expected) && GameInstance.Items.TryGetValue(id, out BaseItem item) && expected.IsInstanceOfType(item))
                return item;
            if (expected == typeof(Attribute) && GameInstance.Attributes.TryGetValue(id, out Attribute attribute))
                return attribute;
            object match = null;
            foreach (FieldInfo field in typeof(GameInstance).GetFields(BindingFlags.Public | BindingFlags.Static))
                if (field.GetValue(null)is IDictionary dict && dict.Contains(id) && expected.IsInstanceOfType(dict[id]))
                {
                    if (match != null && !ReferenceEquals(match, dict[id]))
                        throw new InvalidOperationException($"Ambiguous {expected.Name} ID {id}.");
                    match = dict[id];
                }

            if (match != null)
                return match;
            // Some data (for example maps and weapon abilities) has no int-keyed
            // GameInstance registry. Resolve those references from the loaded catalog.
            bool hasRuntimeRegistry = false;
            foreach (FieldInfo field in typeof(GameInstance).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                Type type = field.FieldType;
                if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Dictionary<, >))
                    continue;
                Type[] args = type.GetGenericArguments();
                if (args[0] == typeof(int) && args[1].IsAssignableFrom(expected))
                    hasRuntimeRegistry = true;
            }

            if (!hasRuntimeRegistry)
            {
                foreach (ScriptableObject asset in Targets.Values)
                    if (expected.IsInstanceOfType(asset) && DataId(asset) == id)
                    {
                        if (match != null && !ReferenceEquals(match, asset))
                            throw new InvalidOperationException($"Ambiguous {expected.Name} ID {id} in patch catalog.");
                        match = asset;
                    }

                if (match != null)
                    return match;
            }

            throw new InvalidOperationException($"Missing {expected.Name} ID {id} in GameInstance.");
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();

            public new bool Equals(object a, object b) => ReferenceEquals(a, b);

            public int GetHashCode(object value) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
        }
    }
}
