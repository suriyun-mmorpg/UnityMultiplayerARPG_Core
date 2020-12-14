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

        #region Activity functions
        public virtual void RespawnCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            string respawnMapName = playerCharacterEntity.RespawnMapName;
            Vector3 respawnPosition = playerCharacterEntity.RespawnPosition;
            if (CurrentMapInfo != null)
            {
                CurrentMapInfo.GetRespawnPoint(playerCharacterEntity, out respawnMapName, out respawnPosition);
            }
            WarpCharacter(playerCharacterEntity, respawnMapName, respawnPosition, false, Vector3.zero);
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
            if (CurrentGameInstance.currentPositionSaveMode == CurrentPositionSaveMode.UseRespawnPosition ||
                !CurrentMapInfo.SaveCurrentMapPosition)
                return playerCharacterEntity.RespawnPosition;
            return playerCharacterEntity.CacheTransform.position;
        }

        public virtual void SetCurrentPosition(BasePlayerCharacterEntity playerCharacterEntity, Vector3 position)
        {
            playerCharacterEntity.Teleport(position);
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
        /// Deposit gold
        /// </summary>
        /// <param name="playerCharacterEntity">Character who deposit gold</param>
        /// <param name="amount">Amount of gold</param>
        public abstract void DepositGold(BasePlayerCharacterEntity playerCharacterEntity, int amount);

        /// <summary>
        /// Withdraw gold
        /// </summary>
        /// <param name="playerCharacterEntity">Character who withdraw gold</param>
        /// <param name="amount">Amount of gold</param>
        public abstract void WithdrawGold(BasePlayerCharacterEntity playerCharacterEntity, int amount);

        /// <summary>
        /// Deposit guild gold
        /// </summary>
        /// <param name="playerCharacterEntity">Character who deposit gold</param>
        /// <param name="amount">Amount of gold</param>
        public abstract void DepositGuildGold(BasePlayerCharacterEntity playerCharacterEntity, int amount);

        /// <summary>
        /// Withdraw guild gold
        /// </summary>
        /// <param name="playerCharacterEntity">Character who withdraw gold</param>
        /// <param name="amount">Amount of gold</param>
        public abstract void WithdrawGuildGold(BasePlayerCharacterEntity playerCharacterEntity, int amount);

        /// <summary>
        /// Find characters by name
        /// </summary>
        /// <param name="playerCharacterEntity">Character who find other characters</param>
        /// <param name="characterName">Character name</param>
        public abstract void FindCharacters(BasePlayerCharacterEntity playerCharacterEntity, string characterName);

        /// <summary>
        /// Add friend
        /// </summary>
        /// <param name="playerCharacterEntity">Character who adding friend</param>
        /// <param name="friendCharacterId">Id of character whom will be added</param>
        public abstract void AddFriend(BasePlayerCharacterEntity playerCharacterEntity, string friendCharacterId);

        /// <summary>
        /// Remove friend
        /// </summary>
        /// <param name="playerCharacterEntity">Character who removing friend</param>
        /// <param name="friendCharacterId">Id of character whom will be removed</param>
        public abstract void RemoveFriend(BasePlayerCharacterEntity playerCharacterEntity, string friendCharacterId);

        /// <summary>
        /// Get friends
        /// </summary>
        /// <param name="playerCharacterEntity">Character who request friend list</param>
        public abstract void GetFriends(BasePlayerCharacterEntity playerCharacterEntity);

        /// <summary>
        /// Warp character to other map if `mapName` is not empty
        /// </summary>
        /// <param name="playerCharacterEntity"></param>
        /// <param name="mapName"></param>
        /// <param name="position"></param>
        /// <param name="overrideRotation"></param>
        /// <param name="rotation"></param>
        protected abstract void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation);

        /// <summary>
        /// Warp character to instance map
        /// </summary>
        /// <param name="playerCharacterEntity"></param>
        /// <param name="mapName"></param>
        /// <param name="position"></param>
        /// <param name="overrideRotation"></param>
        /// <param name="rotation"></param>
        protected abstract void WarpCharacterToInstance(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation);

        /// <summary>
        /// Check if this game network manager is for instance map or not
        /// </summary>
        /// <returns></returns>
        protected abstract bool IsInstanceMap();
    }
}
