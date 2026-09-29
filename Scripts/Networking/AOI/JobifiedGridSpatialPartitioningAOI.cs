using Insthync.SpatialPartitioningSystems;
using LiteNetLibManager;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;

namespace MultiplayerARPG
{
    public class JobifiedGridSpatialPartitioningAOI : BaseInterestManager
    {
        private const int InitialResultCapacity = 64;
        protected static readonly ProfilerMarker s_UpdateProfilerMarker = new ProfilerMarker("JobifiedGridSpatialPartitioningAOI - Update");
        protected static readonly ProfilerMarker s_CompleteProfilerMarker = new ProfilerMarker("JobifiedGridSpatialPartitioningAOI - Complete");

        public float cellSize = 64f;
        public int maxObjects = 10000;
        [Tooltip("Update every ? seconds")]
        public float updateInterval = 1.0f;
        public Vector3 bufferedCells = Vector3.one;
        public Color boundsGizmosColor = Color.green;

        private JobifiedGridSpatialPartitioningSystem _system;
        private float _updateCountDown;
        private Bounds _bounds;
        private Dictionary<uint, HashSet<uint>> _playerSubscribings = new Dictionary<uint, HashSet<uint>>();
        private HashSet<uint> _activePlayerObjectIds = new HashSet<uint>();
        private List<uint> _stalePlayerObjectIds = new List<uint>();
        private HashSet<uint> _alwaysVisibleObjects = new HashSet<uint>();
        private Queue<NativeList<SpatialObject>> _queryQueue = new Queue<NativeList<SpatialObject>>();
        private Dictionary<uint, NativeList<SpatialObject>> _queryingPlayerSubscribings = new Dictionary<uint, NativeList<SpatialObject>>();
        private Dictionary<uint, NativeList<SpatialObject>> _queryingComponentSubscribings = new Dictionary<uint, NativeList<SpatialObject>>();
        private bool _isQuerying = false;

        public NativeList<SpatialObject> ReserveResultList()
        {
            if (_queryQueue.Count > 0)
            {
                return _queryQueue.Dequeue();
            }
            else
            {
                return new NativeList<SpatialObject>(InitialResultCapacity, Allocator.Persistent);
            }
        }

        public void ReturnResultList(NativeList<SpatialObject> resultList)
        {
            resultList.Clear();
            _queryQueue.Enqueue(resultList);
        }

        public void DisposeResultLists()
        {
            foreach (var resultList in _queryingPlayerSubscribings)
            {
                if (resultList.Value.IsCreated)
                    resultList.Value.Dispose();
            }
            foreach (var resultList in _queryingComponentSubscribings)
            {
                if (resultList.Value.IsCreated)
                    resultList.Value.Dispose();
            }
            while (_queryQueue.Count > 0)
            {
                NativeList<SpatialObject> resultList = _queryQueue.Dequeue();
                if (resultList.IsCreated)
                    resultList.Dispose();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Color color = Gizmos.color;
            Gizmos.color = boundsGizmosColor;
            Gizmos.DrawWireCube(_bounds.center, _bounds.size);
            Gizmos.color = color;
        }

        private void OnDestroy()
        {
            ResetSystem();
            DisposeResultLists();
        }

        private void ResetSystem()
        {
            _system?.Dispose();
            _system = null;
            _isQuerying = false;

            foreach (var resultList in _queryingPlayerSubscribings.Values)
                ReturnResultList(resultList);
            _queryingPlayerSubscribings.Clear();
            foreach (var resultList in _queryingComponentSubscribings.Values)
                ReturnResultList(resultList);
            _queryingComponentSubscribings.Clear();

            _playerSubscribings.Clear();
            _activePlayerObjectIds.Clear();
            _stalePlayerObjectIds.Clear();
            _alwaysVisibleObjects.Clear();
        }

        public override void Setup(LiteNetLibGameManager manager)
        {
            base.Setup(manager);
            manager.Assets.onLoadSceneFinish.RemoveListener(OnLoadSceneFinish);
            PrepareSystem();
            manager.Assets.onLoadSceneFinish.AddListener(OnLoadSceneFinish);
        }

        private void OnLoadSceneFinish(string sceneName, bool isAdditive, bool isOnline, float progress)
        {
            if (!IsServer || !isOnline)
            {
                ResetSystem();
                return;
            }
            PrepareSystem();
        }

        public void PrepareSystem()
        {
            ResetSystem();
            if (!IsServer || !Manager.ServerSceneInfo.HasValue)
                return;

            var mapBounds = GenericUtils.GetComponentsFromAllLoadedScenes<AOIMapBounds>(true);
            if (mapBounds.Count > 0)
            {
                _bounds = mapBounds[0].GetBounds();
                for (int i = 0; i < mapBounds.Count; ++i)
                {
                    _bounds.Encapsulate(mapBounds[i].GetBounds());
                }
                _bounds.extents += bufferedCells * cellSize * 2;
                switch (GameInstance.Singleton.DimensionType)
                {
                    case DimensionType.Dimension3D:
                        _system = new JobifiedGridSpatialPartitioningSystem(_bounds, cellSize, maxObjects, false, true, false);
                        break;
                    case DimensionType.Dimension2D:
                        _system = new JobifiedGridSpatialPartitioningSystem(_bounds, cellSize, maxObjects, false, false, true);
                        break;
                }
            }
            else
            {
                switch (GameInstance.Singleton.DimensionType)
                {
                    case DimensionType.Dimension3D:
                        var collider3Ds = GenericUtils.GetComponentsFromAllLoadedScenes<Collider>(true);
                        if (collider3Ds.Count > 0)
                        {
                            _bounds = collider3Ds[0].bounds;
                            for (int i = 1; i < collider3Ds.Count; ++i)
                            {
                                _bounds.Encapsulate(collider3Ds[i].bounds);
                            }
                            _bounds.extents += bufferedCells * cellSize * 2;
                            _system = new JobifiedGridSpatialPartitioningSystem(_bounds, cellSize, maxObjects, false, true, false);
                        }
                        break;
                    case DimensionType.Dimension2D:
                        var collider2Ds = GenericUtils.GetComponentsFromAllLoadedScenes<Collider2D>(true);
                        if (collider2Ds.Count > 0)
                        {
                            _bounds = collider2Ds[0].bounds;
                            for (int i = 1; i < collider2Ds.Count; ++i)
                            {
                                _bounds.Encapsulate(collider2Ds[i].bounds);
                            }
                            _bounds.extents += bufferedCells * cellSize * 2;
                            _system = new JobifiedGridSpatialPartitioningSystem(_bounds, cellSize, maxObjects, false, false, true);
                        }
                        break;
                }
            }
        }

        public override void UpdateInterestManagementImmediate()
        {
            _updateCountDown = 0f;
            UpdateInterestManagement(0f);
        }

        public override void UpdateInterestManagement(float deltaTime)
        {
            if (_system == null)
                return;

            Manager.Assets.manuallyApplyOwnerChanges = true;
            _updateCountDown -= deltaTime;
            if (_updateCountDown > 0)
                return;
            _updateCountDown = updateInterval;

            using (s_UpdateProfilerMarker.Auto())
            {
                _system.ClearObjects();
                var players = Manager.GetPlayers();
                while (players.MoveNext())
                {
                    LiteNetLibPlayer player = players.Current.Value;
                    if (!player.IsReady)
                    {
                        // Don't subscribe if player not ready
                        continue;
                    }
                    var playerObjs = player.GetSpawnedObjects();
                    while (playerObjs.MoveNext())
                    {
                        LiteNetLibIdentity playerObj = playerObjs.Current.Value;
                        _system.AddObjectToGrid(new SpatialObject()
                        {
                            objectId = playerObj.ObjectId,
                            position = playerObj.transform.position,
                        });
                    }
                }
                _system.UpdateGrid();

                _alwaysVisibleObjects.Clear();
                var spawnedObjects = Manager.Assets.GetSpawnedObjects();
                while (spawnedObjects.MoveNext())
                {
                    LiteNetLibIdentity spawnedObject = spawnedObjects.Current.Value;
                    if (spawnedObject == null)
                        continue;
                    if (spawnedObject.AlwaysVisible)
                    {
                        _alwaysVisibleObjects.Add(spawnedObject.ObjectId);
                        continue;
                    }
                    NativeList<SpatialObject> nativeQueryResult = ReserveResultList();
                    _queryingPlayerSubscribings[spawnedObject.ObjectId] = nativeQueryResult;
                    _system.QuerySphere(spawnedObject.transform.position, GetVisibleRange(spawnedObject), nativeQueryResult);
                }

                var components = SpatialObjectContainer.GetEnumerator();
                while (components.MoveNext())
                {
                    ISpatialObjectComponent component = components.Current.Value;
                    if (component == null)
                        continue;
                    component.ClearSubscribers();
                    if (!component.SpatialObjectEnabled)
                        continue;
                    NativeList<SpatialObject> nativeQueryResult = ReserveResultList();
                    _queryingComponentSubscribings[component.SpatialObjectId] = nativeQueryResult;
                    switch (component.SpatialObjectShape)
                    {
                        case SpatialObjectShape.Box:
                            _system.QueryBox(component.SpatialObjectPosition, component.SpatialObjectExtents, nativeQueryResult);
                            break;
                        default:
                            _system.QuerySphere(component.SpatialObjectPosition, component.SpatialObjectRadius, nativeQueryResult);
                            break;
                    }
                }

                _isQuerying = true;
            }
        }

        private void LateUpdate()
        {
            if (_isQuerying)
            {
                _isQuerying = false;

                using (s_CompleteProfilerMarker.Auto())
                {
                    _system.Complete();

                    NativeList<SpatialObject> queryResult;
                    HashSet<uint> subscribings;
                    LiteNetLibIdentity foundPlayerObject;
                    foreach (var playerQueryKvp in _queryingPlayerSubscribings)
                    {
                        queryResult = playerQueryKvp.Value;
                        if (Manager.Assets.TryGetSpawnedObject(playerQueryKvp.Key, out LiteNetLibIdentity spawnedObject))
                        {
                            for (int i = 0; i < queryResult.Length; ++i)
                            {
                                uint contactedObjectId = queryResult[i].objectId;
                                if (!Manager.Assets.TryGetSpawnedObject(contactedObjectId, out foundPlayerObject))
                                    continue;
                                if (!ShouldSubscribe(foundPlayerObject, spawnedObject, false))
                                    continue;
                                if (!_playerSubscribings.TryGetValue(contactedObjectId, out subscribings))
                                    subscribings = new HashSet<uint>();
                                subscribings.Add(spawnedObject.ObjectId);
                                _playerSubscribings[contactedObjectId] = subscribings;
                            }
                        }
                        ReturnResultList(queryResult);
                    }
                    _queryingPlayerSubscribings.Clear();

                    foreach (var componentQueryKvp in _queryingComponentSubscribings)
                    {
                        queryResult = componentQueryKvp.Value;
                        if (SpatialObjectContainer.TryGet(componentQueryKvp.Key, out ISpatialObjectComponent component))
                        {
                            for (int i = 0; i < queryResult.Length; ++i)
                            {
                                uint contactedObjectId = queryResult[i].objectId;
                                if (Manager.Assets.TryGetSpawnedObject(contactedObjectId, out foundPlayerObject))
                                    component.AddSubscriber(foundPlayerObject.ObjectId);
                            }
                        }
                        ReturnResultList(queryResult);
                    }
                    _queryingComponentSubscribings.Clear();

                    _activePlayerObjectIds.Clear();
                    var players = Manager.GetPlayers();
                    while (players.MoveNext())
                    {
                        LiteNetLibPlayer player = players.Current.Value;
                        if (!player.IsReady)
                        {
                            // Don't subscribe if player not ready
                            continue;
                        }
                        var playerObjs = player.GetSpawnedObjects();
                        while (playerObjs.MoveNext())
                        {
                            LiteNetLibIdentity playerObj = playerObjs.Current.Value;
                            _activePlayerObjectIds.Add(playerObj.ObjectId);
                            if (_playerSubscribings.TryGetValue(playerObj.ObjectId, out subscribings))
                            {
                                if (_alwaysVisibleObjects.Count > 0)
                                {
                                    foreach (uint alwaysVisibleObject in _alwaysVisibleObjects)
                                    {
                                        subscribings.Add(alwaysVisibleObject);
                                    }
                                }
                                playerObj.UpdateSubscribings(subscribings);
                                subscribings.Clear();
                            }
                            else if (_alwaysVisibleObjects.Count > 0)
                            {
                                playerObj.UpdateSubscribings(_alwaysVisibleObjects);
                            }
                            else
                            {
                                playerObj.ClearSubscribings();
                            }
                        }
                    }
                    _stalePlayerObjectIds.Clear();
                    foreach (uint playerObjectId in _playerSubscribings.Keys)
                    {
                        if (!_activePlayerObjectIds.Contains(playerObjectId))
                            _stalePlayerObjectIds.Add(playerObjectId);
                    }
                    foreach (uint playerObjectId in _stalePlayerObjectIds)
                        _playerSubscribings.Remove(playerObjectId);
                }
            }

            Manager.Assets.ApplyOwnerChanges();
        }
    }
}
