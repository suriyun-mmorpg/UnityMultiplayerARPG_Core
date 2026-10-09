using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(DataPatchProfile))]
    public sealed class DataPatchProfileEditor : Editor
    {
        private string status;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var profile = (DataPatchProfile)target;
            EditorGUILayout.LabelField("Scanned Assets", profile.scannedData.Length.ToString());
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode || profile.database == null))
            {
                if (GUILayout.Button("Scan Data and Prefab Dependencies"))
                {
                    try
                    {
                        status = $"Found {DataPatchDependencyScanner.Scan(profile).Targets.Count} data assets. Scan errors are recorded in Show Log.";
                        AssetDatabase.SaveAssets();
                    }
                    catch (System.Exception ex)
                    {
                        status = ex.Message;
                    }
                }

                if (GUILayout.Button("Review Patch Data / Create Patch"))
                    DataPatchExportWindow.Open(profile);
            }

            using (new EditorGUI.DisabledScope(!DataPatchBuildLog.HasLog(profile)))
                if (GUILayout.Button("Show Log"))
                    DataPatchBuildLog.Show(profile);
            if (GUILayout.Button("Configure Upload Secret"))
                Selection.activeObject = PatchUploadConfig.LoadOrCreate();
            EditorGUILayout.HelpBox("Assign this profile to GameInstance for runtime patch loading. Scan dependencies again after changing source assets. Upload credentials remain editor-only.", MessageType.Info);
            if (!string.IsNullOrEmpty(status))
                EditorGUILayout.HelpBox(status, MessageType.Info);
        }
    }
}
