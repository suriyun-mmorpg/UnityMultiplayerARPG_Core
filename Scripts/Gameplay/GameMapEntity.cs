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
    public string MapName { get; set; }
    public Vector3 MapExtents { get; set; }
    public Vector3 MapOffsets { get; set; }
    private GameObject physicsPrefab;

    private Transform tempTransform;
    public Transform TempTransform
    {
        get
        {
            if (tempTransform == null)
                tempTransform = GetComponent<Transform>();
            return tempTransform;
        }
    }

    public bool IsWorldPositionInMap(Vector3 position)
    {
        var minX = TempTransform.position.x - MapExtents.x;
        var minZ = TempTransform.position.z - MapExtents.z;
        var maxX = TempTransform.position.x + MapExtents.x;
        var maxZ = TempTransform.position.z + MapExtents.z;
        return position.x >= minX &&
            position.x <= maxX &&
            position.z >= minZ &&
            position.z <= maxZ;
    }

    public bool IsLocalPositionInMap(Vector3 position)
    {
        return IsWorldPositionInMap(ConvertLocalPositionToWorld(position));
    }

    public Vector3 ConvertWorldPositionToLocal(Vector3 worldPosition)
    {
        var localPosition = worldPosition - MapOffsets;
        return localPosition;
    }

    public Vector3 ConvertLocalPositionToWorld(Vector3 localPosition)
    {
        var worldPosition = localPosition + MapOffsets;
        return worldPosition;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, MapExtents * 2);
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
            Bounds mapBounds = new Bounds();
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
                mapBounds = bounds;
            }

            if (physicsPrefab != null)
                DestroyImmediate(physicsPrefab);

            physicsPrefab = Instantiate(gameObject);
            physicsPrefab.transform.parent = null;
            physicsPrefab.transform.position = transform.position;
            physicsPrefab.transform.rotation = transform.rotation;
            physicsPrefab.transform.localScale = transform.localScale;

            var components = physicsPrefab.GetComponentsInChildren<Component>();
            foreach (var component in components)
            {
                if (component is Transform || component is GameMapEntity || component is Collider)
                    continue;

                DestroyImmediate(component);
            }

            var gameMap = ScriptableObject.CreateInstance<GameMap>();
            gameMap.mapName = sceneName;
            gameMap.mapExtents = mapBounds.extents;

            var savingDirectory = Path.GetDirectoryName(path);
            var savingFileName = path.Substring(savingDirectory.Length);
            var physicsPrefabFileName = savingFileName.Substring(0, savingFileName.Length - fileExt.Length - 1) + "_Physics.prefab";
            var physicsPrefabPath = savingDirectory + physicsPrefabFileName;
            AssetDatabase.DeleteAsset(physicsPrefabPath);
            var resultPhysicPrefab = PrefabUtility.CreatePrefab(physicsPrefabPath, physicsPrefab);

            gameMap.physicPrefab = resultPhysicPrefab;
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(gameMap, path);

            DestroyImmediate(physicsPrefab);
        }
        else
            Debug.LogWarning("Invalid game map save path");
    }
#endif
}
