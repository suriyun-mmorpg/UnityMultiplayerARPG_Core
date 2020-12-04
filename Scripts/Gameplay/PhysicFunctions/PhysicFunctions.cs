using System.Collections;
using System.Collections.Generic;
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
            return PhysicUtils.SortedRaycastNonAlloc3D(position + (Vector3.up * distance * 0.5f), Vector3.down, raycasts, distance, layerMask, queryTriggerInteraction);
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

        public float GetRaycastDistance(int index)
        {
            return raycasts[index].distance;
        }

        public Transform GetRaycastTransform(int index)
        {
            return raycasts[index].transform;
        }

        public Transform GetRaycastColliderTransform(int index)
        {
            return raycasts[index].collider.transform;
        }

        public GameObject GetRaycastObject(int index)
        {
            return raycasts[index].transform.gameObject;
        }

        public GameObject GetRaycastColliderGameObject(int index)
        {
            return raycasts[index].collider.gameObject;
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
    }
}
