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
            LoopSetBuildingArea(physicFunctions.RaycastDown(raycastPosition, CurrentGameInstance.GetBuildLayerMask()), raycastPosition);
        }

        public void FindAndSetBuildingAreaByMousePosition()
        {
            Vector3 worldPosition2D;
            LoopSetBuildingArea(physicFunctions.RaycastPickObjects(CacheGameplayCamera, InputManager.MousePosition(), CurrentGameInstance.GetBuildLayerMask(), 100f, out worldPosition2D), worldPosition2D);
        }

        /// <summary>
        /// Return true if found building area
        /// </summary>
        /// <param name="count"></param>
        /// <param name="raycastPosition"></param>
        /// <returns></returns>
        private bool LoopSetBuildingArea(int count, Vector3 raycastPosition)
        {
            IGameEntity gameEntity;
            BuildingArea buildingArea;
            Transform tempTransform;
            Vector3 tempVector3;
            for (int tempCounter = 0; tempCounter < count; ++tempCounter)
            {
                tempTransform = physicFunctions.GetRaycastTransform(tempCounter);
                tempVector3 = GameplayUtils.ClampPosition(CacheTransform.position, physicFunctions.GetRaycastPoint(tempCounter), ConstructingBuildingEntity.BuildDistance);
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                    tempVector3.y = physicFunctions.GetRaycastPoint(tempCounter).y;

                buildingArea = tempTransform.GetComponent<BuildingArea>();
                if (buildingArea == null)
                {
                    gameEntity = tempTransform.GetComponent<IGameEntity>();
                    if (gameEntity == null || gameEntity.Entity != ConstructingBuildingEntity)
                    {
                        // Hit something and it is not part of constructing building entity, assume that it is ground
                        ConstructingBuildingEntity.BuildingArea = null;
                        ConstructingBuildingEntity.Position = GetBuildingPlacePosition(tempVector3);
                        break;
                    }
                    continue;
                }

                if (buildingArea.IsPartOfBuildingEntity(ConstructingBuildingEntity) ||
                    !ConstructingBuildingEntity.BuildingTypes.Contains(buildingArea.buildingType))
                {
                    // Skip because this area is not allowed to build the building that you are going to build
                    continue;
                }

                ConstructingBuildingEntity.BuildingArea = buildingArea;
                ConstructingBuildingEntity.Position = GetBuildingPlacePosition(tempVector3);
                return true;
            }
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
            {
                ConstructingBuildingEntity.BuildingArea = null;
                ConstructingBuildingEntity.Position = GetBuildingPlacePosition(raycastPosition);
            }
            return false;
        }
    }
}
