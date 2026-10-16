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
        private readonly Dictionary<string, DefaultState> defaults = new Dictionary<string, DefaultState>();
        private readonly List<ScriptableObject> staging = new List<ScriptableObject>();
        private readonly List<PreparedChange> changes = new List<PreparedChange>();
        private readonly List<object> cacheTargets = new List<object>();
        private int preparationVersion;
        private bool progressivePreparation;
        private int progressiveChangedRecordCount;
        private readonly Dictionary<string, DefaultState> chunkUndo = new Dictionary<string, DefaultState>();
        private Dictionary<string, RecordState> activeRecords = new Dictionary<string, RecordState>();
        private readonly Dictionary<string, RecordState> preparedRecords = new Dictionary<string, RecordState>();
        private Dictionary<string, ScriptableObject> activeRegistry = new Dictionary<string, ScriptableObject>();
        private Dictionary<string, string[]> activeChunks = new Dictionary<string, string[]>();
        private Dictionary<string, string[]> preparedChunks;
        private Dictionary<ScriptableObject, object[]> activeCacheRoots = new Dictionary<ScriptableObject, object[]>(UnityObjectComparer.Instance);
        private readonly Dictionary<ScriptableObject, object[]> preparedCacheRoots = new Dictionary<ScriptableObject, object[]>(UnityObjectComparer.Instance);
        private bool sameRegistry;
        private bool registryRetainsPreviousAssets;

        private sealed class RecordState
        {
            public ScriptableObject target;
            public string inputHash;
            public string[] warnings;
            public bool requiresRegistryRefresh;
        }
        private sealed class DefaultState
        {
            public ScriptableObject target;
            public byte[] data;
        }

        private sealed class RecordInput
        {
            public DataPatchEntry entry;
            public string hash;
        }

        public long RetainedSnapshotBytes => defaults.Values.Sum(state => (long)state.data.Length);
        public int LastParsedChunkCount { get; private set; }
        public int LastMemoryCollectionCount { get; private set; }
        public long PeakPreparationManagedBytes { get; private set; }
        public int LastReusedChunkCount { get; private set; }
        public int LastReuseVerificationPassCount { get; private set; }
        public int LastAppliedChunkCount { get; private set; }
        public int PreparedChangeCount => changes.Count + progressiveChangedRecordCount;
        public long PendingUndoBytes => chunkUndo.Values.Sum(state => (long)state.data.Length);
        public int CapturedDefaultCount => defaults.Count;
        public int LastPreparationRecordCount { get; private set; }
        public int LastReusedCurrentExportCount { get; private set; }
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
                    CaptureDefault(entry.Key, entry.Value, DataPatchSerializer.Export(entry.Value));
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
                    CaptureDefault(entry.Key, entry.Value, DataPatchSerializer.Export(entry.Value));
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
            LastReusedCurrentExportCount = 0;
            LastCacheTraversalCount = 0;
            LastParsedChunkCount = 0;
            LastMemoryCollectionCount = 0;
            PeakPreparationManagedBytes = 0;
            LastReusedChunkCount = 0;
            LastReuseVerificationPassCount = 0;
            LastAppliedChunkCount = 0;
            var timer = Stopwatch.StartNew();
            try
            {
                RefreshRegistry();
                BeginRecordCache();
                ValidateScope(release);
                LogMemory("before preparation");
                preparedChunks = new Dictionary<string, string[]>();
                var seen = new HashSet<string>(StringComparer.Ordinal);
                DataPatchManifest manifest = release == null ? null : DataPatchHttp.ValidateManifest(release);
                if (release != null && manifest == null)
                    PrepareBatch(ReadBatch(release, null), seen);
                if (manifest != null)
                {
                    foreach (DataPatchChunk chunk in manifest.chunks)
                    {
                        if (CanReuseChunk(chunk, out string[] keys))
                        {
                            ++LastReuseVerificationPassCount;
                            DataPatchHttp.VerifyChunk(release, chunk);
                            ReuseChunk(chunk, keys, seen);
                        }
                        else
                            preparedChunks[chunk.hash] = PrepareBatch(ReadBatch(release, chunk), seen);
                    }
                }
                foreach (var entry in RestorationTargets(seen))
                    PrepareRecord(entry, null, null);
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

        public async UniTask PrepareAsync(DataPatchRelease release, CancellationToken cancellationToken = default, bool refreshRegistry = true, bool applyImmediately = false)
        {
            Discard();
            progressivePreparation = applyImmediately;
            PreparationWarnings.Clear();
            LastPreparationRecordCount = 0;
            LastReusedCurrentExportCount = 0;
            LastCacheTraversalCount = 0;
            LastParsedChunkCount = 0;
            LastMemoryCollectionCount = 0;
            PeakPreparationManagedBytes = 0;
            LastReusedChunkCount = 0;
            LastReuseVerificationPassCount = 0;
            LastAppliedChunkCount = 0;
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
                LogMemory("before preparation");
                // Use existing free heap space (especially in the Editor), keeping headroom for a record.
                long memoryCeiling = Math.Max(UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() + 8 * 1024 * 1024,
                    UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong() - 24 * 1024 * 1024);
                preparedChunks = new Dictionary<string, string[]>();
                var seen = new HashSet<string>(StringComparer.Ordinal);
                DataPatchManifest manifest = release == null ? null : DataPatchHttp.ValidateManifest(release);
                int batchCount = release == null ? 0 : manifest?.chunks.Length ?? 1;
                var cachedChunkKeys = new string[batchCount][];
                if (manifest != null)
                {
                    for (int i = 0; i < batchCount; ++i)
                    {
                        CheckPreparation(version, cancellationToken);
                        if (CanReuseChunk(manifest.chunks[i], out string[] keys))
                        {
                            cachedChunkKeys[i] = keys;
                            ++LastReusedChunkCount;
                        }
                    }
                    if (LastReusedChunkCount > 0)
                    {
                        // Verify files sequentially in one worker pass instead of returning to Unity per chunk.
                        ++LastReuseVerificationPassCount;
                        await DataPatchHttp.RunBackground(() =>
                        {
                            for (int i = 0; i < batchCount; ++i)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                if (cachedChunkKeys[i] != null)
                                    DataPatchHttp.VerifyChunk(release, manifest.chunks[i]);
                            }
                            return true;
                        }, cancellationToken);
                        CheckPreparation(version, cancellationToken);
                    }
                }
                UniTask<RecordInput[]> pendingBatch = default;
                bool hasPendingBatch = false;
                int nextBatchIndex = 0;
                void ReadAhead()
                {
                    while (nextBatchIndex < batchCount && cachedChunkKeys[nextBatchIndex] != null)
                        ++nextBatchIndex;
                    if (nextBatchIndex >= batchCount)
                        return;
                    CheckPreparation(version, cancellationToken);
                    pendingBatch = ReadBatchAsync(release, manifest?.chunks[nextBatchIndex], cancellationToken).Preserve();
                    hasPendingBatch = true;
                    ++nextBatchIndex;
                }
                try
                {
                    ReadAhead();
                    frame.Restart();
                    for (int batchIndex = 0; batchIndex < batchCount; ++batchIndex)
                    {
                        CheckPreparation(version, cancellationToken);
                        DataPatchChunk chunk = manifest?.chunks[batchIndex];
                        string[] cachedKeys = cachedChunkKeys[batchIndex];
                        if (cachedKeys != null)
                        {
                            foreach (string key in cachedKeys)
                            {
                                CheckPreparation(version, cancellationToken);
                                ReuseRecord(key, seen);
                                if (frame.Elapsed.TotalMilliseconds >= budget)
                                {
                                    await YieldPreparationFrame(cancellationToken);
                                    frame.Restart();
                                }
                            }
                            preparedChunks[chunk.hash] = cachedKeys;
                            continue;
                        }
                        RecordInput[] batch = await pendingBatch;
                        pendingBatch = default;
                        hasPendingBatch = false;
                        try
                        {
                            CheckPreparation(version, cancellationToken);
                            // At most the current chunk and one read-ahead chunk hold parsed JSON.
#if !UNITY_WEBGL || UNITY_EDITOR
                            ReadAhead();
#endif
                            ++LastParsedChunkCount;
                            if (LastParsedChunkCount == 1)
                                LogMemory("after first chunk parsed");
                            var keys = new string[batch.Length];
                            frame.Restart();
                            for (int i = 0; i < batch.Length; ++i)
                            {
                                CheckPreparation(version, cancellationToken);
                                keys[i] = PrepareInput(batch[i], seen);
                                batch[i].entry.data = null;
                                batch[i] = null;
                                if (frame.Elapsed.TotalMilliseconds >= budget)
                                {
                                    memoryCeiling = await MaintainPreparationMemory(memoryCeiling, version, cancellationToken);
                                    await YieldPreparationFrame(cancellationToken);
                                    frame.Restart();
                                }
                            }
                            if (chunk != null)
                                preparedChunks[chunk.hash] = keys;
                            if (progressivePreparation)
                                ApplyChunk();
                        }
                        finally
                        {
                            ClearBatch(batch);
                        }
                        memoryCeiling = await MaintainPreparationMemory(memoryCeiling, version, cancellationToken);
#if UNITY_WEBGL && !UNITY_EDITOR
                        // Web players have no worker threads; do not parse the next chunk early.
                        ReadAhead();
#endif
                    }
                }
                finally
                {
                    if (hasPendingBatch)
                    {
                        // Observe an in-flight read even after cancellation/failure and release its JSON.
                        try
                        {
                            ClearBatch(await pendingBatch);
                        }
                        catch (Exception)
                        {
                            // The preparation exception is reported by the outer catch.
                        }
                        pendingBatch = default;
                    }
                }
                frame.Restart();
                foreach (var entry in RestorationTargets(seen))
                {
                    CheckPreparation(version, cancellationToken);
                    PrepareRecord(entry, null, null);
                    if (progressivePreparation)
                        ApplyChunk();
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

        private async UniTask<long> MaintainPreparationMemory(long ceiling, int version, CancellationToken cancellationToken)
        {
            long used = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
            PeakPreparationManagedBytes = Math.Max(PeakPreparationManagedBytes, used);
            if (used < ceiling || !UnityEngine.Scripting.GarbageCollector.isIncremental ||
                UnityEngine.Scripting.GarbageCollector.GCMode != UnityEngine.Scripting.GarbageCollector.Mode.Enabled)
                return ceiling;
            ++LastMemoryCollectionCount;
            var timer = Stopwatch.StartNew();
            // Reclaim temporary records before processing more data expands the managed heap.
            for (int slice = 0; slice < 60 && timer.Elapsed.TotalMilliseconds < 1000; ++slice)
            {
                CheckPreparation(version, cancellationToken);
                if (UnityEngine.Scripting.GarbageCollector.GCMode != UnityEngine.Scripting.GarbageCollector.Mode.Enabled ||
                    !UnityEngine.Scripting.GarbageCollector.CollectIncremental(2000000))
                    break;
                await YieldPreparationFrame(cancellationToken);
            }
            CheckPreparation(version, cancellationToken);
            // Account for live definitions and the bounded read-ahead batch without collecting continuously.
            used = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
            return Math.Max(ceiling, used + 8 * 1024 * 1024);
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
            if (release?.schemaVersion == 2 && (release.chunkPaths == null ||
                release.chunkPaths.Length != DataPatchHttp.ValidateManifest(release).chunks.Length))
                throw new InvalidOperationException("Patch chunks are incomplete.");
        }

        private static UniTask<RecordInput[]> ReadBatchAsync(DataPatchRelease release, DataPatchChunk chunk, CancellationToken cancellationToken)
        {
            // Once a read starts, return its result even if canceled so the caller can clear it.
            return DataPatchHttp.RunBackground(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return ReadBatch(release, chunk);
            });
        }

        private static void ClearBatch(RecordInput[] batch)
        {
            foreach (RecordInput input in batch)
                if (input?.entry != null)
                    input.entry.data = null;
            Array.Clear(batch, 0, batch.Length);
        }

        private static RecordInput[] ReadBatch(DataPatchRelease release, DataPatchChunk chunk)
        {
            DataPatchEntry[] entries = chunk == null ? DataPatchHttp.ReadLegacyEntries(release) : DataPatchHttp.ReadChunkEntries(release, chunk);
            return entries.Select(entry => new RecordInput
            {
                entry = entry,
                hash = entry?.data == null ? null : DataPatchStorage.Fingerprint(entry.data),
            }).ToArray();
        }

        private string[] PrepareBatch(RecordInput[] batch, HashSet<string> seen)
        {
            ++LastParsedChunkCount;
            if (LastParsedChunkCount == 1)
                LogMemory("after first chunk parsed");
            try
            {
                return batch.Select(input => PrepareInput(input, seen)).ToArray();
            }
            finally
            {
                ClearBatch(batch);
            }
        }

        private string PrepareInput(RecordInput input, HashSet<string> seen)
        {
            DataPatchEntry record = input.entry;
            if (record == null)
                throw new InvalidOperationException("Null patch record.");
            string key = DataPatchRegistry.Key(record.dataType, record.dataId);
            if (record.data == null || !registry.Targets.TryGetValue(key, out ScriptableObject target) ||
                !DataPatchSerializer.Fields(target.GetType()).Any() || !seen.Add(key))
                throw new InvalidOperationException($"Unknown or duplicate patch target: {key}");
            PrepareRecord(new KeyValuePair<string, ScriptableObject>(key, target), record.data, input.hash);
            return key;
        }

        private bool CanReuseChunk(DataPatchChunk chunk, out string[] keys)
        {
            return activeChunks.TryGetValue(chunk.hash, out keys) && keys.Length == chunk.entryCount && keys.All(key =>
                key.StartsWith(chunk.dataType + ":", StringComparison.Ordinal) &&
                activeRecords.TryGetValue(key, out RecordState previous) &&
                registry.Targets.TryGetValue(key, out ScriptableObject target) && previous.target == target &&
                (sameRegistry || registryRetainsPreviousAssets && !previous.requiresRegistryRefresh));
        }

        private void ReuseChunk(DataPatchChunk chunk, string[] keys, HashSet<string> seen)
        {
            ++LastReusedChunkCount;
            foreach (string key in keys)
                ReuseRecord(key, seen);
            preparedChunks[chunk.hash] = keys;
        }

        private void ReuseRecord(string key, HashSet<string> seen)
        {
            if (!seen.Add(key))
                throw new InvalidOperationException($"Duplicate patch target: {key}");
            RecordState previous = activeRecords[key];
            preparedRecords[key] = previous;
            PreparationWarnings.AddRange(previous.warnings);
        }

        private IEnumerable<KeyValuePair<string, ScriptableObject>> RestorationTargets(HashSet<string> seen)
        {
            foreach (var entry in registry.Targets)
                if (!seen.Contains(entry.Key) && defaults.ContainsKey(entry.Key))
                    yield return entry;
        }

        private void CaptureDefault(string key, ScriptableObject target, JObject data)
        {
            defaults[key] = new DefaultState { target = target, data = DataPatchStorage.Compress(data) };
        }

        private void BeginRecordCache()
        {
            registryRetainsPreviousAssets = activeRegistry.All(pair =>
                registry.Targets.TryGetValue(pair.Key, out ScriptableObject current) && current == pair.Value);
            sameRegistry = registryRetainsPreviousAssets && activeRegistry.Count == registry.Targets.Count;
        }

        private void PrepareRecord(KeyValuePair<string, ScriptableObject> entry, JObject input, string inputHash)
        {
            if (activeRecords.TryGetValue(entry.Key, out RecordState previous) && previous.target == entry.Value &&
                (sameRegistry || registryRetainsPreviousAssets && !previous.requiresRegistryRefresh) && previous.inputHash == inputHash)
            {
                preparedRecords[entry.Key] = previous;
                PreparationWarnings.AddRange(previous.warnings);
                return;
            }
            var state = new RecordState { target = entry.Value, inputHash = inputHash, warnings = Array.Empty<string>() };
            preparedRecords[entry.Key] = state;
            int warningStart = PreparationWarnings.Count;
            ++LastPreparationRecordCount;
            // Full no-op exports already describe current values; do not allocate another tree.
            bool inputMatchesCurrent = input != null && DataPatchSerializer.MatchesExport(entry.Value, input);
            JObject current = inputMatchesCurrent ? input : DataPatchSerializer.Export(entry.Value);
            if (inputMatchesCurrent)
                ++LastReusedCurrentExportCount;
            bool hasSnapshot = defaults.TryGetValue(entry.Key, out DefaultState snapshot) && snapshot.target == entry.Value;
            JObject bundled = hasSnapshot ? DataPatchStorage.Read(snapshot.data) : current;
            if (input == null && JToken.DeepEquals(current, bundled))
                return;
            ScriptableObject copy = UnityEngine.Object.Instantiate(entry.Value);
            copy.hideFlags = HideFlags.HideAndDontSave;
            staging.Add(copy);
            try
            {
                if (!ReferenceEquals(current, bundled))
                    DataPatchSerializer.ReadInto(copy, bundled, registry, false);
                if (input != null)
                {
                    JObject data = input;
                    if (skipInvalidRecords)
                        data = (JObject)PreserveNullValues(data, current, entry.Key);
                    DataPatchSerializer.ReadInto(copy, data, registry, reportWarning: skipInvalidRecords ? message => Warn(entry.Key + ": " + message) : (Action<string>)null);
                }
                FieldInfo[] fields = DataPatchSerializer.Fields(copy.GetType())
                    .Where(field => !DataPatchSerializer.MatchesExport(field.GetValue(copy), current[field.Name], false, 1)).ToArray();
                if (fields.Length == 0)
                {
                    staging.Remove(copy);
                    UnityEngine.Object.DestroyImmediate(copy);
                    return;
                }
                // Rollback is only necessary once a record has a real change to retain.
                if (!hasSnapshot)
                    CaptureDefault(entry.Key, entry.Value, inputMatchesCurrent ? DataPatchSerializer.Export(entry.Value) : current);
                changes.Add(new PreparedChange { target = entry.Value, fields = fields, values = fields.Select(field => field.GetValue(copy)).ToArray() });
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
            LogMemory("after preparation");
            if (PreparationWarnings.Count > 0)
            {
                var details = PreparationWarnings.Where(message => message.StartsWith("Skipped record ", StringComparison.Ordinal) || message.Contains("Skipped NPC shop row")).Take(10);
                UnityEngine.Debug.LogWarning($"[Data Patch] Collected {PreparationWarnings.Count} preparation warnings. Full details: DataPatchRuntime.Engine.PreparationWarnings.\n" + string.Join("\n", details));
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"[Data Patch] Prepared {PreparedChangeCount} changed records in {LastPreparationMilliseconds:F1} ms across preparation frames; registry {LastRegistryMilliseconds:F1} ms, visited {LastPreparationRecordCount}/{registry.Targets.Count} records, visited {LastCacheTraversalCount} cache containers, retained {defaults.Count} compressed rollback snapshots ({RetainedSnapshotBytes / 1048576.0:F2} MiB), reused {LastReusedCurrentExportCount} current JSON exports, parsed {LastParsedChunkCount} chunks, reused {LastReusedChunkCount} chunks in {LastReuseVerificationPassCount} verification passes, sampled managed peak {PeakPreparationManagedBytes / 1048576.0:F1} MiB, incremental memory passes {LastMemoryCollectionCount}.");
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        internal void LogMemory(string phase)
        {
            long managedUsed = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
            long managedReserved = UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong();
            long unityUsed = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            long unityReserved = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
            string processMemory = "";
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    process.Refresh();
                    if (process.WorkingSet64 > 0 || process.PrivateMemorySize64 > 0)
                        processMemory = $", process working set {process.WorkingSet64 / 1048576.0:F1} MiB, private {process.PrivateMemorySize64 / 1048576.0:F1} MiB";
                }
            }
            catch (Exception)
            {
                // Some player platforms do not expose process counters.
            }
#endif
            UnityEngine.Debug.Log($"[Data Patch] Memory {phase}: managed used {managedUsed / 1048576.0:F1} MiB, reserved {managedReserved / 1048576.0:F1} MiB, Unity used {unityUsed / 1048576.0:F1} MiB, reserved {unityReserved / 1048576.0:F1} MiB, rollback bytes {RetainedSnapshotBytes / 1048576.0:F2} MiB, pending undo {PendingUndoBytes / 1048576.0:F2} MiB, staging copies {staging.Count}, pending changes {changes.Count}{processMemory}.");
        }

        private sealed class PreparedChange
        {
            public ScriptableObject target;
            public FieldInfo[] fields;
            public object[] values;
        }

        private JToken PreserveNullValues(JToken patch, JToken current, string path)
        {
            if (patch.Type == JTokenType.Null)
            {
                Warn($"Skipped null value at {PatchValuePath(path, patch)}; existing value retained.");
                return current?.DeepClone() ?? JValue.CreateNull();
            }
            // The parsed input belongs only to this batch and is cleared after processing.
            // Reuse its containers; only null replacements require a copy of current data.
            if (patch is JObject source)
            {
                foreach (JProperty property in source.Properties())
                {
                    JToken previous = (current as JObject)?[property.Name];
                    JToken value = PreserveNullValues(property.Value, previous, path);
                    if (!ReferenceEquals(value, property.Value))
                        property.Value = value;
                }
                return source;
            }
            if (patch is JArray array)
            {
                var previous = current as JArray;
                for (int i = 0; i < array.Count; ++i)
                {
                    if (array[i].Type == JTokenType.Null && (previous == null || i >= previous.Count))
                        throw new InvalidOperationException($"Null row at {PatchValuePath(path, array[i])} has no existing value to preserve.");
                    JToken value = PreserveNullValues(array[i], previous != null && i < previous.Count ? previous[i] : null, path);
                    if (!ReferenceEquals(value, array[i]))
                        array[i] = value;
                }
                return array;
            }
            return patch;
        }

        private static string PatchValuePath(string root, JToken value)
        {
            // Build a diagnostic path only for warnings/errors, not for every valid primitive.
            string path = value.Path;
            return path.Length == 0 ? root : root + "." + path;
        }

        private void Warn(string message)
        {
            PreparationWarnings.Add(message);
        }

        public void Commit()
        {
            // Runtime chunks are already live. Staged tool callers still swap without yielding.
            var timer = Stopwatch.StartNew();
            int changedCount = PreparedChangeCount;
            if (!progressivePreparation)
                foreach (var change in changes)
                    for (int i = 0; i < change.fields.Length; ++i)
                        change.fields[i].SetValue(change.target, change.values[i]);
            if (changedCount > 0 && !progressivePreparation)
            {
                RefreshGameplayCaches();
            }
            activeCacheRoots = new Dictionary<ScriptableObject, object[]>(preparedCacheRoots, UnityObjectComparer.Instance);
            activeRecords = new Dictionary<string, RecordState>(preparedRecords);
            activeRegistry = new Dictionary<string, ScriptableObject>(registry.Targets);
            if (preparedChunks != null)
                activeChunks = preparedChunks;
            ActiveReleaseId = PreparedRelease?.id ?? "";
            // Chunks are already live; commit finalizes the release identity and forgets undo.
            chunkUndo.Clear();
            progressivePreparation = false;
            progressiveChangedRecordCount = 0;
            Discard();
            LastCommitMilliseconds = timer.Elapsed.TotalMilliseconds;
            LogMemory("after activation");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"[Data Patch] Activated {changedCount} changed records in {LastCommitMilliseconds:F1} ms.");
#endif
        }

        private void ApplyChunk()
        {
            if (changes.Count == 0)
                return;
            foreach (PreparedChange change in changes)
            {
                string key = DataPatchRegistry.Key(change.target.GetType().Name, DataPatchRegistry.DataId(change.target));
                if (!chunkUndo.ContainsKey(key))
                {
                    JObject current = DataPatchSerializer.Export(change.target);
                    var original = new JObject();
                    foreach (FieldInfo field in change.fields)
                        original.Add(field.Name, current[field.Name].DeepClone());
                    chunkUndo[key] = new DefaultState { target = change.target, data = DataPatchStorage.Compress(original) };
                }
                for (int i = 0; i < change.fields.Length; ++i)
                    change.fields[i].SetValue(change.target, change.values[i]);
            }
            // Resolve cache containers from the live values, not copies that will be destroyed.
            foreach (object value in EnumerateCacheTargets(liveValues: true))
                AddCacheTarget(value);
            RefreshGameplayCaches();
            activeCacheRoots = new Dictionary<ScriptableObject, object[]>(preparedCacheRoots, UnityObjectComparer.Instance);
            progressiveChangedRecordCount += changes.Count;
            ++LastAppliedChunkCount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"[Data Patch] Applied chunk {LastAppliedChunkCount}: {changes.Count} changed records are live; pending undo {PendingUndoBytes / 1048576.0:F2} MiB.");
#endif
            changes.Clear();
            cacheTargets.Clear();
            DestroyStaging();
        }

        private void UndoChunks()
        {
            if (chunkUndo.Count == 0)
                return;
            foreach (DefaultState undo in chunkUndo.Values)
                if (undo.target != null)
                    DataPatchSerializer.ReadInto(undo.target, DataPatchStorage.Read(undo.data), registry, false);
            activeCacheRoots.Clear();
            preparedCacheRoots.Clear();
            changes.Clear();
            cacheTargets.Clear();
            foreach (object value in EnumerateCacheTargets(forceRefresh: true, liveValues: true))
                AddCacheTarget(value);
            RefreshGameplayCaches();
        }

        private void RefreshGameplayCaches()
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

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();

            public new bool Equals(object left, object right) => ReferenceEquals(left, right);

            public int GetHashCode(object value) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
        }

        private IEnumerable<object> EnumerateCacheTargets(bool forceRefresh = false, bool liveValues = false)
        {
            if (changes.Count == 0 && !forceRefresh)
            {
                var retained = new HashSet<ScriptableObject>(registry.Targets.Values, UnityObjectComparer.Instance);
                foreach (var cached in activeCacheRoots)
                    if (retained.Contains(cached.Key))
                        preparedCacheRoots[cached.Key] = cached.Value;
                yield break;
            }
            var pending = changes.ToDictionary(change => change.target, UnityObjectComparer.Instance);
            var unique = new HashSet<object>(ReferenceComparer.Instance);
            foreach (ScriptableObject asset in registry.Targets.Values)
            {
                if (!pending.ContainsKey(asset) && activeCacheRoots.TryGetValue(asset, out object[] cached))
                {
                    preparedCacheRoots[asset] = cached;
                    foreach (object value in cached)
                        if (unique.Add(value))
                            yield return value;
                    continue;
                }
                var targets = new List<object>();
                foreach (object value in EnumerateCacheTargets(asset, pending, new HashSet<object>(ReferenceComparer.Instance), liveValues))
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

        private static IEnumerable<object> EnumerateCacheTargets(object value, Dictionary<ScriptableObject, PreparedChange> pending, HashSet<object> visited, bool liveValues)
        {
            if (value == null)
                yield break;
            Type type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || !visited.Add(value))
                yield break;
            // Yield each container so even a large dependency graph can be budgeted across frames.
            yield return value;
            PreparedChange change = null;
            if (!liveValues && value is ScriptableObject asset)
                pending.TryGetValue(asset, out change);
            foreach (FieldInfo field in DataPatchSerializer.Fields(type))
            {
                Type element = field.FieldType.IsArray ? field.FieldType.GetElementType() :
                    field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>) ? field.FieldType.GetGenericArguments()[0] : null;
                if (element != null && (element.IsPrimitive || element.IsEnum || element == typeof(string)))
                    continue;
                int index = change == null ? -1 : Array.IndexOf(change.fields, field);
                object child = index >= 0 ? change.values[index] : field.GetValue(value);
                if (child is System.Collections.IList list)
                {
                    foreach (object item in list)
                        foreach (object nested in EnumerateCacheTargets(item, pending, visited, liveValues))
                            yield return nested;
                }
                else if (!(child is UnityEngine.Object))
                {
                    foreach (object nested in EnumerateCacheTargets(child, pending, visited, liveValues))
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
            try
            {
                UndoChunks();
            }
            finally
            {
                chunkUndo.Clear();
                progressivePreparation = false;
                progressiveChangedRecordCount = 0;
                changes.Clear();
                cacheTargets.Clear();
                preparedRecords.Clear();
                preparedCacheRoots.Clear();
                preparedChunks = null;
                PreparedRelease = null;
                DestroyStaging();
            }
        }

        public void Dispose()
        {
            Discard();
            if (ActiveReleaseId.Length > 0)
            {
                Prepare(null);
                Commit();
            }
            defaults.Clear();
            activeRecords.Clear();
            activeRegistry.Clear();
            activeChunks.Clear();
            activeCacheRoots.Clear();
            PreparationWarnings.Clear();
        }
    }
}
