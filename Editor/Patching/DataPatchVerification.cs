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
                VerifyItemCategoryValidation();
                VerifyTolerantRecords();
                VerifyRegistrySecondaryLookups();
                VerifyChunkedTransport();
                await VerifyPreparationPerformance();
                Debug.Log("DATA_PATCH_PREPARATION_PERFORMANCE_PASSED");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
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
            return new DataPatchRelease{id = id, databaseId = database.patchDatabaseId, environment = database.dataPatchSettings.environment, schemaVersion = 1, publishTime = DateTime.UtcNow.ToString("O"), payloadJson = json, payloadHash = DataPatchHttp.Hash(json)};
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
