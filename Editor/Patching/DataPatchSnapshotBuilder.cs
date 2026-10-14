using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class DataPatchSnapshotBuilder
    {
        public static DataPatchRegistry CreateRegistry(DataPatchProfile profile, DataPatchBuildLog log)
        {
            if (profile == null || profile.database == null)
                throw new InvalidOperationException("Select a game database on the patch profile.");
            var registry = new DataPatchRegistry(false, (error, value) => log.Error("Discover data (skipped)", value as UnityEngine.Object, error));
            registry.AddGraph(profile.database);
            foreach (ScriptableObject asset in profile.scannedData ?? Array.Empty<ScriptableObject>())
                registry.AddGraph(asset);
            return registry;
        }

        public static DataPatchEntry ExportRecord(ScriptableObject asset, DataPatchRegistry registry, DataPatchBuildLog log)
        {
            DataPatchEntry entry;
            try
            {
                entry = new DataPatchEntry
                {
                    dataType = asset.GetType().Name,
                    dataId = DataPatchRegistry.DataId(asset),
                    data = DataPatchSerializer.Export(asset),
                };
            }
            catch (Exception ex)
            {
                log.Error("Export record (omitted)", asset, ex);
                return null;
            }
            if (entry.data.Count == 0)
                return null;
            ScriptableObject copy = null;
            try
            {
                copy = UnityEngine.Object.Instantiate(asset);
                DataPatchSerializer.ReadInto(copy, entry.data, registry);
            }
            catch (Exception ex)
            {
                log.Error("Validate record (included in snapshot)", asset, ex);
            }
            finally
            {
                if (copy != null)
                    UnityEngine.Object.DestroyImmediate(copy);
            }
            return entry;
        }

        public static async UniTask<DataPatchEntry[]> Build(DataPatchProfile profile, Action<int, int> progress, CancellationToken cancellationToken, ISet<string> dataTypes = null)
        {
            var log = new DataPatchBuildLog(profile);
            var collected = new List<DataPatchEntry>();
            try
            {
                // Let the dialog draw before preparing Unity assets on the main thread.
                await UniTask.Delay(1, DelayType.Realtime, cancellationToken: cancellationToken);
                var registry = CreateRegistry(profile, log);
                var targets = registry.Targets.Where(pair => dataTypes == null || dataTypes.Contains(pair.Value.GetType().Name)).OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray();
                var frame = Stopwatch.StartNew();
                for (int i = 0; i < targets.Length; ++i)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    DataPatchEntry entry = ExportRecord(targets[i].Value, registry, log);
                    if (entry != null)
                        collected.Add(entry);
                    progress?.Invoke(i + 1, targets.Length);
                    if (frame.Elapsed.TotalMilliseconds >= 2)
                    {
                        await UniTask.Delay(1, DelayType.Realtime, cancellationToken: cancellationToken);
                        frame.Restart();
                    }
                }
                log.Save($"Created {collected.Count} records. Validation errors are advisory during export; runtime validation still applies.");
                return collected.OrderBy(entry => entry.dataType, StringComparer.Ordinal).ThenBy(entry => entry.dataId).ToArray();
            }
            catch
            {
                log.Save($"Preparation did not finish. {collected.Count} collected records were not offered for upload.");
                throw;
            }
        }
    }
}
