using UnityEngine;

namespace MultiplayerARPG
{
    public class PhysicFunctions2D : IPhysicFunctions
    {
        private readonly RaycastHit2D[] _raycasts2D;
        private readonly Collider2D[] _overlapColliders2D;

        public PhysicFunctions2D(int allocSize)
        {
            _raycasts2D = new RaycastHit2D[allocSize];
            _overlapColliders2D = new Collider2D[allocSize];
        }

        public bool SingleRaycast(Vector3 start, Vector3 end, out PhysicRaycastResult result, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            result = new PhysicRaycastResult();
            RaycastHit2D hit = Physics2D.Raycast(start, (end - start).normalized, Vector3.Distance(start, end), layerMask);
            if (hit.collider != null)
            {
                result.point = hit.point;
                result.normal = hit.normal;
                result.distance = hit.distance;
                result.transform = hit.transform;
                return true;
            }
            return false;
        }

        public bool SingleRaycast(Vector3 origin, Vector3 direction, out PhysicRaycastResult result, float distance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            result = new PhysicRaycastResult();
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, layerMask);
            if (hit.collider != null)
            {
                result.point = hit.point;
                result.normal = hit.normal;
                result.distance = hit.distance;
                result.transform = hit.transform;
                return true;
            }
            return false;
        }

        public int Raycast(Vector3 start, Vector3 end, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return PhysicUtils.SortedRaycastNonAlloc2D(start, (end - start).normalized, _raycasts2D, Vector3.Distance(start, end), layerMask);
        }

        public int Raycast(Vector3 origin, Vector3 direction, float distance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return PhysicUtils.SortedRaycastNonAlloc2D(origin, direction, _raycasts2D, distance, layerMask);
        }

        public int RaycastPickObjects(Camera camera, Vector3 mousePosition, int layerMask, float distance, out Vector3 raycastPosition, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            raycastPosition = camera.ScreenToWorldPoint(mousePosition);
            raycastPosition.z = 0;
            return PhysicUtils.SortedLinecastNonAlloc2D(raycastPosition, raycastPosition, _raycasts2D, layerMask);
        }

        public int RaycastDown(Vector3 position, int layerMask, float distance = 100f, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return PhysicUtils.SortedLinecastNonAlloc2D(position, position, _raycasts2D, layerMask);
        }

        public bool GetRaycastIsTrigger(int index)
        {
            return _raycasts2D[index].collider.isTrigger;
        }

        public Vector3 GetRaycastPoint(int index)
        {
            return _raycasts2D[index].point;
        }

        public Vector3 GetRaycastNormal(int index)
        {
            return _raycasts2D[index].normal;
        }

        public Bounds GetRaycastColliderBounds(int index)
        {
            return _raycasts2D[index].collider.bounds;
        }

        public float GetRaycastDistance(int index)
        {
            return _raycasts2D[index].distance;
        }

        public Transform GetRaycastTransform(int index)
        {
            return _raycasts2D[index].transform;
        }

        public GameObject GetRaycastObject(int index)
        {
            return _raycasts2D[index].transform.gameObject;
        }

        public Vector3 GetRaycastColliderClosestPoint(int index, Vector3 position)
        {
            return _raycasts2D[index].collider.ClosestPoint(position);
        }

        public int OverlapObjects(Vector3 position, float radius, int layerMask, bool sort = false, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return sort ? PhysicUtils.SortedOverlapCircleNonAlloc(position, radius, _overlapColliders2D, layerMask) :
                Physics2D.OverlapCircleNonAlloc(position, radius, _overlapColliders2D, layerMask);
        }

        public GameObject GetOverlapObject(int index)
        {
            return _overlapColliders2D[index].gameObject;
        }

        public Vector3 GetOverlapColliderClosestPoint(int index, Vector3 position)
        {
            return _overlapColliders2D[index].ClosestPoint(position);
        }

        public bool GetOverlapColliderRaycast(int index, Vector3 origin, Vector3 direction, out Vector3 point, out Vector3 normal, out float distance, out Transform transform, float maxDistance)
        {
            RaycastHit2D[] hits = new RaycastHit2D[1];
            int hitCount = _overlapColliders2D[index].Raycast(direction, hits, maxDistance);
            if (hitCount > 0)
            {
                point = hits[0].point;
                normal = hits[0].normal;
                distance = hits[0].distance;
                transform = hits[0].transform;
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
