using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterController
    {
        protected RaycastHit[] raycasts = new RaycastHit[512];
        protected Collider[] overlapColliders = new Collider[512];
        protected RaycastHit2D[] raycasts2D = new RaycastHit2D[512];
        protected Collider2D[] overlapColliders2D = new Collider2D[512];

        public int FindClickObjects(out Vector3 worldPointFor2D)
        {
            worldPointFor2D = Vector3.zero;
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                return PhysicUtils.SortedRaycastNonAlloc3D(CacheGameplayCamera.ScreenPointToRay(InputManager.MousePosition()), raycasts, 100f, CurrentGameInstance.GetTargetLayerMask());
            worldPointFor2D = CacheGameplayCamera.ScreenToWorldPoint(InputManager.MousePosition());
            return PhysicUtils.SortedLinecastNonAlloc2D(worldPointFor2D, worldPointFor2D, raycasts2D, CurrentGameInstance.GetTargetLayerMask());
        }

        public void FindAndSetBuildingAreaFromMousePosition()
        {
            int tempCount = 0;
            Vector3 tempVector3;
            switch (CurrentGameInstance.DimensionType)
            {
                case DimensionType.Dimension3D:
                    tempCount = PhysicUtils.SortedRaycastNonAlloc3D(CacheGameplayCamera.ScreenPointToRay(InputManager.MousePosition()), raycasts, 100f, CurrentGameInstance.GetBuildLayerMask());
                    break;
                case DimensionType.Dimension2D:
                    tempVector3 = CacheGameplayCamera.ScreenToWorldPoint(InputManager.MousePosition());
                    tempCount = PhysicUtils.SortedLinecastNonAlloc2D(tempVector3, tempVector3, raycasts2D, CurrentGameInstance.GetBuildLayerMask());
                    break;
            }
            LoopSetBuildingArea(tempCount);
        }

        public void FindAndSetBuildingAreaFromCharacterDirection()
        {
            if (ConstructingBuildingEntity == null)
                return;
            int tempCount = 0;
            Vector3 tempVector3;
            switch (CurrentGameInstance.DimensionType)
            {
                case DimensionType.Dimension3D:
                    tempVector3 = MovementTransform.position + (MovementTransform.forward * ConstructingBuildingEntity.characterForwardDistance);
                    ConstructingBuildingEntity.CacheTransform.eulerAngles = GetBuildingPlaceEulerAngles(MovementTransform.eulerAngles);
                    ConstructingBuildingEntity.BuildingArea = null;
                    tempCount = PhysicUtils.SortedRaycastNonAlloc3D(tempVector3 + (Vector3.up * 2.5f), Vector3.down, raycasts, 100f, CurrentGameInstance.GetBuildLayerMask());
                    if (!LoopSetBuildingArea(tempCount))
                        ConstructingBuildingEntity.CacheTransform.position = GetBuildingPlacePosition(tempVector3);
                    break;
                case DimensionType.Dimension2D:
                    tempVector3 = MovementTransform.position;
                    DirectionType2D directionType2D = GameplayUtils.GetDirectionTypeByVector2(PlayerCharacterEntity.Direction2D);
                    if (directionType2D.HasFlag(DirectionType2D.Down))
                        tempVector3 += Vector3.down * ConstructingBuildingEntity.characterForwardDistance;
                    if (directionType2D.HasFlag(DirectionType2D.Up))
                        tempVector3 += Vector3.up * ConstructingBuildingEntity.characterForwardDistance;
                    if (directionType2D.HasFlag(DirectionType2D.Left))
                        tempVector3 += Vector3.left * ConstructingBuildingEntity.characterForwardDistance;
                    if (directionType2D.HasFlag(DirectionType2D.Right))
                        tempVector3 += Vector3.right * ConstructingBuildingEntity.characterForwardDistance;
                    ConstructingBuildingEntity.BuildingArea = null;
                    tempCount = PhysicUtils.SortedLinecastNonAlloc2D(tempVector3, tempVector3, raycasts2D, CurrentGameInstance.GetBuildLayerMask());
                    if (!LoopSetBuildingArea(tempCount))
                        ConstructingBuildingEntity.CacheTransform.position = GetBuildingPlacePosition(tempVector3);
                    break;
            }
        }

        /// <summary>
        /// Return true if found building area
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private bool LoopSetBuildingArea(int count)
        {
            BuildingArea buildingArea;
            Transform tempTransform;
            Vector3 tempVector3;
            Vector3 tempOffset;
            for (int tempCounter = 0; tempCounter < count; ++tempCounter)
            {
                tempTransform = GetRaycastTransform(tempCounter);
                tempVector3 = GetRaycastPoint(tempCounter);
                tempOffset = tempVector3 - MovementTransform.position;
                tempVector3 = MovementTransform.position + Vector3.ClampMagnitude(tempOffset, CurrentGameInstance.buildDistance);

                buildingArea = tempTransform.GetComponent<BuildingArea>();
                if (buildingArea == null ||
                    (buildingArea.Entity && buildingArea.GetObjectId() == ConstructingBuildingEntity.ObjectId) ||
                    !ConstructingBuildingEntity.buildingTypes.Contains(buildingArea.buildingType))
                {
                    // Skip because this area is not allowed to build the building that you are going to build
                    continue;
                }

                ConstructingBuildingEntity.BuildingArea = buildingArea;
                ConstructingBuildingEntity.CacheTransform.position = GetBuildingPlacePosition(tempVector3);
                return true;
            }
            return false;
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

        public int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                return Physics.OverlapSphereNonAlloc(position, distance, overlapColliders, layerMask);
            return Physics2D.OverlapCircleNonAlloc(position, distance, overlapColliders2D, layerMask);
        }

        public GameObject GetOverlapObject(int index)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                return overlapColliders[index].gameObject;
            return overlapColliders2D[index].gameObject;
        }

        public bool IsTargetInAttackDistance(IDamageableEntity target, float attackDistance, int layerMask)
        {
            Transform damageTransform = PlayerCharacterEntity.GetDamageTransform(isLeftHandAttacking);
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
            {
                IDamageableEntity damageableEntity;
                int tempCount = PhysicUtils.SortedOverlapSphereNonAlloc(damageTransform.position, attackDistance, overlapColliders, layerMask);
                for (int i = 0; i < tempCount; ++i)
                {
                    if (GetOverlapObject(i) == target.GetGameObject())
                        return true;
                    damageableEntity = GetOverlapObject(i).GetComponent<IDamageableEntity>();
                    if (damageableEntity != null && damageableEntity.GetObjectId() == target.GetObjectId())
                        return true;
                }
            }
            else
            {
                IDamageableEntity damageableEntity;
                int tempCount = PhysicUtils.SortedOverlapCircleNonAlloc(damageTransform.position, attackDistance, overlapColliders2D, layerMask);
                for (int i = 0; i < tempCount; ++i)
                {
                    if (GetOverlapObject(i) == target.GetTransform())
                        return true;
                    damageableEntity = GetOverlapObject(i).GetComponent<IDamageableEntity>();
                    if (damageableEntity != null && damageableEntity.GetObjectId() == target.GetObjectId())
                        return true;
                }
            }
            return false;
        }
    }
}
