using System.Collections.Concurrent;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        public struct EnterGameCharacterLocation
        {
            public string channelId;
            public string mapName;
            public Vector3 position;
            public Vector3 rotation;
            public string safeArea;
        }

        /// <summary>
        /// Args: Character's ID, Location when enter server
        /// </summary>
        protected ConcurrentDictionary<string, EnterGameCharacterLocation> _characterLocationsWhenEnterGame = new ConcurrentDictionary<string, EnterGameCharacterLocation>();

        #region Activity validation functions
        public virtual bool CanWarpCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (!IsServer || playerCharacterEntity == null || playerCharacterEntity.IsWarping)
                return false;
            return true;
        }
        #endregion

        public virtual string GetCurrentChannel(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (IsInstanceMap())
                return _characterLocationsWhenEnterGame[playerCharacterEntity.Id].channelId;
            return ChannelId;
        }

        /// <summary>
        /// Get current map Id for saving purpose
        /// </summary>
        /// <param name="playerCharacterEntity"></param>
        /// <returns></returns>
        public virtual string GetCurrentMapId(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (IsInstanceMap())
                return _characterLocationsWhenEnterGame[playerCharacterEntity.Id].mapName;
#if !DISABLE_DIFFER_MAP_RESPAWNING
            if (CurrentGameInstance.currentPositionSaveMode == CurrentPositionSaveMode.UseRespawnPosition || !CurrentMapInfo.SaveCurrentMapPosition)
                return playerCharacterEntity.RespawnMapName;
#endif
            return CurrentMapInfo.Id;
        }

        /// <summary>
        /// Get current position for saving purpose
        /// </summary>
        /// <param name="playerCharacterEntity"></param>
        /// <returns></returns>
        public virtual Vector3 GetCurrentPosition(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (IsInstanceMap())
                return _characterLocationsWhenEnterGame[playerCharacterEntity.Id].position;
#if !DISABLE_DIFFER_MAP_RESPAWNING
            if (CurrentGameInstance.currentPositionSaveMode == CurrentPositionSaveMode.UseRespawnPosition || !CurrentMapInfo.SaveCurrentMapPosition)
                return playerCharacterEntity.RespawnPosition;
#endif
            Vector3 currentPosition = playerCharacterEntity.EntityTransform.position;
            if (!playerCharacterEntity.PassengingVehicleEntity.IsNull())
                currentPosition.y = playerCharacterEntity.PassengingVehicleEntity.Entity.EntityTransform.position.y;
            return currentPosition;
        }

        public virtual void SetCurrentPosition(BasePlayerCharacterEntity playerCharacterEntity, Vector3 position)
        {
            playerCharacterEntity.Teleport(position, Quaternion.LookRotation(-playerCharacterEntity.MovementTransform.forward), true);
        }

        public virtual string GetCurrentSafeArea(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (IsInstanceMap())
                return _characterLocationsWhenEnterGame[playerCharacterEntity.Id].safeArea;
            return playerCharacterEntity.SafeArea != null ? playerCharacterEntity.SafeArea.name : string.Empty;
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
        public abstract bool IsInstanceMap();
    }
}
