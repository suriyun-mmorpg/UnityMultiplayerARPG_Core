using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Cysharp.Text;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG
{
    public static class DataPatchVerification
    {
        [Serializable]
        private sealed class ExportComparisonFixture
        {
            public int integer = 123;
            public long largeInteger = long.MaxValue - 7;
            public decimal exactDecimal = 12345678901234567890.12345678m;
            public float fractional = 0.125f;
            public double precise = double.MaxValue;
            public char character = 'x';
            public string dateLikeText = "2026-10-15T12:00:00Z";
            public Vector2 vector2 = new Vector2(1, 2);
            public Vector3 vector3 = new Vector3(1, 2, 3);
            public Vector2Int vector2Int = new Vector2Int(1, 2);
            public Vector3Int vector3Int = new Vector3Int(1, 2, 3);
            public Rect rectangle = new Rect(1, 2, 3, 4);
            public Bounds bounds = new Bounds(Vector3.one, Vector3.one * 2);
            public LayerMask mask = 123;
            public int[] array = new[] { 1, 2, 3 };
            public System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string> { "a", null, "b" };
            public ItemAmount[] references;
            public DamageType enumeration = DamageType.Melee;
        }

        public static void VerifyExportComparison()
        {
            var compare = (Func<object, JToken, bool, int, bool>)Delegate.CreateDelegate(
                typeof(Func<object, JToken, bool, int, bool>), typeof(DataPatchSerializer).GetMethod(
                    "MatchesExport", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
            var fixture = new ExportComparisonFixture();
            var item = ScriptableObject.CreateInstance<JunkItem>();
            item.Id = "comparison-fixture-" + Guid.NewGuid().ToString("N");
            fixture.references = new[] { new ItemAmount { item = item, amount = 10 } };
            try
            {
                JObject snapshot = DataPatchSerializer.Export(fixture);
                Check(compare(fixture, snapshot, true, 0), "Direct comparison rejected an identical export.");
                var candidates = new JToken[] { JValue.CreateNull(), new JValue(false), new JValue(1L), new JValue(1d),
                    new JValue(1m), new JValue("text"), new JObject(), new JArray(), new JObject { ["dataId"] = item.DataId } };
                foreach (var field in DataPatchSerializer.Fields(fixture.GetType()))
                {
                    object value = field.GetValue(fixture);
                    Check(compare(value, snapshot[field.Name], false, 1), "Direct comparison differs for " + field.Name);
                    foreach (JToken candidate in candidates)
                    {
                        bool equal;
                        try
                        {
                            equal = JToken.DeepEquals(snapshot[field.Name], candidate);
                        }
                        catch (OverflowException)
                        {
                            equal = false;
                        }
                        Check(compare(value, candidate, false, 1) == equal,
                            "Direct comparison changed JSON equality for " + field.Name);
                    }
                }
                JObject reordered = new JObject();
                foreach (var property in System.Linq.Enumerable.Reverse(snapshot.Properties()))
                    reordered.Add(property.Name, property.Value.DeepClone());
                Check(compare(fixture, reordered, true, 0), "Property order changed export equality.");
                snapshot["integer"] = 124;
                Check(!compare(fixture, snapshot, true, 0), "Direct comparison missed a changed primitive.");
                snapshot.Remove("integer");
                Check(!compare(fixture, snapshot, true, 0), "Direct comparison accepted a partial export as full equality.");
                Debug.Log("DATA_PATCH_DIRECT_EXPORT_COMPARISON_PASSED");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(item);
            }
        }

        public static void Run()
        {
            try
            {
                Verify();
                Debug.Log("DATA_PATCH_VERIFICATION_PASSED");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
        }

        public static void RunPreparationPerformance()
        {
            RunPreparationPerformanceAsync().Forget();
        }

        private static async Cysharp.Threading.Tasks.UniTask RunPreparationPerformanceAsync()
        {
            try
            {
                await VerifyNpcShopPatching();
                VerifyItemCategoryValidation();
                VerifyTolerantRecords();
                VerifyRegistrySecondaryLookups();
                VerifyChunkedTransport();
                VerifyExportComparison();
                await VerifyPreparationPerformance();
                await VerifyStartupPreparation();
                await VerifyProgressivePatchLoading();
                await VerifyCompactPatchMemory();
                await VerifyChunkReloadScheduling();
                await VerifyRuntimeLoadingEvents();
                VerifyPrimitiveExportAllocations();
                VerifyRegistryTraversal();
                Debug.Log("DATA_PATCH_PREPARATION_PERFORMANCE_PASSED");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
        }

        public static async Cysharp.Threading.Tasks.UniTask VerifyNpcShopPatching()
        {
            var item = ScriptableObject.CreateInstance<JunkItem>();
            var shop = ScriptableObject.CreateInstance<NpcDialog>();
            var database = ScriptableObject.CreateInstance<GameDatabase>();
            var profile = ScriptableObject.CreateInstance<DataPatchProfile>();
            item.Id = "patch-shop-item-" + Guid.NewGuid().ToString("N");
            shop.name = "patch-shop-dialog-" + Guid.NewGuid().ToString("N");
            shop.type = NpcDialogType.Shop;
            shop.sellItems = new[]
            {
                new NpcSellItem { item = item, level = 1, amount = 1, sellPrice = 10 },
                new NpcSellItem { item = null, level = 1, amount = 1, sellPrice = 40 },
            };
            typeof(ItemCraft).GetField("amount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(shop.itemCraft, 0);
            database.items = new BaseItem[] { item };
            profile.database = database;
            profile.patchDatabaseId = "shop-patch-verification";
            profile.scannedData = new ScriptableObject[] { shop };
            try
            {
                using (var engine = new DataPatchEngine(profile, refresh: false))
                {
                    JObject snapshot = DataPatchSerializer.Export(shop);
                    Check(snapshot["sellItems"][0]["inspectorTitle"] == null && snapshot["sellItems"][0]["unlockableGameData"] == null, "NPC inspector-only controls leaked into patch export.");
                    // Existing uploaded releases must remain readable by players without UNITY_EDITOR.
                    snapshot["sellItems"][0]["inspectorTitle"] = "legacy editor title";
                    snapshot["sellItems"][0]["unlockableGameData"] = JValue.CreateNull();
                    snapshot["sellItems"][0]["sellPrice"] = 1000;
                    snapshot["sellItems"][1]["sellPrice"] = 2000;
                    await engine.PrepareAsync(Release(profile, "shop-price", new[]
                    {
                        new DataPatchEntry { dataType = nameof(NpcDialog), dataId = shop.DataId, data = snapshot },
                    }));
                    Check(engine.PreparedChangeCount == 1 && !engine.PreparationWarnings.Exists(warning => warning.StartsWith("Skipped record ", StringComparison.Ordinal)), "Shop price snapshot was skipped: " + string.Join("; ", engine.PreparationWarnings));
                    Check(shop.sellItems[0].sellPrice == 10, "Shop price changed before activation.");
                    engine.Commit();
                    Check(shop.sellItems[0].sellPrice == 1000 && shop.itemCraft.Amount == 1, "Shop price or crafting fallback was not applied correctly.");
                    Check(shop.sellItems.Length == 2 && shop.sellItems[1].item == null && shop.sellItems[1].sellPrice == 40, "Broken shop row was changed or its slot/index was removed.");
                    Check(engine.PreparationWarnings.Exists(warning => warning.Contains("Skipped NPC shop row [1]")), "Broken shop row was not reported.");
                    var allItems = typeof(NpcDialog).GetProperty("AllSellItems")?.GetValue(shop) as NpcSellItem[];
                    Check(allItems == null || allItems.Length == 2 && allItems[0].sellPrice == 1000 && allItems[1].sellPrice == 40, "NPC purchase/UI cache retained the old shop price.");
                    Check(GameInstance.NpcDialogs[shop.DataId] == shop, "Shop patch replaced the runtime dialog identity.");

                    JObject invalid = snapshot.DeepClone() as JObject;
                    invalid["itemCraft"]["amount"] = -1;
                    await engine.PrepareAsync(Release(profile, "shop-invalid", new[]
                    {
                        new DataPatchEntry { dataType = nameof(NpcDialog), dataId = shop.DataId, data = invalid },
                    }));
                    Check(engine.PreparedChangeCount == 0 && engine.PreparationWarnings.Exists(warning => warning.StartsWith("Skipped record ", StringComparison.Ordinal)), "Negative crafting amount was accepted in a shop patch.");
                    engine.Commit();
                    Check(shop.sellItems[0].sellPrice == 1000, "Invalid shop snapshot replaced the active price.");
                    await engine.PrepareAsync(null);
                    engine.Commit();
                    Check(shop.sellItems[0].sellPrice == 10, "Shop price baseline was not restored.");
                    allItems = typeof(NpcDialog).GetProperty("AllSellItems")?.GetValue(shop) as NpcSellItem[];
                    Check(allItems == null || allItems[0].sellPrice == 10, "NPC purchase/UI cache did not restore the baseline price.");
                }
            }
            finally
            {
                if (GameInstance.Items.TryGetValue(item.DataId, out BaseItem registeredItem) && registeredItem == item)
                    GameInstance.Items.Remove(item.DataId);
                if (GameInstance.NpcDialogs.TryGetValue(shop.DataId, out BaseNpcDialog registeredShop) && registeredShop == shop)
                    GameInstance.NpcDialogs.Remove(shop.DataId);
                UnityEngine.Object.DestroyImmediate(shop);
                UnityEngine.Object.DestroyImmediate(item);
                UnityEngine.Object.DestroyImmediate(database);
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        public static async Cysharp.Threading.Tasks.UniTask VerifyPreparationPerformance()
        {
            GameInstance.ClearData();
            var database = ScriptableObject.CreateInstance<GameDatabase>();
            var profile = ScriptableObject.CreateInstance<DataPatchProfile>();
            var table = ScriptableObject.CreateInstance<ItemDropTable>();
            var items = new JunkItem[96];
            int updates = 0;
            EditorApplication.CallbackFunction countUpdate = () => ++updates;
            EditorApplication.update += countUpdate;
            try
            {
                for (int i = 0; i < items.Length; ++i)
                {
                    items[i] = ScriptableObject.CreateInstance<JunkItem>();
                    items[i].Id = "async-patch-test-" + i;
                    items[i].SellPrice = 10;
                }
                database.items = items;
                profile.database = database;
                profile.patchDatabaseId = "async-patch-test";
                profile.dataPatchSettings.preparationFrameBudgetMilliseconds = 0.1f;
                table.name = "async-patch-drops";
                table.randomItems = new[] { new ItemDrop { item = items[0], minLevel = 1, maxLevel = 1, minAmount = 1, maxAmount = 1, dropRate = 1 } };
                profile.scannedData = new ScriptableObject[] { table };
                var drops = new ItemDropManager { itemDropTables = new[] { table } };
                using (var engine = new DataPatchEngine(profile, false, refresh: false))
                {
                    var release = Release(profile, "async-first", new[]
                    {
                        new DataPatchEntry { dataType = nameof(JunkItem), dataId = items[0].DataId, data = JObject.Parse("{\"sellPrice\":1000}") },
                        new DataPatchEntry { dataType = nameof(ItemDropTable), dataId = table.DataId, data = JObject.Parse("{\"randomItems\":[{\"item\":{\"dataId\":" + items[0].DataId + "},\"minLevel\":1,\"maxLevel\":1,\"minAmount\":4,\"maxAmount\":4,\"dropRate\":1}]}") },
                    });
                    _ = drops.CacheRandomItems;
                    int before = updates;
                    var preparation = engine.PrepareAsync(release).Preserve();
                    await Cysharp.Threading.Tasks.UniTask.Delay(1, Cysharp.Threading.Tasks.DelayType.Realtime);
                    Check(items[0].SellPrice == 10, "Async preparation changed live values before activation.");
                    await preparation;
                    Check(updates > before, "Async preparation did not yield editor frames.");
                    Check(engine.PreparedChangeCount == 2, "Unchanged assets were included in the activation.");
                    Check(items[0].SellPrice == 10, "Completed preparation changed live values.");
                    engine.Commit();
                    Check(items[0].SellPrice == 1000 && drops.CacheRandomItems[0].maxAmount == 4, "Async activation failed to update fields or drop caches.");
                    uint revision = DataPatchEngine.Revision;
                    await engine.PrepareAsync(release);
                    Check(engine.PreparedChangeCount == 0, "Identical release was prepared as changes.");
                    engine.Commit();
                    Check(DataPatchEngine.Revision == revision, "No-op activation invalidated gameplay caches.");
                    var rollback = Release(profile, "async-rollback", Array.Empty<DataPatchEntry>());
                    await engine.PrepareAsync(rollback);
                    engine.Commit();
                    Check(items[0].SellPrice == 10 && drops.CacheRandomItems[0].maxAmount == 1, "Absent overrides did not restore defaults or invalidate dependent caches.");
                    using (var cancellation = new System.Threading.CancellationTokenSource())
                    {
                        var pending = engine.PrepareAsync(release, cancellation.Token).Preserve();
                        await Cysharp.Threading.Tasks.UniTask.Delay(1, Cysharp.Threading.Tasks.DelayType.Realtime);
                        cancellation.Cancel();
                        bool cancelled = false;
                        try
                        {
                            await pending;
                        }
                        catch (OperationCanceledException)
                        {
                            cancelled = true;
                        }
                        Check(cancelled && engine.PreparedChangeCount == 0 && items[0].SellPrice == 10, "Cancelled preparation left staged or live changes.");
                    }
                    release.payloadHash = "tampered";
                    bool rejected = false;
                    try
                    {
                        await engine.PrepareAsync(release);
                    }
                    catch (InvalidOperationException)
                    {
                        rejected = true;
                    }
                    Check(Cysharp.Threading.Tasks.PlayerLoopHelper.IsMainThread, "Worker failure resumed cleanup off the main thread.");
                    Check(rejected && items[0].SellPrice == 10 && engine.ActiveReleaseId == rollback.id, "Invalid integrity partially activated.");
                    Debug.Log($"DATA_PATCH_ASYNC_NOOP_ROLLBACK_CANCEL_PASSED; last commit {engine.LastCommitMilliseconds:F1} ms");
                }
            }
            finally
            {
                EditorApplication.update -= countUpdate;
                GameInstance.ClearData();
                foreach (var item in items)
                    if (item != null)
                        UnityEngine.Object.DestroyImmediate(item);
                UnityEngine.Object.DestroyImmediate(table);
                UnityEngine.Object.DestroyImmediate(profile);
                UnityEngine.Object.DestroyImmediate(database);
            }
        }

        public static async UniTask VerifyProgressivePatchLoading()
        {
            var database = ScriptableObject.CreateInstance<GameDatabase>();
            var profile = ScriptableObject.CreateInstance<DataPatchProfile>();
            var items = new JunkItem[2];
            string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "unity-progressive-patch-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(directory);
            DataPatchRelease MakeRelease(string id, int firstPrice, int secondPrice)
            {
                var chunks = new DataPatchChunk[2];
                var paths = new string[2];
                for (int i = 0; i < chunks.Length; ++i)
                {
                    string payload = JsonConvert.SerializeObject(new[]
                    {
                        new DataPatchEntry { dataType = nameof(JunkItem), dataId = items[i].DataId, data = new JObject { ["sellPrice"] = i == 0 ? firstPrice : secondPrice } },
                    });
                    paths[i] = System.IO.Path.Combine(directory, id + "-" + i + ".json");
                    System.IO.File.WriteAllText(paths[i], payload, new System.Text.UTF8Encoding(false));
                    chunks[i] = new DataPatchChunk { index = i, dataType = nameof(JunkItem), hash = DataPatchHttp.Hash(payload), bytes = System.Text.Encoding.UTF8.GetByteCount(payload), entryCount = 1 };
                }
                string manifest = JsonConvert.SerializeObject(new DataPatchManifest { chunks = chunks });
                return new DataPatchRelease { id = id, databaseId = profile.patchDatabaseId, schemaVersion = 2, publishTime = "published", manifestJson = manifest, payloadHash = DataPatchHttp.Hash(manifest), chunkPaths = paths };
            }
            try
            {
                for (int i = 0; i < items.Length; ++i)
                {
                    items[i] = ScriptableObject.CreateInstance<JunkItem>();
                    items[i].Id = "progressive-item-" + Guid.NewGuid().ToString("N");
                    items[i].SellPrice = 10;
                }
                database.items = items;
                profile.database = database;
                profile.patchDatabaseId = "progressive-test";
                profile.dataPatchSettings.preparationFrameBudgetMilliseconds = 0.1f;
                using (var engine = new DataPatchEngine(profile, false, refresh: false))
                {
                    await engine.PrepareAsync(MakeRelease("previous", 50, 60));
                    engine.Commit();
                    int originalCopies = System.Linq.Enumerable.Count(Resources.FindObjectsOfTypeAll<JunkItem>(), item => item.hideFlags == HideFlags.HideAndDontSave);
                    await engine.PrepareAsync(MakeRelease("next", 100, 200), applyImmediately: true);
                    Check(items[0].SellPrice == 100 && items[1].SellPrice == 200 && engine.LastAppliedChunkCount == 2, "Chunk values did not become live before release confirmation.");
                    Check(engine.ActiveReleaseId == "previous" && engine.PendingUndoBytes > 0, "Release identity changed before confirmation or undo was discarded.");
                    Check(System.Linq.Enumerable.Count(Resources.FindObjectsOfTypeAll<JunkItem>(), item => item.hideFlags == HideFlags.HideAndDontSave) == originalCopies, "Progressive loading retained native staging copies.");
                    engine.Discard();
                    Check(items[0].SellPrice == 50 && items[1].SellPrice == 60 && engine.PendingUndoBytes == 0, "Discard restored bundled data instead of the previously active values.");
                    var failed = MakeRelease("failed", 300, 400);
                    System.IO.File.WriteAllText(failed.chunkPaths[1], "[]");
                    bool rejected = false;
                    try
                    {
                        await engine.PrepareAsync(failed, applyImmediately: true);
                    }
                    catch (System.IO.IOException)
                    {
                        rejected = true;
                    }
                    Check(rejected && engine.LastAppliedChunkCount == 1 && items[0].SellPrice == 50 && items[1].SellPrice == 60 && engine.ActiveReleaseId == "previous", "A later chunk failure did not undo earlier applied chunks.");
                    using (var cancellation = new System.Threading.CancellationTokenSource())
                    {
                        var task = engine.PrepareAsync(MakeRelease("cancel", 500, 600), cancellation.Token, applyImmediately: true).AsTask();
                        while (engine.LastAppliedChunkCount == 0 && !task.IsCompleted)
                            await UniTask.Delay(1, DelayType.Realtime);
                        bool sawIntermediate = !task.IsCompleted && items[0].SellPrice == 500 && items[1].SellPrice == 60;
                        cancellation.Cancel();
                        bool canceled = false;
                        try
                        {
                            await task;
                        }
                        catch (OperationCanceledException)
                        {
                            canceled = true;
                        }
                        Check(sawIntermediate, "No intermediate chunk state was observable.");
                        Check(canceled && items[0].SellPrice == 50 && items[1].SellPrice == 60 && engine.PendingUndoBytes == 0, "Cancellation left progressive values active.");
                    }
                    await engine.PrepareAsync(MakeRelease("confirmed", 700, 800), applyImmediately: true);
                    engine.Commit();
                    Check(items[0].SellPrice == 700 && items[1].SellPrice == 800 && engine.ActiveReleaseId == "confirmed" && engine.PendingUndoBytes == 0, "Confirmation reset applied values or retained undo buffers.");
                    await engine.PrepareAsync(null, applyImmediately: true);
                    Check(items[0].SellPrice == 10 && items[1].SellPrice == 10, "Progressive baseline restoration did not apply.");
                    engine.Discard();
                    Check(items[0].SellPrice == 700 && items[1].SellPrice == 800, "Canceled baseline restoration lost the previous patch.");
                    Debug.Log("DATA_PATCH_PROGRESSIVE_CHUNK_UNDO_CANCELLATION_PASSED");
                }
                Check(items[0].SellPrice == 10 && items[1].SellPrice == 10, "Disposal failed to restore bundled values after progressive confirmation.");
            }
            finally
            {
                foreach (JunkItem item in items)
                {
                    if (item == null)
                        continue;
                    if (GameInstance.Items.TryGetValue(item.DataId, out BaseItem registered) && registered == item)
                        GameInstance.Items.Remove(item.DataId);
                    UnityEngine.Object.DestroyImmediate(item);
                }
                UnityEngine.Object.DestroyImmediate(profile);
                UnityEngine.Object.DestroyImmediate(database);
                string resolved = System.IO.Path.GetFullPath(directory);
                Check(resolved.StartsWith(System.IO.Path.GetFullPath(System.IO.Path.GetTempPath()), StringComparison.OrdinalIgnoreCase), "Unsafe progressive test cleanup path.");
                System.IO.Directory.Delete(resolved, true);
            }
        }

        [Serializable]
        private sealed class PrimitiveExportData
        {
            public bool boolean = true;
            public char character = '\u754c';
            public sbyte int8 = sbyte.MinValue;
            public byte uint8 = byte.MaxValue;
            public short int16 = short.MinValue;
            public ushort uint16 = ushort.MaxValue;
            public int int32 = int.MinValue;
            public uint uint32 = uint.MaxValue;
            public long int64 = long.MinValue;
            public ulong uint64 = ulong.MaxValue;
            public float single = float.MaxValue;
            public double number = double.MaxValue;
            public string text = "2026-10-15T00:00:00Z";
            public int[] values = new int[512];
        }

        public static void VerifyPrimitiveExportAllocations()
        {
            var fixture = new PrimitiveExportData();
            for (int i = 0; i < fixture.values.Length; ++i)
                fixture.values[i] = i;
            JObject expected = JObject.FromObject(fixture);
            JObject exported = DataPatchSerializer.Export(fixture);
            Check(JToken.DeepEquals(expected, exported), "Direct primitive JSON values changed exported numeric, character, boolean or text data.");
            var registry = new DataPatchRegistry(false);
            var restored = new PrimitiveExportData();
            DataPatchSerializer.ReadInto(restored, exported, registry);
            Check(JToken.DeepEquals(exported, DataPatchSerializer.Export(restored)), "Direct primitive reads changed numeric limits, text, characters, booleans or arrays.");
            var invalid = new[]
            {
                new JObject { ["int8"] = 128 },
                new JObject { ["uint8"] = -1 },
                new JObject { ["int16"] = 32768 },
                new JObject { ["uint16"] = -1 },
                new JObject { ["int32"] = 2147483648L },
                new JObject { ["uint32"] = -1 },
                new JObject { ["int64"] = ulong.MaxValue },
                new JObject { ["uint64"] = -1 },
                new JObject { ["boolean"] = 1 },
                new JObject { ["int32"] = 1.5 },
                new JObject { ["single"] = double.MaxValue },
                new JObject { ["number"] = double.PositiveInfinity },
            };
            foreach (JObject data in invalid)
            {
                bool rejected = false;
                try
                {
                    DataPatchSerializer.ReadInto(restored, data, registry);
                }
                catch (Exception error) when (error is InvalidOperationException || error is OverflowException || error is ArgumentException || error is JsonException)
                {
                    rejected = true;
                }
                Check(rejected, "Direct primitive reads accepted overflow, a wrong JSON type, rounding or a non-finite number: " + data);
            }
            for (int i = 0; i < 3; ++i)
                DataPatchSerializer.Export(fixture);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 16; ++i)
                DataPatchSerializer.Export(fixture);
            long direct = GC.GetAllocatedBytesForCurrentThread() - before;
            before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 16; ++i)
            {
                var values = new JArray();
                foreach (int value in fixture.values)
                    values.Add(JToken.FromObject(value));
            }
            long oldPrimitivePath = GC.GetAllocatedBytesForCurrentThread() - before;
            if (oldPrimitivePath > 0)
            {
                Check(direct < oldPrimitivePath * 0.7, $"Primitive export allocation regression: full exports {direct}, old primitive path {oldPrimitivePath} bytes.");
                Debug.Log($"DATA_PATCH_PRIMITIVE_ALLOCATION_PASSED: full direct exports {direct} bytes; old primitive array conversion alone {oldPrimitivePath} bytes.");
            }
            else
            {
                Debug.Log("DATA_PATCH_PRIMITIVE_VALUES_PASSED: allocation counter unavailable on this Unity runtime.");
            }
        }

        public static async UniTask VerifyRuntimeLoadingEvents()
        {
            Check(DataPatchRuntime.Engine == null && !DataPatchRuntime.IsLoading, "Loading event checks require an idle patch runtime.");
            var database = ScriptableObject.CreateInstance<GameDatabase>();
            var profile = ScriptableObject.CreateInstance<DataPatchProfile>();
            var item = ScriptableObject.CreateInstance<JunkItem>();
            item.Id = "patch-events-item-" + Guid.NewGuid().ToString("N");
            item.SellPrice = 10;
            database.items = new[] { item };
            profile.database = database;
            profile.patchDatabaseId = "patch-events-" + Guid.NewGuid().ToString("N");
            int starts = 0;
            int finishes = 0;
            int mainThread = System.Threading.Thread.CurrentThread.ManagedThreadId;
            bool statesValid = true;
            bool lastSuccess = false;
            string lastError = null;
            Action started = () =>
            {
                ++starts;
                statesValid &= DataPatchRuntime.IsLoading && DataPatchRuntime.LastError == "" && System.Threading.Thread.CurrentThread.ManagedThreadId == mainThread;
            };
            Action<bool, string> finished = (success, error) =>
            {
                ++finishes;
                lastSuccess = success;
                lastError = error;
                statesValid &= !DataPatchRuntime.IsLoading && System.Threading.Thread.CurrentThread.ManagedThreadId == mainThread;
            };
            Action reset = () => DataPatchRuntime.Reset();
            Action throwStarted = () => throw new InvalidOperationException("Expected patch-start subscriber failure.");
            Action<bool, string> throwFinished = (success, error) => throw new InvalidOperationException("Expected patch-finish subscriber failure.");
            try
            {
                var engine = new DataPatchEngine(profile, refresh: false);
                engine.Prepare(Release(profile, "before-cleanup", new[]
                {
                    new DataPatchEntry { dataType = nameof(JunkItem), dataId = item.DataId, data = new JObject { ["sellPrice"] = 20 } },
                }));
                engine.Commit();
                typeof(DataPatchRuntime).GetProperty(nameof(DataPatchRuntime.Engine)).SetValue(null, engine);
                typeof(DataPatchRuntime).GetProperty(nameof(DataPatchRuntime.Profile)).SetValue(null, profile);
                // Faulty subscribers precede healthy ones; all must still be invoked.
                DataPatchRuntime.PatchingStarted += throwStarted;
                DataPatchRuntime.PatchingFinished += throwFinished;
                DataPatchRuntime.PatchingStarted += started;
                DataPatchRuntime.PatchingFinished += finished;
                await DataPatchRuntime.Prepare("", baseline: true);
                Check(starts == 1 && finishes == 1 && lastSuccess && lastError == "" && statesValid, "Successful patch loading did not report one healthy event pair on the main thread.");
                Check(item.SellPrice == 10, "Finished loading was announced before baseline values were applied.");
                if (UnityEngine.Scripting.GarbageCollector.isIncremental && UnityEngine.Scripting.GarbageCollector.GCMode == UnityEngine.Scripting.GarbageCollector.Mode.Enabled)
                    Check(DataPatchRuntime.LastCleanupMilliseconds > 0, "A changed patch did not run its budgeted temporary-memory cleanup.");
                DataPatchRuntime.Commit();
                Check(finishes == 1, "Release confirmation emitted a duplicate loading-finished event.");
                DataPatchRuntime.PatchingStarted -= throwStarted;
                DataPatchRuntime.PatchingFinished -= throwFinished;
                bool rejected = false;
                try
                {
                    await DataPatchRuntime.Prepare("", baseline: true, expectedHash: "mismatched-hash");
                }
                catch (InvalidOperationException)
                {
                    rejected = true;
                }
                Check(rejected && starts == 2 && finishes == 2 && !lastSuccess && !string.IsNullOrEmpty(lastError) && lastError == DataPatchRuntime.LastError, "Failed patch loading did not report its error and clear the loading state.");
                DataPatchRuntime.PatchingStarted += reset;
                bool canceled = false;
                try
                {
                    await DataPatchRuntime.Prepare("", baseline: true);
                }
                catch (OperationCanceledException)
                {
                    canceled = true;
                }
                Check(canceled && starts == 3 && finishes == 3 && !lastSuccess && lastError == "Patch runtime was reset." && statesValid, "Reset did not finish an active load exactly once.");
                DataPatchRuntime.PatchingStarted -= reset;
                DataPatchRuntime.Reset();
                await DataPatchRuntime.Initialize(null, database);
                Check(starts == 3 && finishes == 3 && !DataPatchRuntime.IsLoading, "Idle reset or disabled startup emitted spurious patch-loading events.");
                Debug.Log("DATA_PATCH_RUNTIME_LOADING_EVENTS_PASSED");
            }
            finally
            {
                DataPatchRuntime.PatchingStarted -= throwStarted;
                DataPatchRuntime.PatchingStarted -= started;
                DataPatchRuntime.PatchingStarted -= reset;
                DataPatchRuntime.PatchingFinished -= throwFinished;
                DataPatchRuntime.PatchingFinished -= finished;
                DataPatchRuntime.Reset();
                if (GameInstance.Items.TryGetValue(item.DataId, out BaseItem registered) && registered == item)
                    GameInstance.Items.Remove(item.DataId);
                UnityEngine.Object.DestroyImmediate(item);
                UnityEngine.Object.DestroyImmediate(profile);
                UnityEngine.Object.DestroyImmediate(database);
            }
        }

        public static async UniTask VerifyChunkReloadScheduling()
        {
            var database = ScriptableObject.CreateInstance<GameDatabase>();
            var profile = ScriptableObject.CreateInstance<DataPatchProfile>();
            var items = new JunkItem[128];
            string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "unity-patch-scheduling-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(directory);
            try
            {
                string prefix = "chunk-scheduling-" + Guid.NewGuid().ToString("N");
                var chunks = new DataPatchChunk[items.Length];
                var paths = new string[items.Length];
                for (int i = 0; i < items.Length; ++i)
                {
                    items[i] = ScriptableObject.CreateInstance<JunkItem>();
                    items[i].Id = prefix + "-" + i;
                    items[i].SellPrice = 10;
                    string payload = JsonConvert.SerializeObject(new[]
                    {
                        new DataPatchEntry { dataType = nameof(JunkItem), dataId = items[i].DataId, data = new JObject { ["sellPrice"] = 20 } },
                    });
                    paths[i] = System.IO.Path.Combine(directory, i + ".json");
                    System.IO.File.WriteAllText(paths[i], payload, new System.Text.UTF8Encoding(false));
                    chunks[i] = new DataPatchChunk { index = i, dataType = nameof(JunkItem), hash = DataPatchHttp.Hash(payload), bytes = System.Text.Encoding.UTF8.GetByteCount(payload), entryCount = 1 };
                }
                database.items = items;
                profile.database = database;
                profile.patchDatabaseId = prefix;
                profile.dataPatchSettings.preparationFrameBudgetMilliseconds = 2;
                string manifest = JsonConvert.SerializeObject(new DataPatchManifest { chunks = chunks });
                var release = new DataPatchRelease { id = prefix, databaseId = prefix, schemaVersion = 2, publishTime = "published", manifestJson = manifest, payloadHash = DataPatchHttp.Hash(manifest), chunkPaths = paths };
                using (var engine = new DataPatchEngine(profile, false, refresh: false))
                {
                    await engine.PrepareAsync(release, applyImmediately: true);
                    Check(engine.LastParsedChunkCount == items.Length && engine.LastAppliedChunkCount == items.Length, "Read-ahead skipped initial chunks or delayed progressive application.");
                    engine.Commit();
                    uint revision = DataPatchEngine.Revision;
                    var timer = System.Diagnostics.Stopwatch.StartNew();
                    await engine.PrepareAsync(release, applyImmediately: true);
                    Check(engine.LastParsedChunkCount == 0 && engine.LastPreparationRecordCount == 0 && engine.PreparedChangeCount == 0, "Cached reload reparsed or restaged unchanged chunks.");
                    Check(engine.LastReusedChunkCount == items.Length && engine.LastReuseVerificationPassCount == 1, "Cached reload used one verification pass per chunk.");
                    engine.Commit();
                    Check(DataPatchEngine.Revision == revision && System.Linq.Enumerable.All(items, item => item.SellPrice == 20), "Cached reload invalidated gameplay caches or lost applied prices.");
                    Debug.Log($"DATA_PATCH_CHUNK_SCHEDULING_PASSED: {items.Length} unchanged chunks, one verification pass, {timer.Elapsed.TotalMilliseconds:F1} ms reload.");
                }
                Check(System.Linq.Enumerable.All(items, item => item.SellPrice == 10), "Read-ahead disposal did not restore bundled prices.");
            }
            finally
            {
                foreach (JunkItem item in items)
                {
                    if (item == null)
                        continue;
                    if (GameInstance.Items.TryGetValue(item.DataId, out BaseItem registered) && registered == item)
                        GameInstance.Items.Remove(item.DataId);
                    UnityEngine.Object.DestroyImmediate(item);
                }
                UnityEngine.Object.DestroyImmediate(profile);
                UnityEngine.Object.DestroyImmediate(database);
                string resolved = System.IO.Path.GetFullPath(directory);
                Check(resolved.StartsWith(System.IO.Path.GetFullPath(System.IO.Path.GetTempPath()), StringComparison.OrdinalIgnoreCase), "Unsafe scheduling test cleanup path.");
                System.IO.Directory.Delete(resolved, true);
            }
        }

        public static async UniTask VerifyCompactPatchMemory()
        {
            var numeric = new JObject
            {
                ["exactDecimal"] = new JValue(12345678901234567890.123456789m),
                ["largeDouble"] = new JValue(double.MaxValue),
                ["largeFloat"] = new JValue(float.MaxValue),
                ["signedInteger"] = new JValue(long.MaxValue),
                ["unsignedInteger"] = new JValue(ulong.MaxValue),
            };
            Type storage = typeof(DataPatchEngine).Assembly.GetType("MultiplayerARPG.DataPatchStorage");
            byte[] compressed = (byte[])storage.GetMethod("Compress").Invoke(null, new object[] { numeric });
            var restored = (JObject)storage.GetMethod("Read").Invoke(null, new object[] { compressed });
            Check(restored["exactDecimal"].Value<decimal>() == 12345678901234567890.123456789m &&
                restored["largeDouble"].Value<double>() == double.MaxValue && restored["largeFloat"].Value<float>() == float.MaxValue &&
                restored["signedInteger"].Value<long>() == long.MaxValue && restored["unsignedInteger"].ToObject<ulong>() == ulong.MaxValue,
                "Compact snapshots rounded decimal, floating-point or integer values.");
            var database = ScriptableObject.CreateInstance<GameDatabase>();
            var profile = ScriptableObject.CreateInstance<DataPatchProfile>();
            var items = new JunkItem[8];
            string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "unity-compact-patch-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(directory);
            long rawBaselineBytes = 0;
            DataPatchRelease MakeRelease(string id, bool changed)
            {
                var chunks = new DataPatchChunk[4];
                var paths = new string[4];
                for (int i = 0; i < chunks.Length; ++i)
                {
                    var records = new DataPatchEntry[2];
                    for (int j = 0; j < records.Length; ++j)
                    {
                        JunkItem item = items[i * 2 + j];
                        // Build from the bundled values even after earlier patches were applied.
                        JObject data = DataPatchSerializer.Export(item);
                        data["sellPrice"] = int.MaxValue - 7;
                        data["dismantleReturnGold"] = int.MaxValue - 7;
                        data["defaultDescription"] = new string('x', 32000) + "測試";
                        if (changed && i == 0 && j == 0)
                        {
                            data["sellPrice"] = 123;
                            data["dismantleReturnGold"] = 456;
                            data["defaultDescription"] = "patched";
                        }
                        records[j] = new DataPatchEntry { dataType = nameof(JunkItem), dataId = item.DataId, data = data };
                    }
                    string payload = JsonConvert.SerializeObject(records);
                    paths[i] = System.IO.Path.Combine(directory, id + "-" + i + ".json");
                    System.IO.File.WriteAllText(paths[i], payload, new System.Text.UTF8Encoding(false));
                    chunks[i] = new DataPatchChunk
                    {
                        index = i,
                        dataType = nameof(JunkItem),
                        hash = DataPatchHttp.Hash(payload),
                        bytes = System.Text.Encoding.UTF8.GetByteCount(payload),
                        entryCount = records.Length,
                    };
                }
                string manifest = JsonConvert.SerializeObject(new DataPatchManifest { chunks = chunks });
                return new DataPatchRelease
                {
                    id = id,
                    databaseId = profile.patchDatabaseId,
                    schemaVersion = 2,
                    publishTime = "published",
                    manifestJson = manifest,
                    payloadHash = DataPatchHttp.Hash(manifest),
                    chunkPaths = paths,
                };
            }
            try
            {
                string prefix = "compact-patch-" + Guid.NewGuid().ToString("N");
                for (int i = 0; i < items.Length; ++i)
                {
                    items[i] = ScriptableObject.CreateInstance<JunkItem>();
                    items[i].Id = prefix + "-" + i;
                    items[i].SellPrice = int.MaxValue - 7;
                    items[i].DismantleReturnGold = int.MaxValue - 7;
                    items[i].DefaultDescription = new string('x', 32000) + "測試";
                    items[i].DefaultTitle = "2026-10-14T12:00:00Z";
                    // Match normal database registration before exporting a no-op fixture.
                    GameInstance.AddItems(items[i]);
                    typeof(BaseItem).GetField("audioGenderUsingSFX")?.SetValue(items[i], null);
                    rawBaselineBytes += System.Text.Encoding.UTF8.GetByteCount(DataPatchSerializer.Export(items[i]).ToString(Formatting.None));
                }
                database.items = items;
                profile.database = database;
                profile.patchDatabaseId = prefix;
                profile.dataPatchSettings.preparationFrameBudgetMilliseconds = 0.1f;
                var first = MakeRelease("compact-first", false);
                var second = MakeRelease("compact-second", true);
                var engine = new DataPatchEngine(profile, false, refresh: false);
                try
                {
                    await engine.PrepareAsync(first);
                    Check(engine.LastParsedChunkCount == 4 && engine.PreparedChangeCount == 0, "Initial chunks were not prepared as a no-op snapshot.");
                    Check(engine.LastCacheTraversalCount == 0, "An unchanged patch rediscovered the complete cache graph.");
                    Check(engine.RetainedSnapshotBytes < rawBaselineBytes / 8, "Rollback snapshots retain expanded JSON rather than compact bytes.");
                    engine.Commit();
                    long snapshotBytes = engine.RetainedSnapshotBytes;
                    await engine.PrepareAsync(second);
                    Check(engine.LastParsedChunkCount == 1 && engine.LastPreparationRecordCount == 1, "Changed-chunk reload reparsed unchanged chunks or prepared unchanged records.");
                    Check(engine.LastReusedChunkCount == 3 && engine.LastReuseVerificationPassCount == 1, "Unchanged chunks required separate worker verification passes.");
                    Check(items[0].SellPrice == int.MaxValue - 7, "Streaming preparation modified live values before activation.");
                    engine.Commit();
                    Check(items[0].SellPrice == 123 && items[0].DismantleReturnGold == 456 && items[0].DefaultDescription == "patched" && items[0].DefaultTitle == "2026-10-14T12:00:00Z", "Compact patch did not activate or changed date-like text.");
                    Check(snapshotBytes == 0 && engine.CapturedDefaultCount == 1 && engine.RetainedSnapshotBytes > 0,
                        "No-op records captured defaults, or the first real change lost its rollback snapshot.");
                    snapshotBytes = engine.RetainedSnapshotBytes;
                    await engine.PrepareAsync(second);
                    Check(engine.LastParsedChunkCount == 0 && engine.LastPreparationRecordCount == 0, "An identical reload rebuilt parsed record trees.");
                    Check(engine.LastReusedChunkCount == 4 && engine.LastReuseVerificationPassCount == 1, "An unchanged reload returned to the main thread for every file verification.");
                    engine.Commit();
                    Check(engine.RetainedSnapshotBytes == snapshotBytes, "Identical reload duplicated a rollback buffer.");
                    // Reused chunks must still reject duplicated identities across descriptors.
                    var originalManifest = JsonConvert.DeserializeObject<DataPatchManifest>(second.manifestJson);
                    var duplicate = originalManifest.chunks[0];
                    string duplicateManifest = JsonConvert.SerializeObject(new DataPatchManifest
                    {
                        chunks = new[]
                        {
                            duplicate,
                            new DataPatchChunk { index = 1, dataType = duplicate.dataType, hash = duplicate.hash, bytes = duplicate.bytes, entryCount = duplicate.entryCount },
                        },
                    });
                    var duplicateRelease = new DataPatchRelease
                    {
                        id = "compact-duplicate",
                        databaseId = profile.patchDatabaseId,
                        schemaVersion = 2,
                        publishTime = "published",
                        manifestJson = duplicateManifest,
                        payloadHash = DataPatchHttp.Hash(duplicateManifest),
                        chunkPaths = new[] { second.chunkPaths[0], second.chunkPaths[0] },
                    };
                    bool duplicateRejected = false;
                    try
                    {
                        await engine.PrepareAsync(duplicateRelease);
                    }
                    catch (InvalidOperationException)
                    {
                        duplicateRejected = true;
                    }
                    Check(duplicateRejected && engine.PreparedChangeCount == 0 && items[0].SellPrice == 123, "Reused chunks accepted duplicate identities or changed live values.");
                    // Reused metadata must still detect corruption of an unchanged chunk.
                    string original = System.IO.File.ReadAllText(second.chunkPaths[3]);
                    System.IO.File.WriteAllText(second.chunkPaths[3], "[]");
                    bool rejected = false;
                    try
                    {
                        await engine.PrepareAsync(second);
                    }
                    catch (System.IO.IOException)
                    {
                        rejected = true;
                    }
                    finally
                    {
                        System.IO.File.WriteAllText(second.chunkPaths[3], original, new System.Text.UTF8Encoding(false));
                    }
                    Check(rejected && engine.PreparedChangeCount == 0 && items[0].SellPrice == 123, "Corrupt cached chunks bypassed integrity or changed live data.");
                    using (var cancellation = new System.Threading.CancellationTokenSource())
                    {
                        cancellation.Cancel();
                        bool canceled = false;
                        try
                        {
                            await engine.PrepareAsync(first, cancellation.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            canceled = true;
                        }
                        Check(canceled && engine.PreparedChangeCount == 0 && items[0].SellPrice == 123, "Canceled preparation changed active values.");
                    }
                    // Earlier published snapshots and complete omissions must still restore correctly.
                    await engine.PrepareAsync(first);
                    engine.Commit();
                    Check(items[0].SellPrice == int.MaxValue - 7 && items[0].DismantleReturnGold == int.MaxValue - 7 && items[0].DefaultDescription == new string('x', 32000) + "測試", "Compressed rollback lost numeric precision or text.");
                    await engine.PrepareAsync(second);
                    engine.Commit();
                    await engine.PrepareAsync(null);
                    engine.Commit();
                    Check(items[0].SellPrice == int.MaxValue - 7, "Full replacement did not restore omitted overrides.");
                    Debug.Log($"DATA_PATCH_COMPACT_MEMORY_PASSED: raw baseline {rawBaselineBytes} bytes, retained snapshots {snapshotBytes} bytes.");
                }
                finally
                {
                    engine.Dispose();
                }
                Check(engine.RetainedSnapshotBytes == 0 && engine.CapturedDefaultCount == 0, "Disposed engine retained rollback buffers.");
            }
            finally
            {
                foreach (JunkItem item in items)
                {
                    if (item == null)
                        continue;
                    if (GameInstance.Items.TryGetValue(item.DataId, out BaseItem registered) && registered == item)
                        GameInstance.Items.Remove(item.DataId);
                    UnityEngine.Object.DestroyImmediate(item);
                }
                UnityEngine.Object.DestroyImmediate(profile);
                UnityEngine.Object.DestroyImmediate(database);
                string resolved = System.IO.Path.GetFullPath(directory);
                Check(resolved.StartsWith(System.IO.Path.GetFullPath(System.IO.Path.GetTempPath()), StringComparison.OrdinalIgnoreCase), "Unsafe compact test cleanup path.");
                System.IO.Directory.Delete(resolved, true);
            }
        }

        public static async UniTask VerifyStartupPreparation()
        {
            var database = ScriptableObject.CreateInstance<GameDatabase>();
            var profile = ScriptableObject.CreateInstance<DataPatchProfile>();
            var items = new JunkItem[128];
            try
            {
                string prefix = "startup-patch-test-" + Guid.NewGuid().ToString("N");
                for (int i = 0; i < items.Length; ++i)
                {
                    items[i] = ScriptableObject.CreateInstance<JunkItem>();
                    items[i].Id = prefix + "-" + i;
                    items[i].SellPrice = 10;
                }
                database.items = items;
                profile.database = database;
                profile.patchDatabaseId = prefix;
                profile.dataPatchSettings.preparationFrameBudgetMilliseconds = 0.1f;
                using (var engine = new DataPatchEngine(profile, false, refresh: false))
                {
                    await engine.PrepareAsync(null);
                    Check(engine.CapturedDefaultCount == 0 && engine.LastPreparationRecordCount == 0, "Startup without a release captured or exported bundled records.");
                    engine.Commit();
                    await engine.PrepareAsync(Release(profile, "startup-full-noop", new[]
                    {
                        new DataPatchEntry { dataType = nameof(JunkItem), dataId = items[0].DataId, data = DataPatchSerializer.Export(items[0]) },
                    }));
                    Check(engine.PreparedChangeCount == 0 && engine.CapturedDefaultCount == 0 && engine.LastReusedCurrentExportCount == 1,
                        "A full no-op export allocated rollback data or missed the direct comparison path.");
                    engine.Commit();
                    items[0].SellPrice = -1;
                    bool invalidNoopRejected = false;
                    try
                    {
                        await engine.PrepareAsync(Release(profile, "startup-invalid-noop", new[]
                        {
                            new DataPatchEntry { dataType = nameof(JunkItem), dataId = items[0].DataId, data = DataPatchSerializer.Export(items[0]) },
                        }));
                    }
                    catch (InvalidOperationException)
                    {
                        invalidNoopRejected = true;
                    }
                    Check(invalidNoopRejected && engine.CapturedDefaultCount == 0,
                        "An unchanged invalid value bypassed validation or captured a rollback buffer.");
                    items[0].SellPrice = 10;
                    var first = Release(profile, "startup-first", new[]
                    {
                        new DataPatchEntry { dataType = nameof(JunkItem), dataId = items[0].DataId, data = JObject.Parse("{\"sellPrice\":1000}") },
                    });
                    await engine.PrepareAsync(first);
                    Check(engine.CapturedDefaultCount == 1 && engine.LastPreparationRecordCount == 1, "One override captured unrelated rollback snapshots.");
                    Check(items[0].SellPrice == 10, "Startup staging modified a live asset.");
                    engine.Commit();
                    Check(items[0].SellPrice == 1000, "First lazy override was not activated.");
                    items[127].SellPrice = 33;
                    var second = Release(profile, "startup-second", new[]
                    {
                        new DataPatchEntry { dataType = nameof(JunkItem), dataId = items[1].DataId, data = JObject.Parse("{\"sellPrice\":2000}") },
                    });
                    await engine.PrepareAsync(second);
                    Check(engine.CapturedDefaultCount == 2 && engine.LastPreparationRecordCount == 2, "A subsequent patch did not limit staging to current/prior targets.");
                    engine.Commit();
                    Check(items[0].SellPrice == 10 && items[1].SellPrice == 2000, "Omitted records did not roll back to bundled values.");
                    Check(items[127].SellPrice == 33, "An untouched runtime asset was modified by patch preparation.");
                    await engine.PrepareAsync(Release(profile, "startup-rollback", Array.Empty<DataPatchEntry>()));
                    engine.Commit();
                    Check(items[1].SellPrice == 10 && items[127].SellPrice == 33, "Rollback changed an untouched asset or kept an override.");
                    uint revision = DataPatchEngine.Revision;
                    await engine.PrepareAsync(null);
                    Check(engine.PreparedChangeCount == 0 && engine.CapturedDefaultCount == 2, "No-op baseline captured more data or staged changes.");
                    engine.Commit();
                    Check(DataPatchEngine.Revision == revision, "No-op startup invalidated gameplay caches.");
                    var invalid = Release(profile, "startup-invalid", new[]
                    {
                        new DataPatchEntry { dataType = nameof(JunkItem), dataId = items[2].DataId, data = JObject.Parse("{\"sellPrice\":3000}") },
                        new DataPatchEntry { dataType = nameof(JunkItem), dataId = items[0].DataId, data = JObject.Parse("{\"sellPrice\":-1}") },
                    });
                    bool rejected = false;
                    try
                    {
                        await engine.PrepareAsync(invalid);
                    }
                    catch (InvalidOperationException)
                    {
                        rejected = true;
                    }
                    Check(rejected && engine.PreparedChangeCount == 0 && items[2].SellPrice == 10, "A failed lazy patch partially modified live data.");
                    // Explicit eager refresh remains available for tools that request a whole baseline.
                    engine.Refresh();
                    Check(engine.CapturedDefaultCount >= items.Length, "Explicit baseline refresh no longer captures all data.");
                    Debug.Log("DATA_PATCH_LAZY_STARTUP_NOOP_ROLLBACK_FAILURE_PASSED");
                }
            }
            finally
            {
                foreach (var item in items)
                {
                    if (item == null)
                        continue;
                    if (GameInstance.Items.TryGetValue(item.DataId, out BaseItem registered) && registered == item)
                        GameInstance.Items.Remove(item.DataId);
                    UnityEngine.Object.DestroyImmediate(item);
                }
                UnityEngine.Object.DestroyImmediate(profile);
                UnityEngine.Object.DestroyImmediate(database);
            }
        }

        public static void VerifyRegistryTraversal()
        {
            var first = ScriptableObject.CreateInstance<JunkItem>();
            var second = ScriptableObject.CreateInstance<JunkItem>();
            try
            {
                first.Id = "registry-first-" + Guid.NewGuid().ToString("N");
                second.Id = "registry-second-" + Guid.NewGuid().ToString("N");
                var graph = new RegistryTraversalGraph
                {
                    numbers = new RegistryNumericGraph { values = new float[20000] },
                    rows = new[] { new RegistryReferenceRow { amounts = new[] { new ItemAmount { item = first, amount = 1 } } } },
                    polymorphic = new RegistryTraversalDerived { item = second },
                    unityReference = first,
                };
                var registry = new DataPatchRegistry(false);
                registry.AddGraph(graph);
                Check(registry.Targets.Count == 2, "Registry pruning dropped nested or polymorphic game-data references.");
                Check(ReferenceEquals(registry.Resolve(typeof(BaseItem), first.DataId), first) &&
                    ReferenceEquals(registry.Resolve(typeof(BaseItem), second.DataId), second), "Pruned registry changed data-reference identities.");
                Debug.Log("DATA_PATCH_REGISTRY_STRUCT_POLYMORPHIC_REFERENCES_PASSED");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(first);
                UnityEngine.Object.DestroyImmediate(second);
            }
        }

        [Serializable]
        private struct RegistryNumericGraph
        {
            public float[] values;
        }

        [Serializable]
        private struct RegistryReferenceRow
        {
            public ItemAmount[] amounts;
        }

        [Serializable]
        private class RegistryTraversalBase
        {
        }

        [Serializable]
        private class RegistryTraversalDerived : RegistryTraversalBase
        {
            public BaseItem item;
        }

        [Serializable]
        private class RegistryTraversalGraph
        {
            public RegistryNumericGraph numbers;
            public RegistryReferenceRow[] rows;
            public RegistryTraversalBase polymorphic;
            public UnityEngine.Object unityReference;
        }

        public static void VerifyChunkedTransport()
        {
            string folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "patch-chunks-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(folder);
            try
            {
                var chunks = new DataPatchChunk[4];
                var paths = new string[4];
                for (int i = 0; i < chunks.Length; ++i)
                {
                    string payload = "[{\"dataType\":\"JunkItem\",\"dataId\":" + i + ",\"data\":{\"longValue\":9223372036854775807,\"text\":\"" + new string('x', 900000) + "\"}}]";
                    chunks[i] = new DataPatchChunk
                    {
                        index = i,
                        dataType = "JunkItem",
                        hash = DataPatchHttp.Hash(payload),
                        bytes = System.Text.Encoding.UTF8.GetByteCount(payload),
                        entryCount = 1,
                    };
                    paths[i] = System.IO.Path.Combine(folder, i + ".json");
                    System.IO.File.WriteAllText(paths[i], payload, new System.Text.UTF8Encoding(false));
                }
                string manifestJson = JsonConvert.SerializeObject(new DataPatchManifest { chunks = chunks });
                var release = new DataPatchRelease
                {
                    id = "chunk-test",
                    schemaVersion = 2,
                    manifestJson = manifestJson,
                    payloadHash = DataPatchHttp.Hash(manifestJson),
                    chunkPaths = paths,
                };
                DataPatchHttp.ValidateHash(release);
                int count = 0;
                foreach (var entry in DataPatchHttp.ReadEntries(release))
                {
                    Check(entry.data["longValue"].Value<long>() == long.MaxValue, "Chunk changed Int64 precision.");
                    ++count;
                }
                Check(count == 4, "Chunk reader dropped records.");
                Check(!JObject.Parse(JsonConvert.SerializeObject(release)).ContainsKey("chunkPaths"), "Manifest cache contains local paths.");
                string originalPayload = System.IO.File.ReadAllText(paths[0]);
                string trailingPayload = originalPayload + " []";
                System.IO.File.WriteAllText(paths[0], trailingPayload, new System.Text.UTF8Encoding(false));
                string originalHash = chunks[0].hash;
                int originalBytes = chunks[0].bytes;
                chunks[0].hash = DataPatchHttp.Hash(trailingPayload);
                chunks[0].bytes = System.Text.Encoding.UTF8.GetByteCount(trailingPayload);
                release.manifestJson = JsonConvert.SerializeObject(new DataPatchManifest { chunks = chunks });
                release.payloadHash = DataPatchHttp.Hash(release.manifestJson);
                bool rejectedTrailingJson = false;
                try
                {
                    foreach (DataPatchEntry ignored in DataPatchHttp.ReadEntries(release))
                    {
                    }
                }
                catch (JsonException)
                {
                    rejectedTrailingJson = true;
                }
                finally
                {
                    System.IO.File.WriteAllText(paths[0], originalPayload, new System.Text.UTF8Encoding(false));
                    chunks[0].hash = originalHash;
                    chunks[0].bytes = originalBytes;
                    release.manifestJson = manifestJson;
                    release.payloadHash = DataPatchHttp.Hash(manifestJson);
                }
                Check(rejectedTrailingJson, "Streaming chunk parsing accepted additional JSON after the record array.");
                System.IO.File.WriteAllText(paths[3], "[]");
                bool rejectedCorruptChunk = false;
                try
                {
                    DataPatchHttp.ValidateHash(release);
                }
                catch (System.IO.IOException)
                {
                    rejectedCorruptChunk = true;
                }
                Check(rejectedCorruptChunk, "Corrupt chunk was accepted.");
                release.chunkPaths = null;
                Reject(() => DataPatchHttp.ValidateHash(release));
                release.payloadHash = "tampered";
                Reject(() => DataPatchHttp.ValidateManifest(release));
                Debug.Log("DATA_PATCH_CHUNK_VERIFICATION_PASSED");
            }
            finally
            {
                System.IO.Directory.Delete(folder, true);
            }
        }

        // Legacy Item implements potion interfaces even when its category is Weapon.
        private sealed class WeaponWithPotionInterface : PotionItem
        {
            public override ItemType ItemType => ItemType.Weapon;
        }

        public static void VerifyItemCategoryValidation()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponWithPotionInterface>();
            var potion = ScriptableObject.CreateInstance<PotionItem>();
            try
            {
                var field = typeof(PotionItem).GetField("buff", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                field.SetValue(weapon, null);
                field.SetValue(potion, null);
                var registry = new DataPatchRegistry(false);
                var data = JObject.Parse("{\"sellPrice\":1000}");
                DataPatchSerializer.ReadInto(weapon, data, registry);
                Check(weapon.SellPrice == 1000, "Weapon was rejected by potion validation.");
                Reject(() => DataPatchSerializer.ReadInto(potion, data, registry));
                Debug.Log("DATA_PATCH_ITEM_CATEGORY_PASSED");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(weapon);
                UnityEngine.Object.DestroyImmediate(potion);
            }
        }

        public static void VerifyTolerantRecords()
        {
            var database = ScriptableObject.CreateInstance<GameDatabase>();
            var profile = ScriptableObject.CreateInstance<DataPatchProfile>();
            var item = ScriptableObject.CreateInstance<JunkItem>();
            var invalid = ScriptableObject.CreateInstance<PotionItem>();
            string suffix = Guid.NewGuid().ToString("N");
            item.Id = "null-test-" + suffix;
            invalid.Id = "invalid-test-" + suffix;
            item.SellPrice = 10;
            invalid.SellPrice = 20;
            typeof(PotionItem).GetField("buff", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(invalid, null);
            database.items = new BaseItem[] { item, invalid };
            profile.database = database;
            profile.patchDatabaseId = "null-test";
            try
            {
                using (var engine = new DataPatchEngine(profile))
                {
                    engine.Prepare(Release(profile, "first", new[] { new DataPatchEntry { dataType = nameof(JunkItem), dataId = item.DataId, data = JObject.Parse("{\"sellPrice\":25}") } }));
                    engine.Commit();
                    typeof(PotionItem).GetField("buff", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(invalid, null);
                    engine.Prepare(Release(profile, "second", new[]
                    {
                        new DataPatchEntry { dataType = nameof(JunkItem), dataId = item.DataId, data = JObject.Parse("{\"sellPrice\":null}") },
                        new DataPatchEntry { dataType = nameof(PotionItem), dataId = invalid.DataId, data = JObject.Parse("{\"sellPrice\":99,\"buff\":null}") },
                    }));
                    Check(item.SellPrice == 25 && invalid.SellPrice == 20, "Preparation changed live values.");
                    Check(engine.PreparationWarnings.Count >= 3, "Skipped values were not logged.");
                    engine.Commit();
                    Check(item.SellPrice == 25, "Null restored the bundled value instead of retaining the current value.");
                    Check(invalid.SellPrice == 20, "Invalid record was partially applied.");
                    engine.Prepare(Release(profile, "zero", new[] { new DataPatchEntry { dataType = nameof(JunkItem), dataId = item.DataId, data = JObject.Parse("{\"sellPrice\":0}") } }));
                    engine.Commit();
                    Check(item.SellPrice == 0, "Zero was treated as null.");
                }
                Debug.Log("DATA_PATCH_TOLERANT_RECORDS_PASSED");
            }
            finally
            {
                GameInstance.Items.Remove(item.DataId);
                GameInstance.Items.Remove(invalid.DataId);
                UnityEngine.Object.DestroyImmediate(item);
                UnityEngine.Object.DestroyImmediate(invalid);
                UnityEngine.Object.DestroyImmediate(profile);
                UnityEngine.Object.DestroyImmediate(database);
            }
        }

        public static void VerifyRegistrySecondaryLookups()
        {
            var original = ScriptableObject.CreateInstance<MonsterCharacter>();
            var bundledCopy = ScriptableObject.CreateInstance<MonsterCharacter>();
            original.Id = "registry-secondary-" + Guid.NewGuid().ToString("N");
            bundledCopy.Id = original.Id;
            int id = original.DataId;
            Check(!GameInstance.Characters.ContainsKey(id) && !GameInstance.MonsterCharacters.ContainsKey(id) && !GameInstance.MonsterEntitiesData.ContainsKey(id), "Fixture ID already registered.");
            try
            {
                GameInstance.Characters.Add(id, original);
                GameInstance.MonsterCharacters.Add(id, original);
                // Deliberately collide an entity ID with a data ID. This is not a data registry.
                GameInstance.MonsterEntitiesData.Add(id, bundledCopy);
                var registry = DataPatchRegistry.Runtime(null);
                Check(ReferenceEquals(registry.Resolve(typeof(MonsterCharacter), id), original), "Secondary lookup replaced the canonical character.");
                Check(ReferenceEquals(registry.Targets[DataPatchRegistry.Key(nameof(MonsterCharacter), id)], original), "Secondary lookup became a patch target.");
                GameInstance.MonsterCharacters[id] = bundledCopy;
                Reject(() => DataPatchRegistry.Runtime(null));
                Debug.Log("DATA_PATCH_SECONDARY_LOOKUPS_PASSED");
            }
            finally
            {
                GameInstance.Characters.Remove(id);
                GameInstance.MonsterCharacters.Remove(id);
                GameInstance.MonsterEntitiesData.Remove(id);
                UnityEngine.Object.DestroyImmediate(original);
                UnityEngine.Object.DestroyImmediate(bundledCopy);
            }
        }

        public static void VerifyRegistryObjectIdentity()
        {
            var original = ScriptableObject.CreateInstance<MonsterCharacter>();
            var conflicting = ScriptableObject.CreateInstance<MonsterCharacter>();
            original.name = "OrcArcher";
            conflicting.name = "OrcArcher";
            original.Id = "registry-wrapper-test";
            conflicting.Id = original.Id;
            try
            {
                // Two managed wrappers pointing at one native Unity object.
                var wrapper = (MonsterCharacter)typeof(object).GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(original, null);
                Check(!ReferenceEquals(original, wrapper) && original == wrapper, "Wrapper fixture must share Unity identity.");
                var registry = new DataPatchRegistry(false);
                registry.AddGraph(original);
                registry.AddGraph(wrapper);
                Check(registry.Targets.Count == 1, "The same native asset was registered twice.");
                Check(ReferenceEquals(registry.Resolve(typeof(MonsterCharacter), original.DataId), original), "Reference resolution replaced the original asset.");
                Reject(() => registry.AddGraph(conflicting));
                Debug.Log("DATA_PATCH_REGISTRY_IDENTITY_PASSED");
            }
            finally
            {
                // Do not destroy the wrapper: it points to the same native object.
                UnityEngine.Object.DestroyImmediate(original);
                UnityEngine.Object.DestroyImmediate(conflicting);
            }
        }

        public static void Verify()
        {
            VerifyItemCategoryValidation();
            VerifyTolerantRecords();
            VerifyRegistryObjectIdentity();
            VerifyRegistrySecondaryLookups();
            VerifyChunkedTransport();
            GameInstance.ClearData();
            var database = ScriptableObject.CreateInstance<GameDatabase>();
            var profile = ScriptableObject.CreateInstance<DataPatchProfile>();
            profile.database = database;
            profile.patchDatabaseId = "patch-test";
            var item = ScriptableObject.CreateInstance<JunkItem>();
            item.Id = "patch-test-item";
            item.SellPrice = 10;
            var attribute = ScriptableObject.CreateInstance<Attribute>();
            attribute.Id = "patch-test-attribute";
            var monster = ScriptableObject.CreateInstance<MonsterCharacter>();
            monster.Id = "patch-test-monster";
            monster.Stats = new CharacterStatsIncremental{baseStats = new CharacterStats{hp = 100}};
            var table = ScriptableObject.CreateInstance<ItemDropTable>();
            table.name = "patch-test-drops";
            table.randomItems = new[]{new ItemDrop{item = item, minLevel = 1, maxLevel = 1, minAmount = 1, maxAmount = 1, dropRate = 1}};
            monster.itemDropManager.itemDropTables = new[]{table};
            database.items = new BaseItem[]{item};
            database.attributes = new[]{attribute};
            database.monsterCharacters = new[]{monster};
            GameInstance.AddItems(item);
            GameInstance.AddAttributes(attribute);
            GameInstance.AddCharacters(monster);
            var registry = DataPatchRegistry.Runtime(database, profile.scannedData);
            var holder = new AmountHolder();
            DataPatchSerializer.ReadInto(holder, JObject.Parse("{\"items\":[{\"item\":{\"dataId\":" + item.DataId + "},\"level\":1,\"amount\":10}],\"attributes\":[{\"attribute\":{\"dataId\":" + attribute.DataId + "},\"amount\":10.5}]}"), registry);
            Check(ReferenceEquals(holder.items[0].item, item) && holder.items[0].amount == 10, "ItemAmount did not resolve GameInstance.Items.");
            Check(ReferenceEquals(holder.attributes[0].attribute, attribute) && holder.attributes[0].amount == 10.5f, "AttributeAmount did not resolve GameInstance.Attributes.");
            var collision = ScriptableObject.CreateInstance<WeaponItem>();
            collision.Id = item.Id;
            Reject(() => registry.AddGraph(collision));
            UnityEngine.Object.DestroyImmediate(collision);
            using (var engine = new DataPatchEngine(profile, false))
            {
                var release = Release(profile, "release-one", new[]{new DataPatchEntry{dataType = nameof(JunkItem), dataId = item.DataId, data = JObject.Parse("{\"sellPrice\":25}")}, new DataPatchEntry{dataType = nameof(MonsterCharacter), dataId = monster.DataId, data = JObject.Parse("{\"stats\":{\"baseStats\":{\"hp\":200}}}")}, new DataPatchEntry{dataType = nameof(ItemDropTable), dataId = table.DataId, data = JObject.Parse("{\"randomItems\":[{\"item\":{\"dataId\":" + item.DataId + "},\"minLevel\":1,\"maxLevel\":1,\"minAmount\":3,\"maxAmount\":3,\"dropRate\":1}]}")}});
                _ = monster.ItemDropManager.CacheRandomItems;
                var characterItem = new CharacterItem{id = "patch-test-instance", dataId = item.DataId};
                var previousItemCache = MemoryManager.CharacterItems.GetOrMakeCache(characterItem.id, in characterItem);
                engine.Prepare(release);
                Check(item.SellPrice == 10, "Staging changed live data.");
                engine.Commit();
                Check(item.SellPrice == 25 && monster.Stats.baseStats.hp == 200, "Inherited/private/nested fields did not patch.");
                Check(monster.ItemDropManager.CacheRandomItems[0].maxAmount == 3, "Drop cache remained stale.");
                Check(!ReferenceEquals(previousItemCache, MemoryManager.CharacterItems.GetOrMakeCache(characterItem.id, in characterItem)), "Computed item caches were not invalidated.");
                var invalid = Release(profile, "invalid", new[]{new DataPatchEntry{dataType = nameof(JunkItem), dataId = item.DataId, data = JObject.Parse("{\"sellPrice\":99,\"id\":\"changed\"}")}});
                Reject(() => engine.Prepare(invalid));
                Check(item.SellPrice == 25 && engine.ActiveReleaseId == "release-one", "Invalid release partially applied.");
                invalid = Release(profile, "invalid-ref", new[]{new DataPatchEntry{dataType = nameof(ItemDropTable), dataId = table.DataId, data = JObject.Parse("{\"randomItems\":[{\"item\":{\"dataId\":2147483647}}]}")}});
                Reject(() => engine.Prepare(invalid));
                release.payloadHash = "invalid";
                Reject(() => engine.Prepare(release));
                var rollback = Release(profile, "release-two", Array.Empty<DataPatchEntry>());
                engine.Prepare(rollback);
                engine.Commit();
                Check(item.SellPrice == 10 && monster.Stats.baseStats.hp == 100 && monster.ItemDropManager.CacheRandomItems[0].maxAmount == 1, "Snapshot rollback did not restore omitted overrides.");
            }

            UnityEngine.Object.DestroyImmediate(table);
            UnityEngine.Object.DestroyImmediate(monster);
            UnityEngine.Object.DestroyImmediate(attribute);
            UnityEngine.Object.DestroyImmediate(item);
            UnityEngine.Object.DestroyImmediate(database);
            GameInstance.ClearData();
            VerifyInt32AndIndexedCaches();
            VerifyPatchZip();
            VerifyJsonReviewScrolling();
        }

        public static async UniTask VerifyPartialPublishingPreparation()
        {
            var database = ScriptableObject.CreateInstance<GameDatabase>();
            var profile = ScriptableObject.CreateInstance<DataPatchProfile>();
            var shop = ScriptableObject.CreateInstance<NpcDialog>();
            var currency = ScriptableObject.CreateInstance<Currency>();
            currency.Id = "partial-currency-" + Guid.NewGuid().ToString("N");
            var items = new JunkItem[3];
            string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "unity-partial-patch-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(directory);
            DataPatchRelease ChunkRelease(string id, string itemPayload, string shopPayload)
            {
                string[] payloads = itemPayload == null ? new[] { shopPayload } : new[] { itemPayload, shopPayload };
                var chunks = new DataPatchChunk[payloads.Length];
                var paths = new string[payloads.Length];
                for (int i = 0; i < payloads.Length; ++i)
                {
                    paths[i] = System.IO.Path.Combine(directory, id + "-" + i + ".json");
                    System.IO.File.WriteAllText(paths[i], payloads[i], new System.Text.UTF8Encoding(false));
                    chunks[i] = new DataPatchChunk
                    {
                        index = i,
                        dataType = itemPayload != null && i == 0 ? nameof(JunkItem) : nameof(NpcDialog),
                        hash = DataPatchHttp.Hash(payloads[i]),
                        bytes = System.Text.Encoding.UTF8.GetByteCount(payloads[i]),
                        entryCount = JArray.Parse(payloads[i]).Count,
                    };
                }
                string manifest = JsonConvert.SerializeObject(new DataPatchManifest { chunks = chunks });
                return new DataPatchRelease
                {
                    id = id,
                    databaseId = profile.patchDatabaseId,
                    schemaVersion = 2,
                    publishTime = DateTime.UtcNow.ToString("O"),
                    manifestJson = manifest,
                    payloadHash = DataPatchHttp.Hash(manifest),
                    chunkPaths = paths,
                };
            }
            try
            {
                for (int i = 0; i < items.Length; ++i)
                {
                    items[i] = ScriptableObject.CreateInstance<JunkItem>();
                    items[i].Id = "partial-publishing-" + Guid.NewGuid().ToString("N");
                    items[i].SellPrice = 10 + i;
                }
                // Give the retained items nested containers to skip on reload. Otherwise
                // these flat fixtures cost one visit even when cached, and the newly
                // registered currency alone makes the second traversal larger.
                items[0].DismantleReturnItems = new[] { new ItemAmount { item = items[2], amount = 1 } };
                items[1].DismantleReturnItems = new[] { new ItemAmount { item = items[2], amount = 1 } };
                database.items = items;
                profile.database = database;
                profile.patchDatabaseId = "partial-preparation-test";
                shop.name = "partial-shop-" + Guid.NewGuid().ToString("N");
                shop.type = NpcDialogType.Shop;
                shop.sellItems = new[] { new NpcSellItem { item = items[0], amount = 1, sellPrice = 10 } };
                profile.scannedData = new ScriptableObject[] { shop };
                var selection = new System.Collections.Generic.HashSet<string> { nameof(NpcDialog) };
                var selected = await DataPatchSnapshotBuilder.Build(profile, null, System.Threading.CancellationToken.None, selection);
                Check(selected.Length == 1 && selected[0].dataType == nameof(NpcDialog), "Type selection exported unselected item records.");
                string itemPayload = JsonConvert.SerializeObject(new[]
                {
                    new DataPatchEntry { dataType = nameof(JunkItem), dataId = items[0].DataId, data = JObject.Parse("{\"sellPrice\":100}") },
                    new DataPatchEntry { dataType = nameof(JunkItem), dataId = items[1].DataId, data = JObject.Parse("{\"sellPrice\":200}") },
                    new DataPatchEntry { dataType = nameof(JunkItem), dataId = items[2].DataId, data = JObject.Parse("{\"sellPrice\":-1}") },
                });
                JObject shopData = DataPatchSerializer.Export(shop);
                shopData["sellItems"][0]["sellPrice"] = 1000;
                string firstShop = JsonConvert.SerializeObject(new[] { new DataPatchEntry { dataType = nameof(NpcDialog), dataId = shop.DataId, data = shopData } });
                shopData["sellItems"][0]["sellPrice"] = 2000;
                string nextShop = JsonConvert.SerializeObject(new[] { new DataPatchEntry { dataType = nameof(NpcDialog), dataId = shop.DataId, data = shopData } });
                var second = ChunkRelease("partial-second", itemPayload, nextShop);
                using (var engine = new DataPatchEngine(profile, refresh: false))
                {
                    await engine.PrepareAsync(ChunkRelease("partial-first", itemPayload, firstShop));
                    engine.Commit();
                    int firstCacheCount = engine.LastCacheTraversalCount;
                    Check(items[0].SellPrice == 100 && items[1].SellPrice == 200 && shop.sellItems[0].sellPrice == 1000, "Initial complete snapshot did not apply.");
                    // Scene loading may register unrelated assets after startup.
                    profile.scannedData = new ScriptableObject[] { shop, currency };
                    await engine.PrepareAsync(second);
                    Check(engine.LastPreparationRecordCount == 2, "Partial reload prepared unchanged valid records or failed to retry a rejected record after registry growth.");
                    Check(engine.LastCacheTraversalCount < firstCacheCount,
                        $"Partial reload rediscovered the complete cache graph ({engine.LastCacheTraversalCount} containers, previously {firstCacheCount}).");
                    engine.Commit();
                    Check(items[0].SellPrice == 100 && items[1].SellPrice == 200 && shop.sellItems[0].sellPrice == 2000, "Partial reload reset retained item patches or failed to update the shop.");
                    Check(engine.PreparationWarnings.Exists(warning => warning.Contains("Skipped record JunkItem")), "Unchanged rejected record lost its diagnostic.");
                    await engine.PrepareAsync(second);
                    Check(engine.LastPreparationRecordCount == 0 && engine.PreparedChangeCount == 0, "Repeated release was prepared again.");
                    engine.Commit();
                    await engine.PrepareAsync(ChunkRelease("full-replacement", null, nextShop));
                    engine.Commit();
                    Check(items[0].SellPrice == 10 && items[1].SellPrice == 11 && shop.sellItems[0].sellPrice == 2000, "Complete replacement stopped restoring omitted records to their baseline.");
                }
                Check(shop.sellItems[0].sellPrice == 10, "Rollback cache did not restore the shop baseline.");
                using (var freshClient = new DataPatchEngine(profile, refresh: false))
                {
                    await freshClient.PrepareAsync(second);
                    freshClient.Commit();
                    Check(items[0].SellPrice == 100 && items[1].SellPrice == 200 && shop.sellItems[0].sellPrice == 2000, "Fresh client needed previous releases to load the merged snapshot.");
                }
            }
            finally
            {
                if (GameInstance.NpcDialogs.TryGetValue(shop.DataId, out BaseNpcDialog registered) && registered == shop)
                    GameInstance.NpcDialogs.Remove(shop.DataId);
                foreach (JunkItem item in items)
                {
                    if (item == null)
                        continue;
                    if (GameInstance.Items.TryGetValue(item.DataId, out BaseItem registeredItem) && registeredItem == item)
                        GameInstance.Items.Remove(item.DataId);
                    UnityEngine.Object.DestroyImmediate(item);
                }
                if (GameInstance.Currencies.TryGetValue(currency.DataId, out Currency registeredCurrency) && registeredCurrency == currency)
                    GameInstance.Currencies.Remove(currency.DataId);
                UnityEngine.Object.DestroyImmediate(currency);
                UnityEngine.Object.DestroyImmediate(shop);
                UnityEngine.Object.DestroyImmediate(profile);
                UnityEngine.Object.DestroyImmediate(database);
                string resolved = System.IO.Path.GetFullPath(directory);
                Check(resolved.StartsWith(System.IO.Path.GetFullPath(System.IO.Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) && System.IO.Path.GetFileName(resolved).StartsWith("unity-partial-patch-", StringComparison.Ordinal), "Unsafe test cleanup path.");
                System.IO.Directory.Delete(resolved, true);
            }
        }

        public static void VerifyPatchZip()
        {
            var entries = new[]{new DataPatchEntry{dataType = "BuildingItem", dataId = -123, data = JObject.Parse("{\"sellPrice\":10}")}, new DataPatchEntry{dataType = "JunkItem", dataId = -123, data = JObject.Parse("{\"sellPrice\":20}")}, new DataPatchEntry{dataType = "JunkItem", dataId = 456, data = JObject.Parse("{\"sellPrice\":30}")}};
            using (var stream = new System.IO.MemoryStream())
            {
                DataPatchZipExporter.Write(stream, entries);
                stream.Position = 0;
                using (var zip = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read))
                {
                    Check(zip.Entries.Count == 3, "ZIP did not contain all reviewed records.");
                    Check(zip.GetEntry("JunkItem/-123.json") != null && zip.GetEntry("JunkItem/456.json") != null, "ZIP folders or item filenames are incorrect.");
                    using (var reader = new System.IO.StreamReader(zip.GetEntry("BuildingItem/-123.json").Open()))
                    {
                        var restored = JsonConvert.DeserializeObject<DataPatchEntry>(reader.ReadToEnd());
                        Check(restored.dataType == "BuildingItem" && restored.dataId == -123 && JToken.DeepEquals(restored.data, entries[0].data), "ZIP JSON did not round-trip the reviewed record.");
                    }
                }
            }

            using (var stream = new System.IO.MemoryStream())
                Reject(() => DataPatchZipExporter.Write(stream, new[]{entries[0], entries[0]}));
        }

        public static void VerifyJsonReviewScrolling()
        {
            string source = "first\r\n" + new string ('x', 56832400) + "\nlast";
            var text = new DataPatchJsonText(source);
            Check(text.LineCount == 3 && text.MaxColumns == 56832400, "Large JSON indexing lost content.");
            Check(text.Slice(0, 0, 100) == "first" && text.Slice(2, 0, 100) == "last", "First or last JSON line was truncated.");
            Check(text.Slice(1, 56832300, 200) == new string ('x', 100), "Horizontal scrolling lost the end of a long JSON line.");
            Check(text.Slice(1, 20000000, 200).Length == 200, "Renderer received more than the visible text.");
            var unicode = new DataPatchJsonText("a\U0001F600b");
            Check(unicode.Slice(0, 2, 1) == "\U0001F600", "Viewport split a Unicode character.");
            Check(new DataPatchJsonText("").Slice(0, 0, 100) == "", "Empty JSON preview failed.");
            var lines = new DataPatchJsonText(string.Join("\n", new string[100001]));
            Check(lines.LineCount == 100001 && lines.Slice(100000, 0, 80) == "", "Large line count was truncated.");
        }

        private static DataPatchRelease Release(DataPatchProfile database, string id, DataPatchEntry[] entries)
        {
            string json = JsonConvert.SerializeObject(entries);
            return new DataPatchRelease{id = id, databaseId = database.patchDatabaseId, schemaVersion = 1, publishTime = DateTime.UtcNow.ToString("O"), payloadJson = json, payloadHash = DataPatchHttp.Hash(json)};
        }

        private static void Check(bool success, string message)
        {
            if (!success)
                throw new Exception(message);
        }

        private static void Reject(Action action)
        {
            try
            {
                action();
            }
            catch (InvalidOperationException)
            {
                return;
            }

            throw new Exception("Invalid patch was accepted.");
        }

        private sealed class AmountHolder
        {
            public ItemAmount[] items;
            public AttributeAmount[] attributes;
        }


        private static void VerifyInt32AndIndexedCaches()
        {
            GameInstance.ClearData();
            var attribute = ScriptableObject.CreateInstance<Attribute>();
            attribute.Id = "patch-indexed-attribute";
            var currency = ScriptableObject.CreateInstance<Currency>();
            currency.Id = "patch-int-currency";
            var armor = ScriptableObject.CreateInstance<ArmorItem>();
            armor.Id = "patch-indexed-armor";
            var table = ScriptableObject.CreateInstance<ItemDropTable>();
            table.name = "patch-profile-only-table";
            var database = ScriptableObject.CreateInstance<GameDatabase>();
            var profile = ScriptableObject.CreateInstance<DataPatchProfile>();
            profile.database = database;
            profile.patchDatabaseId = "patch-indexed";
            profile.scannedData = new ScriptableObject[] { armor, attribute, currency, table };
            try
            {
                var registry = DataPatchRegistry.Runtime(database, profile.scannedData);
                Check(registry.Resolve(typeof(ItemDropTable), table.DataId) == table, "Scene-only table did not resolve from profile.");
                Check(typeof(BaseItem).GetProperty("SellPrice").PropertyType == typeof(int), "Item gold price must remain Int32.");
                Check(typeof(CurrencyAmount).GetField("amount").FieldType == typeof(int), "Custom currency must remain Int32.");
                var amount = new CurrencyHolder();
                DataPatchSerializer.ReadInto(amount, JObject.Parse("{\"amount\":2147483647}"), registry);
                Check(amount.amount == int.MaxValue, "Maximum Int32 failed to round-trip.");
                bool overflowRejected = false;
                try
                {
                    DataPatchSerializer.ReadInto(amount, JObject.Parse("{\"amount\":2147483648}"), registry);
                }
                catch (Exception ex) when (ex is OverflowException || ex is JsonException || ex is InvalidOperationException)
                {
                    overflowRejected = true;
                }
                Check(overflowRejected && amount.amount == int.MaxValue, "Out-of-range currency was widened or applied.");
                var quest = ScriptableObject.CreateInstance<Quest>();
                quest.Id = "patch-currency-total";
                try
                {
                    string row = "{\"currency\":{\"dataId\":" + currency.DataId + "},\"amount\":2147483647}";
                    Reject(() => DataPatchSerializer.ReadInto(quest, JObject.Parse("{\"rewardCurrencies\":[" + row + "," + row + "]}"), registry));
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(quest);
                }
                _ = armor.RequireAttributeAmounts;
                using (var engine = new DataPatchEngine(profile, false))
                {
                    var entry = new DataPatchEntry
                    {
                        dataType = nameof(ArmorItem),
                        dataId = armor.DataId,
                        data = JObject.Parse("{\"requirement\":{\"attributeAmounts\":[{\"attribute\":{\"dataId\":" + attribute.DataId + "},\"amount\":5}]}}")
                    };
                    engine.Prepare(Release(profile, "indexed", new[] { entry }));
                    engine.Commit();
                    Check(armor.RequireAttributeAmounts[attribute.RuntimeSlot] == 5, "Indexed item requirement cache stayed stale.");
                    engine.Prepare(null);
                    engine.Commit();
                    Check(armor.RequireAttributeAmounts[attribute.RuntimeSlot] == 0, "Indexed cache did not roll back.");
                }
                var bonus = new EquipmentBonus();
                _ = bonus.Attributes;
                typeof(EquipmentBonus).GetField("attributes", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(bonus, new[] { new AttributeAmount { attribute = attribute, amount = 7 } });
                bonus.ClearPatchCaches();
                Check(bonus.IndexedAttributes[attribute.RuntimeSlot] == 7, "Equipment bonus cache stayed stale.");
            }
            finally
            {
                foreach (var asset in new ScriptableObject[] { armor, attribute, currency, table, database, profile })
                    UnityEngine.Object.DestroyImmediate(asset);
                GameInstance.ClearData();
            }
        }

        private sealed class CurrencyHolder
        {
            public int amount;
        }
    }
}
