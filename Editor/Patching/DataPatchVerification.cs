using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Cysharp.Text;

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

        public static void Verify()
        {
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
            using (var engine = new DataPatchEngine(profile))
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
                using (var engine = new DataPatchEngine(profile))
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
