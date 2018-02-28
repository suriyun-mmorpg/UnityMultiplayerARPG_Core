using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameMapEntity : MonoBehaviour
{
    public string SceneName { get; set; }
    public Vector3 MapOffsets { get; set; }
    public Bounds MapBounds { get; private set; }
    public GameObject PhysicsPrefab { get; private set; }

    public bool IsInMap(Vector3 position)
    {
        return position.x >= MapBounds.min.x &&
            position.x <= MapBounds.max.x &&
            position.z >= MapBounds.min.z &&
            position.z <= MapBounds.max.z;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(MapBounds.center, MapBounds.size);
    }

    private void OnValidate()
    {
        GenerateMapBounds();
    }

    [ContextMenu("Generate Map Bounds")]
    public void GenerateMapBounds()
    {
        var children = GetComponentsInChildren<Collider>();
        if (children.Length > 0)
        {
            Bounds bounds = children[0].bounds;
            for (var i = 1; i < children.Length; ++i)
            {
                var child = children[i];
                if (child == null)
                    continue;
                bounds.Encapsulate(child.bounds);
            }
            MapBounds = bounds;
        }
    }

    [ContextMenu("Generate Physics Prefab")]
    public void GeneratePhysicsPrefab()
    {
        if (PhysicsPrefab != null)
            DestroyImmediate(PhysicsPrefab);

        PhysicsPrefab = Instantiate(gameObject);
        PhysicsPrefab.transform.parent = null;
        PhysicsPrefab.transform.position = transform.position;
        PhysicsPrefab.transform.rotation = transform.rotation;
        PhysicsPrefab.transform.localScale = transform.localScale;

        var components = PhysicsPrefab.GetComponentsInChildren<Component>();
        foreach (var component in components)
        {
            if (component is Transform || component is GameMapEntity || component is Collider)
                continue;

            DestroyImmediate(component);
        }
    }

    [ContextMenu("Make Game Map")]
    public void MakeGameMap()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        var fileName = sceneName + "_GameMap";
        var fileExt = "asset";
        var path = EditorUtility.SaveFilePanelInProject("Make Game Map...", fileName, fileExt, "Please enter a file name to save game map");
        if (path.Length > 0)
        {
            GenerateMapBounds();
            GeneratePhysicsPrefab();

            var gameMap = ScriptableObject.CreateInstance<GameMap>();
            gameMap.sceneName = sceneName;
            gameMap.mapBounds = MapBounds;

            var savingDirectory = Path.GetDirectoryName(path);
            var savingFileName = path.Substring(savingDirectory.Length);
            var physicsPrefabFileName = savingFileName.Substring(0, savingFileName.Length - fileExt.Length - 1) + "_Physics.prefab";
            var physicsPrefabPath = savingDirectory + physicsPrefabFileName;
            AssetDatabase.DeleteAsset(physicsPrefabPath);
            var resultPhysicPrefab = PrefabUtility.CreatePrefab(physicsPrefabPath, PhysicsPrefab);

            gameMap.physicPrefab = resultPhysicPrefab;
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(gameMap, path);

            DestroyImmediate(PhysicsPrefab);
        }
        else
            Debug.LogWarning("Invalid game map save path");
    }
#endif
}
