using LiteNetLibManager;
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public class AssetEditorMenu
    {
        [MenuItem(EditorMenuConsts.VALIDATE_GAME_DATA_AND_PREFABS_MENU, false, EditorMenuConsts.VALIDATE_GAME_DATA_AND_PREFABS_ORDER)]
        public static void ValidateGameDataAndPrefabs()
        {
            Debug.Log("MMORPG KIT: Validating game data and prefabs...");
            string[] guids = AssetDatabase.FindAssets("t:BaseGameData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BaseGameData gameData = AssetDatabase.LoadAssetAtPath<BaseGameData>(path);
                if (gameData == null) continue;
                if (gameData.Validate())
                {
                    Debug.Log($"Validated, and changed game data {gameData.name} at path {path}");
                    EditorUtility.SetDirty(gameData);
                }
            }

            guids = AssetDatabase.FindAssets("t:BaseGameDatabase");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BaseGameDatabase gameDatabase = AssetDatabase.LoadAssetAtPath<BaseGameDatabase>(path);
                if (gameDatabase == null) continue;
                if (gameDatabase.Validate())
                {
                    Debug.Log($"Validated, and changed game data {gameDatabase.name} at path {path}");
                    EditorUtility.SetDirty(gameDatabase);
                }
            }

            guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (ValidatePrefab(prefab))
                {
                    EditorUtility.SetDirty(prefab);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("MMORPG KIT: Validating game data and prefabs completed.");
        }

        public static bool ValidatePrefab(GameObject prefab)
        {
            if (prefab == null)
                return false;
            LiteNetLibIdentity identity = prefab.GetComponent<LiteNetLibIdentity>();
            bool hasChanges = false;
            if (identity != null)
            {
                string assetId = identity.AssetId;
                identity.AssignAssetID();
                string newAssetId = identity.AssetId;
                if (!string.Equals(assetId, newAssetId))
                {
                    Debug.Log($"Assigned Asset ID {newAssetId} (from {assetId}) to prefab {prefab.name}");
                    hasChanges = true;
                }
            }
            GameInstance gameInstance = prefab.GetComponent<GameInstance>();
            if (gameInstance != null && gameInstance.Validate())
            {
                Debug.Log($"Validated, and changed game instance {gameInstance.name}");
                hasChanges = true;
            }
            GameSpawnArea[] spawnAreas = prefab.GetComponentsInChildren<GameSpawnArea>(true);
            foreach (GameSpawnArea spawnArea in spawnAreas)
            {
                if (spawnArea.Validate())
                {
                    Debug.Log($"Validated, and changed game spawn area {spawnArea.name}");
                    hasChanges = true;
                }
            }
            return hasChanges;
        }

        [MenuItem(EditorMenuConsts.VALIDATE_OPENED_SCENES_MENU, false, EditorMenuConsts.VALIDATE_OPENED_SCENES_ORDER)]
        public static void ValidateOpenedScenes()
        {
            Debug.Log("MMORPG KIT: Validating opened scenes...");
            GameSpawnArea.s_ValidateAllGameSpawnAreas();
            LiteNetLibIdentity.s_AssignSceneObjectIDs();
            Debug.Log("MMORPG KIT: Validating opened scenes completed.");
        }
    }
}
