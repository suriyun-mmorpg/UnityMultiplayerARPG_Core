using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class PhysicFunctions2D : IPhysicFunctions
    {
        private readonly RaycastHit2D[] raycasts2D;
        private readonly Collider2D[] overlapColliders2D;

        public PhysicFunctions2D(int allocSize)
        {
            raycasts2D = new RaycastHit2D[allocSize];
            overlapColliders2D = new Collider2D[allocSize];
        }

        public int Raycast(Vector3 start, Vector3 end, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return PhysicUtils.SortedRaycastNonAlloc2D(start, (end - start).normalized, raycasts2D, Vector3.Distance(start, end), layerMask);
        }

        public int Raycast(Vector3 origin, Vector3 direction, float distance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return PhysicUtils.SortedRaycastNonAlloc2D(origin, direction, raycasts2D, distance, layerMask);
        }

        public int RaycastPickObjects(Camera camera, Vector3 mousePosition, int layerMask, float distance, out Vector3 raycastPosition, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            raycastPosition = camera.ScreenToWorldPoint(mousePosition);
            raycastPosition.z = 0;
            return PhysicUtils.SortedLinecastNonAlloc2D(raycastPosition, raycastPosition, raycasts2D, layerMask);
        }

        public int RaycastDown(Vector3 position, int layerMask, float distance = 100f, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return PhysicUtils.SortedLinecastNonAlloc2D(position, position, raycasts2D, layerMask);
        }

        public bool GetRaycastIsTrigger(int index)
        {
            return raycasts2D[index].collider.isTrigger;
        }

        public Vector3 GetRaycastPoint(int index)
        {
            return raycasts2D[index].point;
        }

        public Vector3 GetRaycastNormal(int index)
        {
            return raycasts2D[index].normal;
        }

        public float GetRaycastDistance(int index)
        {
            return raycasts2D[index].distance;
        }

        public Transform GetRaycastTransform(int index)
        {
            return raycasts2D[index].transform;
        }

        public Transform GetRaycastColliderTransform(int index)
        {
            return raycasts2D[index].collider.transform;
        }

        public GameObject GetRaycastObject(int index)
        {
            return raycasts2D[index].transform.gameObject;
        }

        public GameObject GetRaycastColliderObject(int index)
        {
            return raycasts2D[index].collider.gameObject;
        }

        public int OverlapObjects(Vector3 position, float radius, int layerMask, bool sort = false, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return sort ? PhysicUtils.SortedOverlapCircleNonAlloc(position, radius, overlapColliders2D, layerMask) :
                Physics2D.OverlapCircleNonAlloc(position, radius, overlapColliders2D, layerMask);
        }

        public GameObject GetOverlapObject(int index)
        {
            return overlapColliders2D[index].gameObject;
        }
    }
}
