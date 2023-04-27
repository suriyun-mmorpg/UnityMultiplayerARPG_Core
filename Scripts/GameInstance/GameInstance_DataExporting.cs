#if UNITY_EDITOR
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class GameInstance
    {
        [Header("Data Exporting")]
        [InspectorButton(nameof(ExportSocialSystemSettingAsJson))]
        public bool exportSocialSystemSettingAsJson;
        public void ExportSocialSystemSettingAsJson()
        {
            SocialSystemSetting socialSystemSetting = this.socialSystemSetting;
            if (socialSystemSetting == null)
                socialSystemSetting = ScriptableObject.CreateInstance<SocialSystemSetting>();
            string path = EditorUtility.SaveFilePanel("Export Social System Setting", Application.dataPath, "socialSystemSetting", "json");
            if (path.Length > 0)
                File.WriteAllText(path, JsonConvert.SerializeObject(socialSystemSetting, Formatting.Indented));
        }

        [InspectorButton(nameof(ExportItemsAsJson))]
        public bool exportItemsAsJson;
        public void ExportItemsAsJson()
        {
            ClearData();
            onGameDataLoaded += OnGameDataLoadedToExportItemsAsJson;
            gameDatabase.LoadData(this).Forget();
        }

        private void OnGameDataLoadedToExportItemsAsJson()
        {
            onGameDataLoaded -= OnGameDataLoadedToExportItemsAsJson;
            Dictionary<int, Dictionary<string, object>> exportingItems = new Dictionary<int, Dictionary<string, object>>();
            foreach (var kv in Items)
            {
                exportingItems[kv.Key] = new Dictionary<string, object>()
                {
                    { "Id", kv.Value.Id },
                    { "DataId", kv.Value.DataId },
                    { "ItemType", (byte)kv.Value.ItemType },
                    { "SellPrice", kv.Value.SellPrice },
                    { "Weight", kv.Value.Weight },
                    { "MaxStack", kv.Value.MaxStack },
                    { "LockDuration", kv.Value.LockDuration },
                    { "ExpireDuration", kv.Value.ExpireDuration },
                };
            }
            string path = EditorUtility.SaveFilePanel("Export Items", Application.dataPath, "items", "json");
            if (path.Length > 0)
                File.WriteAllText(path, JsonConvert.SerializeObject(exportingItems, Formatting.Indented, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new GameDataContractResolver(),
                }));
        }
    }
}
#endif