using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        #region Activity validation functions
        public virtual bool CanWarpCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !IsServer || playerCharacterEntity.IsWarping)
                return false;
            return true;
        }
        #endregion

        /// <summary>
        /// Get current map Id for saving purpose
        /// </summary>
        /// <param name="playerCharacterEntity"></param>
        /// <returns></returns>
        public virtual string GetCurrentMapId(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (CurrentGameInstance.currentPositionSaveMode == CurrentPositionSaveMode.UseRespawnPosition ||
                !CurrentMapInfo.SaveCurrentMapPosition)
                return playerCharacterEntity.RespawnMapName;
            return CurrentMapInfo.Id;
        }

        /// <summary>
        /// Get current position for saving purpose
        /// </summary>
        /// <param name="playerCharacterEntity"></param>
        /// <returns></returns>
        public virtual Vector3 GetCurrentPosition(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (CurrentGameInstance.currentPositionSaveMode == CurrentPositionSaveMode.UseRespawnPosition || !CurrentMapInfo.SaveCurrentMapPosition)
                return playerCharacterEntity.RespawnPosition;
            Vector3 currentPosition = playerCharacterEntity.EntityTransform.position;
            if (!playerCharacterEntity.PassengingVehicleEntity.IsNull())
                currentPosition.y = playerCharacterEntity.PassengingVehicleEntity.Entity.EntityTransform.position.y;
            return currentPosition;
        }

        public virtual void SetCurrentPosition(BasePlayerCharacterEntity playerCharacterEntity, Vector3 position)
        {
            playerCharacterEntity.Teleport(position, Quaternion.LookRotation(-playerCharacterEntity.MovementTransform.forward));
        }

        public void WarpCharacter(WarpPortalType warpPortalType, BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
            switch (warpPortalType)
            {
                case WarpPortalType.Default:
                    WarpCharacter(playerCharacterEntity, mapName, position, overrideRotation, rotation);
                    break;
                case WarpPortalType.EnterInstance:
                    WarpCharacterToInstance(playerCharacterEntity, mapName, position, overrideRotation, rotation);
                    break;
            }
        }

        /// <summary>
        /// Warp character to other map if `mapName` is not empty
        /// </summary>
        /// <param name="playerCharacterEntity"></param>
        /// <param name="mapName"></param>
        /// <param name="position"></param>
        /// <param name="overrideRotation"></param>
        /// <param name="rotation"></param>
        public abstract void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation);

        /// <summary>
        /// Warp character to instance map
        /// </summary>
        /// <param name="playerCharacterEntity"></param>
        /// <param name="mapName"></param>
        /// <param name="position"></param>
        /// <param name="overrideRotation"></param>
        /// <param name="rotation"></param>
        public abstract void WarpCharacterToInstance(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation);

        /// <summary>
        /// Check if this game network manager is for instance map or not
        /// </summary>
        /// <returns></returns>
        protected abstract bool IsInstanceMap();
    }
}
