using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MultiplayerARPG
{
    public class PhysicFunctions : IPhysicFunctions
    {
        private NativeArray<RaycastHit> _raycastResults;
        private NativeArray<ColliderHit> _overlapResults;
        private int _allocSize;

        public PhysicFunctions(int allocSize)
        {
            _allocSize = allocSize;
            _raycastResults = new NativeArray<RaycastHit>(allocSize, Allocator.Persistent);
            _overlapResults = new NativeArray<ColliderHit>(allocSize, Allocator.Persistent);
        }

        public void Clean()
        {
            if (_raycastResults.IsCreated) _raycastResults.Dispose();
            if (_overlapResults.IsCreated) _overlapResults.Dispose();
        }

        ~PhysicFunctions()
        {
            Clean();
        }

        public bool SingleRaycast(Vector3 start, Vector3 end, out PhysicRaycastResult result, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return SingleRaycast(start, (end - start).normalized, out result, Vector3.Distance(start, end), layerMask, queryTriggerInteraction);
        }

        public bool SingleRaycast(Vector3 origin, Vector3 direction, out PhysicRaycastResult result, float distance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            result = new PhysicRaycastResult();
            NativeArray<RaycastCommand> tempCommands = new NativeArray<RaycastCommand>(1, Allocator.TempJob);
            QueryParameters queryParameters = new QueryParameters(layerMask, false, queryTriggerInteraction, false);
            tempCommands[0] = new RaycastCommand(origin, direction, queryParameters, distance);
            JobHandle handle = RaycastCommand.ScheduleBatch(tempCommands, _raycastResults, 1, 1);
            handle.Complete();
            tempCommands.Dispose();
            if (_raycastResults[0].collider != null)
            {
                result.point = _raycastResults[0].point;
                result.normal = _raycastResults[0].normal;
                result.distance = _raycastResults[0].distance;
                result.transform = _raycastResults[0].transform;
                return true;
            }
            return false;
        }

        public int Raycast(Vector3 start, Vector3 end, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return Raycast(start, (end - start).normalized, Vector3.Distance(start, end), layerMask, queryTriggerInteraction);
        }

        public int Raycast(Vector3 origin, Vector3 direction, float distance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            NativeArray<RaycastCommand> tempCommands = new NativeArray<RaycastCommand>(1, Allocator.TempJob);
            QueryParameters queryParameters = new QueryParameters(layerMask, false, queryTriggerInteraction, false);
            tempCommands[0] = new RaycastCommand(origin, direction, queryParameters, distance);
            JobHandle handle = RaycastCommand.ScheduleBatch(tempCommands, _raycastResults, 1, _allocSize);
            handle.Complete();
            tempCommands.Dispose();
            for (int i = 0; i < _raycastResults.Length; ++i)
            {
                if (_raycastResults[i].collider == null)
                {
                    return i;
                }
            }
            return _raycastResults.Length;
        }

        public int RaycastPickObjects(Camera camera, Vector3 mousePosition, int layerMask, float distance, out Vector3 raycastPosition, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Ray ray = camera.ScreenPointToRay(mousePosition);
            raycastPosition = ray.origin;
            return Raycast(ray.origin, ray.direction, distance, layerMask, queryTriggerInteraction);
        }

        public int RaycastDown(Vector3 position, int layerMask, float distance = 100, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            // Raycast to find hit floor
            int hitCount = Raycast(position + (Vector3.up * distance * 0.5f), Vector3.down * distance, layerMask, queryTriggerInteraction);
            //System.Array.Sort(_raycasts, 0, hitCount, new PhysicUtils.RaycastHitComparerCustomOrigin(position));
            return hitCount;
        }

        public int OverlapObjects(Vector3 position, float radius, int layerMask, bool sort = false, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            NativeArray<OverlapSphereCommand> tempCommands = new NativeArray<OverlapSphereCommand>(1, Allocator.TempJob);
            QueryParameters queryParameters = new QueryParameters(layerMask, false, queryTriggerInteraction, false);
            tempCommands[0] = new OverlapSphereCommand(position, radius, queryParameters);
            JobHandle handle = OverlapSphereCommand.ScheduleBatch(tempCommands, _overlapResults, 1, _allocSize);
            handle.Complete();
            tempCommands.Dispose();
            for (int i = 0; i < _overlapResults.Length; ++i)
            {
                if (_overlapResults[i].collider == null)
                {
                    return i;
                }
            }
            return _overlapResults.Length;
        }

        public bool GetRaycastIsTrigger(int index) => _raycastResults[index].collider.isTrigger;
        public Vector3 GetRaycastPoint(int index) => _raycastResults[index].point;
        public Vector3 GetRaycastNormal(int index) => _raycastResults[index].normal;
        public Bounds GetRaycastColliderBounds(int index) => _raycastResults[index].collider.bounds;
        public float GetRaycastDistance(int index) => _raycastResults[index].distance;
        public Transform GetRaycastTransform(int index) => _raycastResults[index].transform;
        public GameObject GetRaycastObject(int index) => _raycastResults[index].transform.gameObject;
        public Vector3 GetRaycastColliderClosestPoint(int index, Vector3 position) => _raycastResults[index].collider.ClosestPoint(position);

        public GameObject GetOverlapObject(int index) => _overlapResults[index].collider.gameObject;
        public Vector3 GetOverlapColliderClosestPoint(int index, Vector3 position) => _overlapResults[index].collider.ClosestPoint(position);
        public bool GetOverlapColliderRaycast(int index, Vector3 origin, Vector3 direction, out Vector3 point, out Vector3 normal, out float distance, out Transform transform, float maxDistance)
        {
            if (_overlapResults[index].collider.Raycast(new Ray(origin, direction), out RaycastHit hitInfo, maxDistance))
            {
                point = hitInfo.point;
                normal = hitInfo.normal;
                distance = hitInfo.distance;
                transform = hitInfo.transform;
                return true;
            }
            point = origin + direction * maxDistance;
            normal = -direction;
            distance = maxDistance;
            transform = null;
            return false;
        }
    }
}
