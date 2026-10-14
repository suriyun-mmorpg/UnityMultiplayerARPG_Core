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
        private Dictionary<string, RecordState> activeRecords = new Dictionary<string, RecordState>();
        private readonly Dictionary<string, RecordState> preparedRecords = new Dictionary<string, RecordState>();
        private Dictionary<string, ScriptableObject> activeRegistry = new Dictionary<string, ScriptableObject>();
        private Dictionary<string, DataPatchEntry[]> activeChunks = new Dictionary<string, DataPatchEntry[]>();
        private Dictionary<string, DataPatchEntry[]> preparedChunks;
        private Dictionary<ScriptableObject, object[]> activeCacheRoots = new Dictionary<ScriptableObject, object[]>(UnityObjectComparer.Instance);
        private readonly Dictionary<ScriptableObject, object[]> preparedCacheRoots = new Dictionary<ScriptableObject, object[]>(UnityObjectComparer.Instance);
        private bool sameRegistry;
        private bool registryRetainsPreviousAssets;

        private sealed class RecordState
        {
            public ScriptableObject target;
            public JObject input;
            public string[] warnings;
            public bool requiresRegistryRefresh;
        }
        public int PreparedChangeCount => changes.Count;
        public int CapturedDefaultCount => defaults.Count;
        public int LastPreparationRecordCount { get; private set; }
        public int LastCacheTraversalCount { get; private set; }
        public double LastRegistryMilliseconds { get; private set; }
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

        internal void RefreshRegistry()
        {
            var timer = Stopwatch.StartNew();
            registry = DataPatchRegistry.Runtime(database.database, database.scannedData);
            LastRegistryMilliseconds = timer.Elapsed.TotalMilliseconds;
        }

        public void Refresh()
        {
            RefreshRegistry();
            foreach (var entry in registry.Targets)
                if (!defaults.ContainsKey(entry.Key) && DataPatchSerializer.Fields(entry.Value.GetType()).Any())
                    defaults.Add(entry.Key, DataPatchSerializer.Export(entry.Value));
        }

        public async UniTask RefreshAsync(CancellationToken cancellationToken = default)
        {
            int version = preparationVersion;
            var frame = Stopwatch.StartNew();
            double budget = Math.Max(0.1, database.dataPatchSettings.preparationFrameBudgetMilliseconds);
            RefreshRegistry();
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
            LastPreparationRecordCount = 0;
            LastCacheTraversalCount = 0;
            var timer = Stopwatch.StartNew();
            try
            {
                RefreshRegistry();
                BeginRecordCache();
                ValidateScope(release);
                var overrides = ReadOverrides(release);
                foreach (var entry in PreparationTargets(overrides))
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

        public async UniTask PrepareAsync(DataPatchRelease release, CancellationToken cancellationToken = default, bool refreshRegistry = true)
        {
            Discard();
            PreparationWarnings.Clear();
            LastPreparationRecordCount = 0;
            LastCacheTraversalCount = 0;
            int version = preparationVersion;
            var timer = Stopwatch.StartNew();
            var frame = Stopwatch.StartNew();
            double budget = Math.Max(0.1, database.dataPatchSettings.preparationFrameBudgetMilliseconds);
            try
            {
                ValidateScope(release);
                // Startup can reuse the catalog registered before the menu opened.
                if (refreshRegistry || registry == null)
                    RefreshRegistry();
                // Only JSON and files are handled on the worker; Unity assets stay on the main thread.
                BeginRecordCache();
                var chunkCache = new Dictionary<string, DataPatchEntry[]>(activeChunks);
                var records = release == null ? Array.Empty<DataPatchEntry>() : await DataPatchHttp.RunBackground(() => DataPatchHttp.ReadEntries(release, chunkCache).ToArray(), cancellationToken);
                CheckPreparation(version, cancellationToken);
                var manifest = release == null ? null : DataPatchHttp.ValidateManifest(release);
                var hashes = new HashSet<string>(manifest?.chunks.Select(chunk => chunk.hash) ?? Enumerable.Empty<string>());
                preparedChunks = chunkCache.Where(pair => hashes.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);
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
                foreach (var entry in PreparationTargets(overrides))
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
            if (release != null && (release.databaseId != database.patchDatabaseId || (release.schemaVersion != 1 && release.schemaVersion != 2) || string.IsNullOrEmpty(release.publishTime)))
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
            if (entry.data == null || !registry.Targets.TryGetValue(key, out ScriptableObject target) ||
                !DataPatchSerializer.Fields(target.GetType()).Any() || overrides.ContainsKey(key))
                throw new InvalidOperationException($"Unknown or duplicate patch target: {key}");
            overrides.Add(key, entry.data);
        }

        private IEnumerable<KeyValuePair<string, ScriptableObject>> PreparationTargets(Dictionary<string, JObject> overrides)
        {
            // Previously touched records must still be visited when a later patch omits them.
            foreach (var entry in registry.Targets)
                if (overrides.ContainsKey(entry.Key) || defaults.ContainsKey(entry.Key))
                    yield return entry;
        }

        private void BeginRecordCache()
        {
            registryRetainsPreviousAssets = activeRegistry.All(pair =>
                registry.Targets.TryGetValue(pair.Key, out ScriptableObject current) && current == pair.Value);
            sameRegistry = registryRetainsPreviousAssets && activeRegistry.Count == registry.Targets.Count;
        }

        private void PrepareRecord(KeyValuePair<string, ScriptableObject> entry, Dictionary<string, JObject> overrides)
        {
            overrides.TryGetValue(entry.Key, out JObject input);
            if (activeRecords.TryGetValue(entry.Key, out RecordState previous) && previous.target == entry.Value &&
                (sameRegistry || registryRetainsPreviousAssets && !previous.requiresRegistryRefresh) && JToken.DeepEquals(previous.input, input))
            {
                preparedRecords[entry.Key] = previous;
                PreparationWarnings.AddRange(previous.warnings);
                return;
            }
            var state = new RecordState { target = entry.Value, input = input, warnings = Array.Empty<string>() };
            preparedRecords[entry.Key] = state;
            int warningStart = PreparationWarnings.Count;
            ++LastPreparationRecordCount;
            JObject current = DataPatchSerializer.Export(entry.Value);
            if (!defaults.TryGetValue(entry.Key, out JObject bundled))
            {
                // Capture before the first override, and reuse this export for the current comparison.
                bundled = current;
                defaults.Add(entry.Key, bundled);
            }
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
                    DataPatchSerializer.ReadInto(copy, data, registry, reportWarning: skipInvalidRecords ? message => Warn(entry.Key + ": " + message) : (Action<string>)null);
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
            finally
            {
                state.warnings = PreparationWarnings.Skip(warningStart).ToArray();
                state.requiresRegistryRefresh = state.warnings.Any(message => message.StartsWith("Skipped record ", StringComparison.Ordinal) || message.Contains("Skipped NPC shop row"));
            }
        }

        private void FinishPreparation(DataPatchRelease release, Stopwatch timer)
        {
            PreparedRelease = release;
            LastPreparationMilliseconds = timer.Elapsed.TotalMilliseconds;
            if (PreparationWarnings.Count > 0)
            {
                var details = PreparationWarnings.Where(message => message.StartsWith("Skipped record ", StringComparison.Ordinal) || message.Contains("Skipped NPC shop row")).Take(10);
                UnityEngine.Debug.LogWarning($"[Data Patch] Collected {PreparationWarnings.Count} preparation warnings. Full details: DataPatchRuntime.Engine.PreparationWarnings.\n" + string.Join("\n", details));
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"[Data Patch] Prepared {changes.Count} changed records in {LastPreparationMilliseconds:F1} ms across preparation frames; registry {LastRegistryMilliseconds:F1} ms, visited {LastPreparationRecordCount}/{registry.Targets.Count} records, visited {LastCacheTraversalCount} cache containers, retained {defaults.Count} rollback snapshots.");
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
            activeCacheRoots = new Dictionary<ScriptableObject, object[]>(preparedCacheRoots, UnityObjectComparer.Instance);
            activeRecords = new Dictionary<string, RecordState>(preparedRecords);
            activeRegistry = new Dictionary<string, ScriptableObject>(registry.Targets);
            if (preparedChunks != null)
                activeChunks = preparedChunks;
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
            if (changes.Count == 0 && preparedRecords.Count == 0)
                yield break;
            var copies = changes.ToDictionary(change => change.target, change => change.copy, UnityObjectComparer.Instance);
            var unique = new HashSet<object>(ReferenceComparer.Instance);
            foreach (ScriptableObject asset in registry.Targets.Values)
            {
                if (!copies.ContainsKey(asset) && activeCacheRoots.TryGetValue(asset, out object[] cached))
                {
                    preparedCacheRoots[asset] = cached;
                    foreach (object value in cached)
                        if (unique.Add(value))
                            yield return value;
                    continue;
                }
                var targets = new List<object>();
                foreach (object value in EnumerateCacheTargets(asset, copies, new HashSet<object>(ReferenceComparer.Instance)))
                {
                    if (IsCacheTarget(value))
                    {
                        targets.Add(value);
                        if (!unique.Add(value))
                            continue;
                    }
                    // Non-cache containers still yield while discovering a new or changed graph.
                    yield return value;
                }
                preparedCacheRoots[asset] = targets.ToArray();
            }
        }

        private static IEnumerable<object> EnumerateCacheTargets(object value, Dictionary<ScriptableObject, ScriptableObject> copies, HashSet<object> visited)
        {
            if (value == null)
                yield break;
            Type type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || !visited.Add(value))
                yield break;
            // Yield each container so even a large dependency graph can be budgeted across frames.
            yield return value;
            object source = value;
            if (value is ScriptableObject asset && copies.TryGetValue(asset, out ScriptableObject copy))
                source = copy;
            foreach (FieldInfo field in DataPatchSerializer.Fields(type))
            {
                Type element = field.FieldType.IsArray ? field.FieldType.GetElementType() :
                    field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>) ? field.FieldType.GetGenericArguments()[0] : null;
                if (element != null && (element.IsPrimitive || element.IsEnum || element == typeof(string)))
                    continue;
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
            ++LastCacheTraversalCount;
            if (IsCacheTarget(value))
                cacheTargets.Add(value);
        }

        private static bool IsCacheTarget(object value)
        {
            return value is IPatchableData || value is ItemDropManager || value is DamageInfo || value is EquipmentBonus;
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
            preparedRecords.Clear();
            preparedCacheRoots.Clear();
            preparedChunks = null;
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
