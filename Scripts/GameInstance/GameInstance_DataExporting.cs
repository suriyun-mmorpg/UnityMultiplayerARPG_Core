#if UNITY_EDITOR
using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class GameInstance
    {
        [Header("Data Exporting")]
        [InspectorButton(nameof(ExportSocialSystemSetting))]
        public bool exportSocialSystemSetting;

        public void ExportSocialSystemSetting()
        {
            SocialSystemSetting socialSystemSetting = this.socialSystemSetting;
            if (socialSystemSetting == null)
                socialSystemSetting = ScriptableObject.CreateInstance<SocialSystemSetting>();
            string path = EditorUtility.SaveFilePanel("Export Social System Setting", Application.dataPath, "socialSystemSetting", "json");
            if (path.Length > 0)
                File.WriteAllText(path, JsonConvert.SerializeObject(socialSystemSetting));
        }
    }
}
#endif