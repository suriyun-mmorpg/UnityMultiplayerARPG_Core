using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterController
    {
        public int FindClickObjects(out Vector3 worldPosition2D)
        {
            return physicFunctions.RaycastPickObjects(CacheGameplayCamera, InputManager.MousePosition(), CurrentGameInstance.GetTargetLayerMask(), 100f, out worldPosition2D);
        }

        public void FindAndSetBuildingAreaByAxes(Vector2 aimAxes)
        {
            Vector3 raycastPosition = CacheTransform.position + (GameplayUtils.GetDirectionByAxes(CacheGameplayCameraTransform, aimAxes.x, aimAxes.y) * ConstructingBuildingEntity.BuildDistance);
            LoopSetBuildingArea(physicFunctions.RaycastDown(raycastPosition, CurrentGameInstance.GetBuildLayerMask()));
        }

        public void FindAndSetBuildingAreaByMousePosition()
        {
            LoopSetBuildingArea(physicFunctions.RaycastPickObjects(CacheGameplayCamera, InputManager.MousePosition(), CurrentGameInstance.GetBuildLayerMask(), Vector3.Distance(CacheGameplayCameraTransform.position, MovementTransform.position) + ConstructingBuildingEntity.BuildDistance, out _));
        }

        /// <summary>
        /// Return true if found building area
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private bool LoopSetBuildingArea(int count)
        {
            ConstructingBuildingEntity.BuildingArea = null;
            ConstructingBuildingEntity.HitSurface = false;
            BuildingEntity buildingEntity;
            BuildingArea buildingArea;
            Transform tempTransform;
            Bounds tempColliderBounds;
            Vector3 tempVector3;
            for (int tempCounter = 0; tempCounter < count; ++tempCounter)
            {
                tempTransform = physicFunctions.GetRaycastTransform(tempCounter);
                if (ConstructingBuildingEntity.CacheTransform.root == tempTransform.root)
                {
                    // Hit collider which is part of constructing building entity, skip it
                    continue;
                }

                tempVector3 = physicFunctions.GetRaycastPoint(tempCounter);
                tempVector3 = GetBuildingPlacePosition(tempVector3);
                tempColliderBounds = physicFunctions.GetRaycastColliderBounds(tempCounter);

                if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                {
                    // Find ground position from upper position
                    bool hitAimmingObject = false;
                    Vector3 raycastOrigin = new Vector3(tempVector3.x, tempColliderBounds.center.y + tempColliderBounds.extents.y + 0.01f, tempVector3.z);
                    RaycastHit[] groundHits = Physics.RaycastAll(raycastOrigin, Vector3.down, tempColliderBounds.size.y + 0.01f, CurrentGameInstance.GetBuildLayerMask());
                    for (int j = 0; j < groundHits.Length; ++j)
                    {
                        if (groundHits[j].transform == tempTransform)
                        {
                            tempVector3 = groundHits[j].point;
                            ConstructingBuildingEntity.Position = tempVector3;
                            hitAimmingObject = true;
                            break;
                        }
                    }
                    if (!hitAimmingObject)
                        continue;
                }

                buildingEntity = tempTransform.root.GetComponent<BuildingEntity>();
                buildingArea = tempTransform.GetComponent<BuildingArea>();
                if ((buildingArea == null || !ConstructingBuildingEntity.BuildingTypes.Contains(buildingArea.buildingType))
                    && buildingEntity == null)
                {
                    // Hit surface which is not building area or building entity
                    ConstructingBuildingEntity.BuildingArea = null;
                    ConstructingBuildingEntity.HitSurface = true;
                    break;
                }

                if (buildingArea == null || !ConstructingBuildingEntity.BuildingTypes.Contains(buildingArea.buildingType))
                {
                    // Skip because this area is not allowed to build the building that you are going to build
                    continue;
                }

                ConstructingBuildingEntity.BuildingArea = buildingArea;
                ConstructingBuildingEntity.HitSurface = true;
                return true;
            }
            return false;
        }
    }
}
