using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>Stages against bundled defaults and commits synchronously on Unity's main thread.</summary>
    public sealed class DataPatchEngine : IDisposable
    {
        public static uint Revision { get; private set; }

        private readonly DataPatchProfile database;
        private readonly bool skipInvalidRecords;
        public readonly List<string> PreparationWarnings = new List<string>();
        private DataPatchRegistry registry;
        private readonly Dictionary<string, JObject> defaults = new Dictionary<string, JObject>();
        private readonly List<ScriptableObject> staging = new List<ScriptableObject>();
        private readonly List<PreparedChange> changes = new List<PreparedChange>();
        private readonly List<object> cacheTargets = new List<object>();
        private int preparationVersion;
        public int PreparedChangeCount => changes.Count;
        public double LastPreparationMilliseconds { get; private set; }
        public double LastCommitMilliseconds { get; private set; }
        public string ActiveReleaseId { get; private set; } = "";
        public DataPatchRelease PreparedRelease { get; private set; }

        public DataPatchEngine(DataPatchProfile database, bool skipInvalidRecords = true, bool refresh = true)
        {
            this.database = database;
            this.skipInvalidRecords = skipInvalidRecords;
            if (refresh)
                Refresh();
        }

        public void Refresh()
        {
            registry = DataPatchRegistry.Runtime(database.database, database.scannedData);
            foreach (var entry in registry.Targets)
                if (!defaults.ContainsKey(entry.Key) && DataPatchSerializer.Fields(entry.Value.GetType()).Any())
                    defaults.Add(entry.Key, DataPatchSerializer.Export(entry.Value));
        }

        public async UniTask RefreshAsync(CancellationToken cancellationToken = default)
        {
            int version = preparationVersion;
            var frame = Stopwatch.StartNew();
            double budget = Math.Max(0.1, database.dataPatchSettings.preparationFrameBudgetMilliseconds);
            registry = DataPatchRegistry.Runtime(database.database, database.scannedData);
            foreach (var entry in registry.Targets)
            {
                CheckPreparation(version, cancellationToken);
                if (!defaults.ContainsKey(entry.Key) && DataPatchSerializer.Fields(entry.Value.GetType()).Any())
                    defaults.Add(entry.Key, DataPatchSerializer.Export(entry.Value));
                if (frame.Elapsed.TotalMilliseconds >= budget)
                {
                    await YieldPreparationFrame(cancellationToken);
                    frame.Restart();
                }
            }
            CheckPreparation(version, cancellationToken);
        }

        public void Prepare(DataPatchRelease release)
        {
            Discard();
            PreparationWarnings.Clear();
            var timer = Stopwatch.StartNew();
            try
            {
                Refresh();
                ValidateScope(release);
                var overrides = ReadOverrides(release);
                foreach (var entry in registry.Targets)
                    PrepareRecord(entry, overrides);
                foreach (object value in EnumerateCacheTargets())
                    AddCacheTarget(value);
                DestroyStaging();
                FinishPreparation(release, timer);
            }
            catch
            {
                Discard();
                throw;
            }
        }

        public async UniTask PrepareAsync(DataPatchRelease release, CancellationToken cancellationToken = default)
        {
            Discard();
            PreparationWarnings.Clear();
            int version = preparationVersion;
            var timer = Stopwatch.StartNew();
            var frame = Stopwatch.StartNew();
            double budget = Math.Max(0.1, database.dataPatchSettings.preparationFrameBudgetMilliseconds);
            try
            {
                ValidateScope(release);
                await RefreshAsync(cancellationToken);
                // Only JSON and files are handled on the worker; Unity assets stay on the main thread.
                var records = release == null ? Array.Empty<DataPatchEntry>() : await DataPatchHttp.RunBackground(() => DataPatchHttp.ReadEntries(release).ToArray(), cancellationToken);
                CheckPreparation(version, cancellationToken);
                var overrides = new Dictionary<string, JObject>();
                foreach (DataPatchEntry record in records)
                {
                    CheckPreparation(version, cancellationToken);
                    AddOverride(overrides, record);
                    if (frame.Elapsed.TotalMilliseconds >= budget)
                    {
                        await YieldPreparationFrame(cancellationToken);
                        frame.Restart();
                    }
                }
                frame.Restart();
                foreach (var entry in registry.Targets)
                {
                    CheckPreparation(version, cancellationToken);
                    PrepareRecord(entry, overrides);
                    if (frame.Elapsed.TotalMilliseconds >= budget)
                    {
                        await YieldPreparationFrame(cancellationToken);
                        frame.Restart();
                    }
                }
                foreach (object value in EnumerateCacheTargets())
                {
                    CheckPreparation(version, cancellationToken);
                    AddCacheTarget(value);
                    if (frame.Elapsed.TotalMilliseconds >= budget)
                    {
                        await YieldPreparationFrame(cancellationToken);
                        frame.Restart();
                    }
                }
                while (staging.Count > 0)
                {
                    CheckPreparation(version, cancellationToken);
                    int index = staging.Count - 1;
                    UnityEngine.Object.DestroyImmediate(staging[index]);
                    staging.RemoveAt(index);
                    if (frame.Elapsed.TotalMilliseconds >= budget)
                    {
                        await YieldPreparationFrame(cancellationToken);
                        frame.Restart();
                    }
                }
                CheckPreparation(version, cancellationToken);
                FinishPreparation(release, timer);
            }
            catch
            {
                if (preparationVersion == version)
                    Discard();
                throw;
            }
        }

        private static async UniTask YieldPreparationFrame(CancellationToken cancellationToken)
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                // Edit mode does not advance Time.frameCount, so NextFrame would never complete.
                await UniTask.Delay(1, DelayType.Realtime, cancellationToken: cancellationToken);
                return;
            }
#endif
            await UniTask.NextFrame(cancellationToken: cancellationToken);
        }

        private void CheckPreparation(int version, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (version != preparationVersion)
                throw new OperationCanceledException("Patch preparation was discarded.");
        }

        private void ValidateScope(DataPatchRelease release)
        {
            if (release != null && (release.databaseId != database.patchDatabaseId || release.environment != database.dataPatchSettings.environment || (release.schemaVersion != 1 && release.schemaVersion != 2) || string.IsNullOrEmpty(release.publishTime)))
                throw new InvalidOperationException("Patch scope, schema, or publication mismatch.");
        }

        private Dictionary<string, JObject> ReadOverrides(DataPatchRelease release)
        {
            var overrides = new Dictionary<string, JObject>();
            if (release != null)
                foreach (DataPatchEntry entry in DataPatchHttp.ReadEntries(release))
                    AddOverride(overrides, entry);
            return overrides;
        }

        private void AddOverride(Dictionary<string, JObject> overrides, DataPatchEntry entry)
        {
            if (entry == null)
                throw new InvalidOperationException("Null patch record.");
            string key = DataPatchRegistry.Key(entry.dataType, entry.dataId);
            if (entry.data == null || !defaults.ContainsKey(key) || overrides.ContainsKey(key))
                throw new InvalidOperationException($"Unknown or duplicate patch target: {key}");
            overrides.Add(key, entry.data);
        }

        private void PrepareRecord(KeyValuePair<string, ScriptableObject> entry, Dictionary<string, JObject> overrides)
        {
            if (!defaults.TryGetValue(entry.Key, out JObject bundled))
                return;
            JObject current = DataPatchSerializer.Export(entry.Value);
            if (!overrides.ContainsKey(entry.Key) && JToken.DeepEquals(current, bundled))
                return;
            ScriptableObject copy = UnityEngine.Object.Instantiate(entry.Value);
            copy.hideFlags = HideFlags.HideAndDontSave;
            staging.Add(copy);
            try
            {
                DataPatchSerializer.ReadInto(copy, bundled, registry, false);
                if (overrides.TryGetValue(entry.Key, out JObject data))
                {
                    if (skipInvalidRecords)
                        data = (JObject)PreserveNullValues(data, current, entry.Key);
                    DataPatchSerializer.ReadInto(copy, data, registry);
                }
                if (JToken.DeepEquals(current, DataPatchSerializer.Export(copy)))
                {
                    staging.Remove(copy);
                    UnityEngine.Object.DestroyImmediate(copy);
                    return;
                }
                FieldInfo[] fields = DataPatchSerializer.Fields(copy.GetType()).ToArray();
                changes.Add(new PreparedChange { target = entry.Value, copy = copy, fields = fields, values = fields.Select(field => field.GetValue(copy)).ToArray() });
            }
            catch (Exception ex) when (skipInvalidRecords && (ex is InvalidOperationException || ex is ArgumentException || ex is OverflowException || ex is Newtonsoft.Json.JsonException))
            {
                Warn($"Skipped record {entry.Key} ({entry.Value.name}); existing data retained. {ex.Message}");
                staging.Remove(copy);
                UnityEngine.Object.DestroyImmediate(copy);
            }
        }

        private void FinishPreparation(DataPatchRelease release, Stopwatch timer)
        {
            PreparedRelease = release;
            LastPreparationMilliseconds = timer.Elapsed.TotalMilliseconds;
            if (PreparationWarnings.Count > 0)
            {
                var details = PreparationWarnings.Where(message => message.StartsWith("Skipped record ", StringComparison.Ordinal)).Take(10);
                UnityEngine.Debug.LogWarning($"[Data Patch] Collected {PreparationWarnings.Count} preparation warnings. Full details: DataPatchRuntime.Engine.PreparationWarnings.\n" + string.Join("\n", details));
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"[Data Patch] Prepared {changes.Count} changed records in {LastPreparationMilliseconds:F1} ms across preparation frames.");
#endif
        }

        private sealed class PreparedChange
        {
            public ScriptableObject target;
            public ScriptableObject copy;
            public FieldInfo[] fields;
            public object[] values;
        }

        private JToken PreserveNullValues(JToken patch, JToken current, string path)
        {
            if (patch.Type == JTokenType.Null)
            {
                Warn($"Skipped null value at {path}; existing value retained.");
                return current?.DeepClone() ?? JValue.CreateNull();
            }
            if (patch is JObject source)
            {
                var result = new JObject();
                foreach (JProperty property in source.Properties())
                {
                    JToken previous = (current as JObject)?[property.Name];
                    JToken value = PreserveNullValues(property.Value, previous, path + "." + property.Name);
                    result.Add(property.Name, value);
                }
                return result;
            }
            if (patch is JArray array)
            {
                var result = new JArray();
                var previous = current as JArray;
                for (int i = 0; i < array.Count; ++i)
                {
                    if (array[i].Type == JTokenType.Null && (previous == null || i >= previous.Count))
                        throw new InvalidOperationException($"Null row at {path}[{i}] has no existing value to preserve.");
                    result.Add(PreserveNullValues(array[i], previous != null && i < previous.Count ? previous[i] : null, path + "[" + i + "]"));
                }
                return result;
            }
            return patch.DeepClone();
        }

        private void Warn(string message)
        {
            PreparationWarnings.Add(message);
        }

        public void Commit()
        {
            // No awaits: transactions and drop rolls cannot interleave with this swap.
            var timer = Stopwatch.StartNew();
            int changedCount = changes.Count;
            foreach (var change in changes)
                for (int i = 0; i < change.fields.Length; ++i)
                    change.fields[i].SetValue(change.target, change.values[i]);
            if (changedCount > 0)
            {
                unchecked
                {
                    ++Revision;
                }

                foreach (object value in cacheTargets)
                    ClearCache(value);
                MemoryManager.CharacterItems.Clear();
                // Buff caches are not cleared here; normal idle expiration still applies.
                MemoryManager.CharacterSummons.Clear();
                foreach (BasePlayerCharacterEntity player in UnityEngine.Object.FindObjectsByType<BasePlayerCharacterEntity>(FindObjectsSortMode.None))
                {
                    try
                    {
                        player.ForceMakeCaches();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError("Player cache refresh failed after patch activation: " + ex.Message);
                    }
                }
            }
            ActiveReleaseId = PreparedRelease?.id ?? "";
            Discard();
            LastCommitMilliseconds = timer.Elapsed.TotalMilliseconds;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"[Data Patch] Activated {changedCount} changed records in {LastCommitMilliseconds:F1} ms.");
#endif
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();

            public new bool Equals(object left, object right) => ReferenceEquals(left, right);

            public int GetHashCode(object value) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
        }

        private IEnumerable<object> EnumerateCacheTargets()
        {
            if (changes.Count == 0)
                yield break;
            var copies = changes.ToDictionary(change => change.target, change => change.copy, UnityObjectComparer.Instance);
            var visited = new HashSet<object>(ReferenceComparer.Instance);
            foreach (ScriptableObject asset in registry.Targets.Values)
                foreach (object value in EnumerateCacheTargets(asset, copies, visited))
                    yield return value;
        }

        private static IEnumerable<object> EnumerateCacheTargets(object value, Dictionary<ScriptableObject, ScriptableObject> copies, HashSet<object> visited)
        {
            if (value == null || !visited.Add(value))
                yield break;
            Type type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
                yield break;
            // Yield each container so even a large dependency graph can be budgeted across frames.
            yield return value;
            object source = value;
            if (value is ScriptableObject asset && copies.TryGetValue(asset, out ScriptableObject copy))
                source = copy;
            foreach (FieldInfo field in DataPatchSerializer.Fields(type))
            {
                object child = field.GetValue(source);
                if (child is System.Collections.IList list)
                {
                    foreach (object item in list)
                        foreach (object nested in EnumerateCacheTargets(item, copies, visited))
                            yield return nested;
                }
                else if (!(child is UnityEngine.Object))
                {
                    foreach (object nested in EnumerateCacheTargets(child, copies, visited))
                        yield return nested;
                }
            }
        }

        private void AddCacheTarget(object value)
        {
            if (value is IPatchableData || value is ItemDropManager || value is DamageInfo || value is EquipmentBonus)
                cacheTargets.Add(value);
        }

        private static void ClearCache(object value)
        {
            if (value is IPatchableData patchableData)
                patchableData.ClearPatchCaches();
            if (value is ItemDropManager manager)
                manager.ClearPatchCaches();
            if (value is DamageInfo damageInfo)
                damageInfo.RefreshPatchCache();
            if (value is EquipmentBonus equipmentBonus)
                equipmentBonus.ClearPatchCaches();
        }

        private sealed class UnityObjectComparer : IEqualityComparer<ScriptableObject>
        {
            public static readonly UnityObjectComparer Instance = new UnityObjectComparer();

            public bool Equals(ScriptableObject left, ScriptableObject right)
            {
                return left == right;
            }

            public int GetHashCode(ScriptableObject value)
            {
                return value.GetInstanceID();
            }
        }

        private void DestroyStaging()
        {
            foreach (ScriptableObject copy in staging)
                UnityEngine.Object.DestroyImmediate(copy);
            staging.Clear();
        }

        public void Discard()
        {
            ++preparationVersion;
            changes.Clear();
            cacheTargets.Clear();
            PreparedRelease = null;
            DestroyStaging();
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
