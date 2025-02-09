using Insthync.SpatialPartitioningSystems;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    public class JobifiedGridSpatialPartitioningAOI : BaseInterestManager
    {
        public bool includeInactiveComponents = true;
        public float cellSize = 64f;
        public int maxObjects = 10000;
        [Tooltip("Update every ? seconds")]
        public float updateInterval = 1.0f;
        public Color boundsGizmosColor = Color.green;

        private JobifiedGridSpatialPartitioningSystem _system;
        private float _updateCountDown;
        private Bounds _bounds;
        private List<SpatialObject> _spatialObjects = new List<SpatialObject>();

        private void OnDrawGizmosSelected()
        {
            Color color = Gizmos.color;
            Gizmos.color = boundsGizmosColor;
            Gizmos.DrawWireCube(_bounds.center, _bounds.size);
            Gizmos.color = color;
        }

        private void OnDestroy()
        {
            _system = null;
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
            if (!IsServer || isAdditive || !isOnline)
            {
                _system = null;
                return;
            }
            PrepareSystem();
        }

        public void PrepareSystem()
        {
            if (!IsServer || !Manager.ServerSceneInfo.HasValue)
            {
                _system = null;
                return;
            }
            _system = null;
            switch (GameInstance.Singleton.DimensionType)
            {
                case DimensionType.Dimension3D:
                    var collider3Ds = GenericUtils.GetComponentsFromAllLoadedScenes<Collider>(true);
                    if (collider3Ds.Count > 0)
                    {
                        Bounds bounds = collider3Ds[0].bounds;
                        for (int i = 1; i < collider3Ds.Count; ++i)
                        {
                            bounds.Encapsulate(collider3Ds[i].bounds);
                        }
                        _bounds = bounds;
                        _system = new JobifiedGridSpatialPartitioningSystem(_bounds, cellSize, maxObjects);
                    }
                    break;
                case DimensionType.Dimension2D:
                    var collider2Ds = GenericUtils.GetComponentsFromAllLoadedScenes<Collider2D>(true);
                    if (collider2Ds.Count > 0)
                    {
                        Bounds bounds = collider2Ds[0].bounds;
                        for (int i = 0; i < collider2Ds.Count; ++i)
                        {
                            bounds.Encapsulate(collider2Ds[i].bounds);
                        }
                        _bounds = bounds;
                        _system = new JobifiedGridSpatialPartitioningSystem(_bounds, cellSize, maxObjects);
                    }
                    break;
            }
        }

        public override void UpdateInterestManagement(float deltaTime)
        {
            _updateCountDown -= deltaTime;
            if (_updateCountDown > 0)
                return;
            _updateCountDown = updateInterval;
            Profiler.BeginSample("JobifiedGridSpatialPartitioningAOI - Update");

            _spatialObjects.Clear();
            foreach (LiteNetLibIdentity spawnedObject in Manager.Assets.GetSpawnedObjects())
            {
                if (spawnedObject == null)
                    continue;
                _spatialObjects.Add(new SpatialObject()
                {
                    objectId = spawnedObject.ObjectId,
                    position = spawnedObject.transform.position,
                    radius = GetVisibleRange(spawnedObject),
                });
            }
            _system.UpdateGrid(_spatialObjects);

            HashSet<uint> subscribings = new HashSet<uint>();
            foreach (LiteNetLibPlayer player in Manager.GetPlayers())
            {
                if (!player.IsReady)
                {
                    // Don't subscribe if player not ready
                    continue;
                }
                foreach (LiteNetLibIdentity playerObject in player.GetSpawnedObjects())
                {
                    // Update subscribing list, it will unsubscribe objects which is not in this list
                    subscribings.Clear();
                    var resultSpatialObjects = _system.QueryRadius(playerObject.transform.position, 0f);
                    LiteNetLibIdentity contactedObject;
                    for (int i = 0; i < resultSpatialObjects.Length; ++i)
                    {
                        uint contactedObjectId = resultSpatialObjects[i].objectId;
                        if (Manager.Assets.TryGetSpawnedObject(contactedObjectId, out contactedObject) &&
                            ShouldSubscribe(playerObject, contactedObject, false))
                            subscribings.Add(contactedObjectId);
                    }
                    resultSpatialObjects.Dispose();
                    playerObject.UpdateSubscribings(subscribings);
                }
            }
            Profiler.EndSample();
        }
    }
}
