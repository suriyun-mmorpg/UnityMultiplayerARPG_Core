using UnityEngine;

namespace MultiplayerARPG
{
    public class PhysicFunctions : IPhysicFunctions
    {
        private readonly RaycastHit[] raycasts;
        private readonly Collider[] overlapColliders;

        public PhysicFunctions(int allocSize)
        {
            raycasts = new RaycastHit[allocSize];
            overlapColliders = new Collider[allocSize];
        }

        public bool SingleRaycast(Vector3 start, Vector3 end, out PhysicRaycastResult result, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            result = new PhysicRaycastResult();
            RaycastHit hit;
            if (Physics.Raycast(start, (end - start).normalized, out hit, Vector3.Distance(start, end), layerMask, queryTriggerInteraction))
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
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, distance, layerMask, queryTriggerInteraction))
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
            return PhysicUtils.SortedRaycastNonAlloc3D(start, (end - start).normalized, raycasts, Vector3.Distance(start, end), layerMask, queryTriggerInteraction);
        }

        public int Raycast(Vector3 origin, Vector3 direction, float distance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return PhysicUtils.SortedRaycastNonAlloc3D(origin, direction, raycasts, distance, layerMask, queryTriggerInteraction);
        }

        public int RaycastPickObjects(Camera camera, Vector3 mousePosition, int layerMask, float distance, out Vector3 raycastPosition, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Ray ray = camera.ScreenPointToRay(mousePosition);
            raycastPosition = ray.origin;
            return PhysicUtils.SortedRaycastNonAlloc3D(ray, raycasts, distance, layerMask, queryTriggerInteraction);
        }

        public int RaycastDown(Vector3 position, int layerMask, float distance = 100f, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            // Raycast to find hit floor
            int hitCount = Physics.RaycastNonAlloc(position + (Vector3.up * distance * 0.5f), Vector3.down, raycasts, distance, layerMask, queryTriggerInteraction);
            System.Array.Sort(raycasts, 0, hitCount, new PhysicUtils.RaycastHitComparerCustomOrigin(position));
            return hitCount;
        }

        public bool GetRaycastIsTrigger(int index)
        {
            return raycasts[index].collider.isTrigger;
        }

        public Vector3 GetRaycastPoint(int index)
        {
            return raycasts[index].point;
        }

        public Vector3 GetRaycastNormal(int index)
        {
            return raycasts[index].normal;
        }

        public Bounds GetRaycastColliderBounds(int index)
        {
            return raycasts[index].collider.bounds;
        }

        public float GetRaycastDistance(int index)
        {
            return raycasts[index].distance;
        }

        public Transform GetRaycastTransform(int index)
        {
            return raycasts[index].transform;
        }

        public GameObject GetRaycastObject(int index)
        {
            return raycasts[index].transform.gameObject;
        }

        public Vector3 GetRaycastColliderClosestPoint(int index, Vector3 position)
        {
            return raycasts[index].collider.ClosestPoint(position);
        }

        public int OverlapObjects(Vector3 position, float radius, int layerMask, bool sort = false, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return sort ? PhysicUtils.SortedOverlapSphereNonAlloc(position, radius, overlapColliders, layerMask, queryTriggerInteraction) :
                Physics.OverlapSphereNonAlloc(position, radius, overlapColliders, layerMask, queryTriggerInteraction);
        }

        public GameObject GetOverlapObject(int index)
        {
            return overlapColliders[index].gameObject;
        }

        public Vector3 GetOverlapColliderClosestPoint(int index, Vector3 position)
        {
            return overlapColliders[index].ClosestPoint(position);
        }
    }
}
