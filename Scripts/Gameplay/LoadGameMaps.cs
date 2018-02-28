using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshSurface))]
public class LoadGameMaps : MonoBehaviour
{
    // I think only x axis is enough for simulate map on server, if not I will make map simulate on z axis later
    public const float MIN_MAP_X = -90000;
    public float offsetBetweenBounds = 10f;
    public bool loadMapsOnStart;
    public bool loadScene;
    public bool IsDone { get; private set; }
    private int loadedMapCount = 0;
    private GameMap loadingGameMap = null;
    private float loadOffsetX = 0f;

    public System.Action onLoadedMaps;

    private NavMeshSurface tempNavMeshSurface;
    public NavMeshSurface TempNavMeshSurface
    {
        get
        {
            if (tempNavMeshSurface == null)
                tempNavMeshSurface = GetComponent<NavMeshSurface>();
            return tempNavMeshSurface;
        }
    }
    public readonly Dictionary<string, GameMapEntity> LoadedMaps = new Dictionary<string, GameMapEntity>();

    public float Progress
    {
        get { return (float)loadedMapCount / (float)GameInstance.GameMaps.Count; }
    }

    private void Awake()
    {
        TempNavMeshSurface.collectObjects = CollectObjects.Children;
        TempNavMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
    }

    private void Start()
    {
        if (loadMapsOnStart)
            LoadMaps();
    }

    public Coroutine LoadMaps()
    {
        return StartCoroutine(LoadMapsRoutine());
    }

    private IEnumerator LoadMapsRoutine()
    {
        IsDone = false;
        yield return null;
        var gameMaps = GameInstance.GameMaps.Values;
        foreach (var gameMap in gameMaps)
        {
            loadingGameMap = gameMap;
            yield return null;

            if (gameMap.physicPrefab == null || LoadedMaps.ContainsKey(gameMap.sceneName))
                continue;

            var mapBoundsExtents = loadingGameMap.mapBounds.extents;
            var physicPrefab = Instantiate(gameMap.physicPrefab, transform);
            var mapEntity = physicPrefab.GetComponent<GameMapEntity>();
            var defaultMapEntityPosition = mapEntity.transform.position;
            if (loadedMapCount == 0)
            {
                mapEntity.transform.position =
                    (Vector3.right * (MIN_MAP_X + mapBoundsExtents.x));
            }
            else
            {
                mapEntity.transform.position =
                    (Vector3.right * (loadOffsetX + mapBoundsExtents.x + offsetBetweenBounds));
            }
            loadOffsetX = mapEntity.transform.position.x + mapBoundsExtents.x;
            var loadedMapOffset = mapEntity.transform.position - defaultMapEntityPosition;
            // Now we keep loaded map offsets that will be uses at client side
            // To spawn map at (defaultMapEntityPosition + loadedMapOffset)
            // Saving character position as (spawnMapPosition - loadedMapOffset)
            mapEntity.SceneName = gameMap.sceneName;
            mapEntity.MapOffsets = loadedMapOffset;
            LoadedMaps[gameMap.sceneName] = mapEntity;
            if (loadScene)
            {
                SceneManager.LoadScene(gameMap.sceneName, LoadSceneMode.Additive);
                var scene = SceneManager.GetSceneByName(gameMap.sceneName);
                yield return null;
                var rootObjects = scene.GetRootGameObjects();
                foreach (var rootObject in rootObjects)
                {
                    var position = rootObject.transform.position;
                    rootObject.transform.position = position + loadedMapOffset;
                    // Remove all colliders/cameras/audio listeners
                    rootObject.RemoveComponentsInChildren<Collider>(true);
                    rootObject.RemoveComponentsInChildren<AudioListener>(true);
                    rootObject.RemoveComponentsInChildren<FlareLayer>(true);
                    rootObject.RemoveComponentsInChildren<Camera>(true);
                }
            }
            loadingGameMap = null;
            loadedMapCount++;
        }
        TempNavMeshSurface.BuildNavMesh();
        IsDone = true;
        if (onLoadedMaps != null)
            onLoadedMaps.Invoke();
    }

    public GameMapEntity GetMapByWorldPosition(Vector3 position)
    {
        var loadedMaps = LoadedMaps.Values;
        foreach (var loadedMap in loadedMaps)
        {
            if (loadedMap.IsInMap(position))
                return loadedMap;
        }
        return null;
    }
}
