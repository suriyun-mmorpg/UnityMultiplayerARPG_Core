using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class LoadGameMaps : MonoBehaviour
{
    // I think only x axis is enough for simulate map on server, if not I will make map simulate on z axis later
    public const float MIN_MAP_X = -90000;
    public GameMap[] gameMaps;
    public float offsetBetweenBounds = 10f;
    public bool loadMapsOnStart;
    private int loadedMapCount = 0;
    private GameMap loadingGameMap = null;
    private float loadOffsetX = 0f;

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
    public readonly Dictionary<string, GameMapEntity> LoadedMap = new Dictionary<string, GameMapEntity>();

    public float LoadedPercentage
    {
        get { return (float)loadedMapCount / (float)gameMaps.Length * 100f; }
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

    public void LoadMaps()
    {
        StartCoroutine(LoadMapsRoutine());
    }

    private IEnumerator LoadMapsRoutine()
    {
        yield return 0;
        foreach (var gameMap in gameMaps)
        {
            loadingGameMap = gameMap;
            yield return 0;

            if (gameMap.physicPrefab == null || LoadedMap.ContainsKey(gameMap.sceneName))
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
            mapEntity.MapOffsets = loadedMapOffset;
            LoadedMap[gameMap.sceneName] = mapEntity;
            loadingGameMap = null;
            loadedMapCount++;
        }
        TempNavMeshSurface.BuildNavMesh();
    }
}
