using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public class GameDataValidatorTool : EditorWindow
    {
        [MenuItem(EditorMenuConsts.BUILD_VALIDATE_GAME_DATA_MENU, false, EditorMenuConsts.BUILD_VALIDATE_GAME_DATA_ORDER)]
        public static void ValidateGameData()
        {
            // Find all ScriptableObject assets in the project
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (obj is IGameDataValidation validator)
                {
                    validator.OnValidateGameData();
                }
            }

            Debug.Log("All game data validated.");
        }
    }
}
