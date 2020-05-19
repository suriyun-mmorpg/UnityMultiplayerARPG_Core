using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class PhysicFunctions
    {
        private readonly RaycastHit[] raycasts;
        private readonly Collider[] overlapColliders;
        private readonly RaycastHit2D[] raycasts2D;
        private readonly Collider2D[] overlapColliders2D;
        private GameInstance CurrentGameInstance { get { return GameInstance.Singleton; } }
        public RaycastHit[] Raycasts { get { return raycasts; } }
        public Collider[] OverlapColliders { get { return overlapColliders; } }
        public RaycastHit2D[] Raycasts2D { get { return raycasts2D; } }
        public Collider2D[] OverlapColliders2D { get { return overlapColliders2D; } }


        public PhysicFunctions(int allocSize)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
            {
                raycasts = new RaycastHit[allocSize];
                overlapColliders = new Collider[allocSize];
            }
            else
            {
                raycasts2D = new RaycastHit2D[allocSize];
                overlapColliders2D = new Collider2D[allocSize];
            }
        }

        public int RaycastPickObjects(Camera camera, Vector3 mousePosition, int layerMask, float distance, out Vector3 worldPosition2D)
        {
            worldPosition2D = Vector3.zero;
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                return PhysicUtils.SortedRaycastNonAlloc3D(camera.ScreenPointToRay(mousePosition), raycasts, distance, layerMask);
            worldPosition2D = camera.ScreenToWorldPoint(mousePosition);
            return PhysicUtils.SortedLinecastNonAlloc2D(worldPosition2D, worldPosition2D, raycasts2D, layerMask);
        }

        public int RaycastDown(Vector3 position, int layerMask, float distance = 100f)
        {
            return CurrentGameInstance.DimensionType == DimensionType.Dimension3D ?
                 PhysicUtils.SortedRaycastNonAlloc3D(position + (Vector3.up * distance * 0.5f), Vector3.down, raycasts, distance, layerMask) :
                 PhysicUtils.SortedLinecastNonAlloc2D(position, position, raycasts2D, layerMask);
        }

        public Transform GetRaycastTransform(int index)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                return raycasts[index].transform;
            return raycasts2D[index].transform;
        }

        public bool GetRaycastIsTrigger(int index)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                return raycasts[index].collider.isTrigger;
            return raycasts2D[index].collider.isTrigger;
        }

        public Vector3 GetRaycastPoint(int index)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                return raycasts[index].point;
            return raycasts2D[index].point;
        }

        public int OverlapObjects(Vector3 position, float distance, int layerMask, bool sort = false)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                return sort ? PhysicUtils.SortedOverlapSphereNonAlloc(position, distance, overlapColliders, layerMask) :
                    Physics.OverlapSphereNonAlloc(position, distance, overlapColliders, layerMask);
            return sort ? PhysicUtils.SortedOverlapCircleNonAlloc(position, distance, overlapColliders2D, layerMask) :
                Physics2D.OverlapCircleNonAlloc(position, distance, overlapColliders2D, layerMask);
        }

        public GameObject GetOverlapObject(int index)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                return overlapColliders[index].gameObject;
            return overlapColliders2D[index].gameObject;
        }
    }
}
