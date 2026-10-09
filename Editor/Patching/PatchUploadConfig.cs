using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public sealed class PatchUploadConfig : ScriptableObject
    {
        public string serviceUrl = "";
        public string secretKey = "";
        public int requestTimeoutSeconds = 30;
        public string pendingUploadId = "";
        public string pendingPayloadHash = "";
        public string pendingDatabaseId = "";
        public const string AssetPath = "Assets/Editor/Resources/DataPatching/PatchUploadConfig.asset";

        public static PatchUploadConfig LoadOrCreate()
        {
            var config = AssetDatabase.LoadAssetAtPath<PatchUploadConfig>(AssetPath);
            if (config != null)
                return config;
            string parent = "Assets";
            foreach (string directory in new[]{"Editor", "Resources", "DataPatching"})
            {
                string child = parent + "/" + directory;
                if (!AssetDatabase.IsValidFolder(child))
                    AssetDatabase.CreateFolder(parent, directory);
                parent = child;
            }

            config = CreateInstance<PatchUploadConfig>();
            AssetDatabase.CreateAsset(config, AssetPath);
            AssetDatabase.SaveAssets();
            return config;
        }
    }

    [InitializeOnLoad]
    public static class PatchUploadConfigInitializer
    {
        static PatchUploadConfigInitializer()
        {
            EditorApplication.delayCall += EnsureConfig;
        }

        private static void EnsureConfig()
        {
            if (!EditorApplication.isCompiling)
                PatchUploadConfig.LoadOrCreate();
        }
    }

    [CustomEditor(typeof(PatchUploadConfig))]
    public sealed class PatchUploadConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var config = (PatchUploadConfig)target;
            EditorGUI.BeginChangeCheck();
            config.secretKey = EditorGUILayout.PasswordField("Upload Secret", config.secretKey);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(config);
            EditorGUILayout.HelpBox("The patch profile supplies the endpoint and database ID. Local editor credential. Set a key accepted by PATCH_UPLOAD_SECRET_KEYS. This asset must stay out of Git and player builds.", MessageType.Info);
        }
    }
}
