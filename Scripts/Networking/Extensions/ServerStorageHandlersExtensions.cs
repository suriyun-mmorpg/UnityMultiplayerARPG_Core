namespace MultiplayerARPG
{
    public static partial class ServerStorageHandlersExtensions
    {
        public static bool GetStorageId(this IPlayerCharacterData playerCharacter, StorageType storageType, uint objectId, out StorageId storageId)
        {
            storageId = StorageId.Empty;
            switch (storageType)
            {
                case StorageType.Player:
                    storageId = new StorageId(storageType, playerCharacter.UserId);
                    return true;
                case StorageType.Guild:
                    if (playerCharacter.GuildId <= 0)
                        return false;
                    storageId = new StorageId(storageType, playerCharacter.GuildId.ToString());
                    return true;
                case StorageType.Building:
                    StorageEntity buildingEntity;
                    if (!BaseGameNetworkManager.Singleton.TryGetEntityByObjectId(objectId, out buildingEntity))
                        return false;
                    storageId = new StorageId(storageType, buildingEntity.Id);
                    return true;
            }
            return false;
        }

        public static Storage GetStorage(this StorageId storageId, out uint objectId)
        {
            objectId = 0;
            Storage storage = default(Storage);
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    storage = GameInstance.Singleton.playerStorage;
                    break;
                case StorageType.Guild:
                    storage = GameInstance.Singleton.guildStorage;
                    break;
                case StorageType.Building:
                    StorageEntity buildingEntity;
                    if (GameInstance.ServerBuildingHandlers.TryGetBuilding(storageId.storageOwnerId, out buildingEntity))
                    {
                        objectId = buildingEntity.ObjectId;
                        storage = buildingEntity.Storage;
                    }
                    break;
            }
            return storage;
        }

        public static bool CanAccessStorage(this IPlayerCharacterData playerCharacter, StorageId storageId)
        {
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    if (!playerCharacter.UserId.Equals(storageId.storageOwnerId))
                        return false;
                    break;
                case StorageType.Guild:
                    if (!GameInstance.ServerGuildHandlers.ContainsGuild(playerCharacter.GuildId) ||
                        !playerCharacter.GuildId.ToString().Equals(storageId.storageOwnerId))
                        return false;
                    break;
                case StorageType.Building:
                    StorageEntity buildingEntity;
                    if (!GameInstance.ServerBuildingHandlers.TryGetBuilding(storageId.storageOwnerId, out buildingEntity) ||
                        !(buildingEntity.IsCreator(playerCharacter.Id) || buildingEntity.CanUseByEveryone))
                        return false;
                    break;
            }
            return true;
        }
    }
}
