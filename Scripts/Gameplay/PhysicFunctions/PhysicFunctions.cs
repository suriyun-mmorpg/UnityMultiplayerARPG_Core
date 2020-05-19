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

        public int Raycast(Vector3 origin, Vector3 direction, float distance, int layerMask)
        {
            return PhysicUtils.SortedRaycastNonAlloc3D(origin, direction, raycasts, distance, layerMask);
        }

        public int RaycastPickObjects(Camera camera, Vector3 mousePosition, int layerMask, float distance, out Vector3 worldPosition2D)
        {
            worldPosition2D = Vector3.zero;
            return PhysicUtils.SortedRaycastNonAlloc3D(camera.ScreenPointToRay(mousePosition), raycasts, distance, layerMask);
        }

        public int RaycastDown(Vector3 position, int layerMask, float distance = 100f)
        {
            return PhysicUtils.SortedRaycastNonAlloc3D(position + (Vector3.up * distance * 0.5f), Vector3.down, raycasts, distance, layerMask);
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

        public GameObject GetRaycastObject(int index)
        {
            return raycasts[index].transform.gameObject;
        }

        public int OverlapObjects(Vector3 position, float distance, int layerMask, bool sort = false)
        {
            return sort ? PhysicUtils.SortedOverlapSphereNonAlloc(position, distance, overlapColliders, layerMask) :
                Physics.OverlapSphereNonAlloc(position, distance, overlapColliders, layerMask);
        }

        public GameObject GetOverlapObject(int index)
        {
            return overlapColliders[index].gameObject;
        }
    }
}
