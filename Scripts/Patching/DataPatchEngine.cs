using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>Stages against bundled defaults and commits synchronously on Unity's main thread.</summary>
    public sealed class DataPatchEngine : IDisposable
    {
        public static uint Revision { get; private set; }

        private readonly DataPatchProfile database;
        private DataPatchRegistry registry;
        private readonly Dictionary<string, JObject> defaults = new Dictionary<string, JObject>();
        private readonly List<ScriptableObject> staging = new List<ScriptableObject>();
        private readonly List<(ScriptableObject target, ScriptableObject copy)> changes = new List<(ScriptableObject, ScriptableObject)>();
        public string ActiveReleaseId { get; private set; } = "";
        public DataPatchRelease PreparedRelease { get; private set; }

        public DataPatchEngine(DataPatchProfile database)
        {
            this.database = database;
            Refresh();
        }

        public void Refresh()
        {
            registry = DataPatchRegistry.Runtime(database.database, database.scannedData);
            foreach (var entry in registry.Targets)
                if (!defaults.ContainsKey(entry.Key) && DataPatchSerializer.Fields(entry.Value.GetType()).Any())
                    defaults.Add(entry.Key, DataPatchSerializer.Export(entry.Value));
        }

        public void Prepare(DataPatchRelease release)
        {
            Discard();
            Refresh();
            try
            {
                var overrides = new Dictionary<string, JObject>();
                if (release != null)
                {
                    if (release.databaseId != database.patchDatabaseId || release.environment != database.dataPatchSettings.environment || release.schemaVersion != 1 || string.IsNullOrEmpty(release.publishTime))
                        throw new InvalidOperationException("Patch scope, schema, or publication mismatch.");
                    DataPatchHttp.ValidateHash(release);
                    var entries = Newtonsoft.Json.JsonConvert.DeserializeObject<DataPatchEntry[]>(release.payloadJson);
                    if (entries == null || entries.Length > 20000)
                        throw new InvalidOperationException("Invalid patch entry count.");
                    foreach (DataPatchEntry entry in entries)
                    {
                        string key = DataPatchRegistry.Key(entry.dataType, entry.dataId);
                        if (entry.data == null || !defaults.ContainsKey(key) || overrides.ContainsKey(key))
                            throw new InvalidOperationException($"Unknown or duplicate patch target: {key}");
                        overrides.Add(key, entry.data);
                    }
                }

                foreach (var entry in registry.Targets)
                {
                    if (!defaults.TryGetValue(entry.Key, out JObject bundled))
                        continue;
                    ScriptableObject copy = UnityEngine.Object.Instantiate(entry.Value);
                    copy.hideFlags = HideFlags.HideAndDontSave;
                    staging.Add(copy);
                    DataPatchSerializer.ReadInto(copy, bundled, registry, false);
                    if (overrides.TryGetValue(entry.Key, out JObject data))
                        DataPatchSerializer.ReadInto(copy, data, registry);
                    changes.Add((entry.Value, copy));
                }

                PreparedRelease = release;
            }
            catch
            {
                Discard();
                throw;
            }
        }

        public void Commit()
        {
            // No awaits: transactions and drop rolls cannot interleave with this swap.
            foreach (var change in changes)
                foreach (FieldInfo field in DataPatchSerializer.Fields(change.target.GetType()))
                    field.SetValue(change.target, field.GetValue(change.copy));
            unchecked
            {
                ++Revision;
            }

            foreach (var entry in registry.Targets.Values)
                ClearCaches(entry, new HashSet<object>(ReferenceComparer.Instance));
            MemoryManager.CharacterItems.Clear();
            // Buff caches are not cleared here; normal idle expiration still applies.
            MemoryManager.CharacterSummons.Clear();
            ActiveReleaseId = PreparedRelease?.id ?? "";
            foreach (BasePlayerCharacterEntity player in UnityEngine.Object.FindObjectsByType<BasePlayerCharacterEntity>(FindObjectsSortMode.None))
            {
                try
                {
                    player.ForceMakeCaches();
                }
                catch (Exception ex)
                {
                    Debug.LogError("Player cache refresh failed after patch activation: " + ex.Message);
                }
            }

            Discard();
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();

            public new bool Equals(object left, object right) => ReferenceEquals(left, right);

            public int GetHashCode(object value) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
        }

        private static void ClearCaches(object value, HashSet<object> visited)
        {
            if (value == null || !visited.Add(value))
                return;
            Type type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
                return;
            if (value is IPatchableData patchableData)
                patchableData.ClearPatchCaches();
            // Inline data containers are not independently identified patch assets.
            if (value is ItemDropManager manager)
                manager.ClearPatchCaches();
            if (value is DamageInfo damageInfo)
                damageInfo.RefreshPatchCache();
            if (value is EquipmentBonus equipmentBonus)
                equipmentBonus.ClearPatchCaches();

            foreach (FieldInfo field in DataPatchSerializer.Fields(type))
            {
                object child = field.GetValue(value);
                if (child is System.Collections.IList list)
                    foreach (object item in list)
                        ClearCaches(item, visited);
                else if (!(child is UnityEngine.Object))
                    ClearCaches(child, visited);
            }
        }

        public void Discard()
        {
            changes.Clear();
            PreparedRelease = null;
            foreach (ScriptableObject copy in staging)
                UnityEngine.Object.DestroyImmediate(copy);
            staging.Clear();
        }

        public void Dispose()
        {
            Discard();
            if (ActiveReleaseId.Length > 0)
            {
                Prepare(null);
                Commit();
            }
        }
    }
}
