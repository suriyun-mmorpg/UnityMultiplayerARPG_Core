using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterController
    {
        public const int RAYCAST_COLLIDER_SIZE = 32;
        public const int OVERLAP_COLLIDER_SIZE = 32;
        protected RaycastHit[] raycasts = new RaycastHit[RAYCAST_COLLIDER_SIZE];
        protected Collider[] overlapColliders = new Collider[OVERLAP_COLLIDER_SIZE];
        protected RaycastHit2D[] raycasts2D = new RaycastHit2D[RAYCAST_COLLIDER_SIZE];
        protected Collider2D[] overlapColliders2D = new Collider2D[OVERLAP_COLLIDER_SIZE];

        public int FindClickObjects(out Vector3 worldPointFor2D)
        {
            worldPointFor2D = Vector3.zero;
            if (gameInstance.DimensionType == DimensionType.Dimension3D)
                return Physics.RaycastNonAlloc(Camera.main.ScreenPointToRay(Input.mousePosition), raycasts, 100f, gameInstance.GetTargetLayerMask());
            worldPointFor2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return Physics2D.LinecastNonAlloc(worldPointFor2D, worldPointFor2D, raycasts2D, gameInstance.GetTargetLayerMask());
        }

        public void FindAndSetBuildingAreaFromMousePosition()
        {
            int tempCount = 0;
            Vector3 tempVector3;
            switch (gameInstance.DimensionType)
            {
                case DimensionType.Dimension3D:
                    tempCount = Physics.RaycastNonAlloc(Camera.main.ScreenPointToRay(Input.mousePosition), raycasts, 100f, gameInstance.GetBuildLayerMask());
                    break;
                case DimensionType.Dimension2D:
                    tempVector3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    tempCount = Physics2D.LinecastNonAlloc(tempVector3, tempVector3, raycasts2D, gameInstance.GetBuildLayerMask());
                    break;
            }
            LoopSetBuildingArea(tempCount);
        }

        public void FindAndSetBuildingAreaFromCharacterDirection()
        {
            if (CurrentBuildingEntity == null)
                return;
            int tempCount = 0;
            Vector3 tempVector3;
            switch (gameInstance.DimensionType)
            {
                case DimensionType.Dimension3D:
                    tempVector3 = MovementTransform.position + (MovementTransform.forward * CurrentBuildingEntity.characterForwardDistance);
                    CurrentBuildingEntity.CacheTransform.eulerAngles = GetBuildingPlaceEulerAngles(MovementTransform.eulerAngles);
                    CurrentBuildingEntity.buildingArea = null;
                    tempCount = Physics.RaycastNonAlloc(new Ray(tempVector3 + (Vector3.up * 2.5f), Vector3.down), raycasts, 5f, gameInstance.GetBuildLayerMask());
                    if (!LoopSetBuildingArea(tempCount))
                        CurrentBuildingEntity.CacheTransform.position = GetBuildingPlacePosition(tempVector3);
                    break;
                case DimensionType.Dimension2D:
                    tempVector3 = MovementTransform.position;
                    if (PlayerCharacterEntity.CurrentDirectionType.HasFlag(DirectionType2D.Down))
                        tempVector3 += Vector3.down * CurrentBuildingEntity.characterForwardDistance;
                    if (PlayerCharacterEntity.CurrentDirectionType.HasFlag(DirectionType2D.Up))
                        tempVector3 += Vector3.up * CurrentBuildingEntity.characterForwardDistance;
                    if (PlayerCharacterEntity.CurrentDirectionType.HasFlag(DirectionType2D.Left))
                        tempVector3 += Vector3.left * CurrentBuildingEntity.characterForwardDistance;
                    if (PlayerCharacterEntity.CurrentDirectionType.HasFlag(DirectionType2D.Right))
                        tempVector3 += Vector3.right * CurrentBuildingEntity.characterForwardDistance;
                    CurrentBuildingEntity.buildingArea = null;
                    tempCount = Physics2D.LinecastNonAlloc(tempVector3, tempVector3, raycasts2D, gameInstance.GetBuildLayerMask());
                    if (!LoopSetBuildingArea(tempCount))
                        CurrentBuildingEntity.CacheTransform.position = GetBuildingPlacePosition(tempVector3);
                    break;
            }
        }

        private bool LoopSetBuildingArea(int count)
        {
            BuildingArea nonSnapBuildingArea = null;
            Transform tempTransform;
            Vector3 tempVector3;
            for (int tempCounter = 0; tempCounter < count; ++tempCounter)
            {
                tempTransform = GetRaycastTransform(tempCounter);
                tempVector3 = GetRaycastPoint(tempCounter);
                if (Vector3.Distance(tempVector3, MovementTransform.position) > gameInstance.buildDistance)
                    return false;

                BuildingArea buildingArea = tempTransform.GetComponent<BuildingArea>();
                if (buildingArea == null ||
                    (buildingArea.buildingEntity != null && buildingArea.buildingEntity == CurrentBuildingEntity) ||
                    !CurrentBuildingEntity.buildingTypes.Contains(buildingArea.buildingType))
                    continue;

                CurrentBuildingEntity.CacheTransform.position = GetBuildingPlacePosition(tempVector3);
                CurrentBuildingEntity.buildingArea = buildingArea;
                if (buildingArea.snapBuildingObject)
                    return true;
                nonSnapBuildingArea = buildingArea;
            }
            if (nonSnapBuildingArea != null)
                return true;
            return false;
        }

        public Transform GetRaycastTransform(int index)
        {
            if (gameInstance.DimensionType == DimensionType.Dimension3D)
                return raycasts[index].transform;
            return raycasts2D[index].transform;
        }

        public bool GetRaycastIsTrigger(int index)
        {
            if (gameInstance.DimensionType == DimensionType.Dimension3D)
                return raycasts[index].collider.isTrigger;
            return raycasts2D[index].collider.isTrigger;
        }

        public Vector3 GetRaycastPoint(int index)
        {
            if (gameInstance.DimensionType == DimensionType.Dimension3D)
                return raycasts[index].point;
            return raycasts2D[index].point;
        }

        public int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            if (gameInstance.DimensionType == DimensionType.Dimension3D)
                return Physics.OverlapSphereNonAlloc(position, distance, overlapColliders, layerMask);
            return Physics2D.OverlapCircleNonAlloc(position, distance, overlapColliders2D, layerMask);
        }

        public GameObject GetOverlapObject(int index)
        {
            if (gameInstance.DimensionType == DimensionType.Dimension3D)
                return overlapColliders[index].gameObject;
            return overlapColliders2D[index].gameObject;
        }

        public bool FindTarget(GameObject target, float actDistance, int layerMask)
        {
            Collider tempCollider = target.GetComponent<Collider>();
            if (tempCollider != null)
            {
                Ray ray = new Ray(MovementTransform.position, (tempCollider.bounds.center - MovementTransform.position).normalized);
                float intersectDist;
                return tempCollider.bounds.IntersectRay(ray, out intersectDist) && intersectDist < actDistance;
            }

            Collider2D tempCollider2D = target.GetComponent<Collider2D>();
            if (tempCollider2D != null)
            {
                Ray ray = new Ray(MovementTransform.position, (tempCollider2D.bounds.center - MovementTransform.position).normalized);
                float intersectDist;
                return tempCollider2D.bounds.IntersectRay(ray, out intersectDist) && intersectDist < actDistance;
            }

            return false;
        }

    }
}
