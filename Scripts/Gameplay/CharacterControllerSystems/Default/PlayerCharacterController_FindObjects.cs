using System.Collections;
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
            LoopSetBuildingArea(physicFunctions.RaycastDown(MovementTransform.position + (GameplayUtils.GetDirectionByAxes(CacheGameplayCameraTransform, aimAxes.x, aimAxes.y) * ConstructingBuildingEntity.buildDistance), CurrentGameInstance.GetBuildLayerMask()));
        }

        public void FindAndSetBuildingAreaByMousePosition()
        {
            Vector3 worldPosition2D;
            LoopSetBuildingArea(physicFunctions.RaycastPickObjects(CacheGameplayCamera, InputManager.MousePosition(), CurrentGameInstance.GetBuildLayerMask(), 100f, out worldPosition2D));
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
            for (int tempCounter = 0; tempCounter < count; ++tempCounter)
            {
                tempTransform = physicFunctions.GetRaycastTransform(tempCounter);
                tempVector3 = GameplayUtils.ClampPosition(MovementTransform.position, physicFunctions.GetRaycastPoint(tempCounter), ConstructingBuildingEntity.buildDistance);

                buildingArea = tempTransform.GetComponent<BuildingArea>();
                if (buildingArea == null ||
                    (buildingArea.Entity && buildingArea.GetObjectId() == ConstructingBuildingEntity.ObjectId) ||
                    !ConstructingBuildingEntity.buildingTypes.Contains(buildingArea.buildingType))
                {
                    // Skip because this area is not allowed to build the building that you are going to build
                    continue;
                }

                ConstructingBuildingEntity.BuildingArea = buildingArea;
                ConstructingBuildingEntity.Position = GetBuildingPlacePosition(tempVector3);
                return true;
            }
            return false;
        }
    }
}
