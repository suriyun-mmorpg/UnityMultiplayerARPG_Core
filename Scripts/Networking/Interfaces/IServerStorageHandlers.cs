using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    /// <summary>
    /// These properties and functions will be called at server only
    /// </summary>
    public interface IServerStorageHandlers
    {
        /// <summary>
        /// Get all storages and all items which cached in current server
        /// </summary>
        /// <returns></returns>
        IDictionary<StorageId, List<CharacterItem>> GetAllStorageItems();

        /// <summary>
        /// Open storage
        /// </summary>
        /// <param name="playerCharacter">Character who open the storage</param>
        UniTaskVoid OpenStorage(BasePlayerCharacterEntity playerCharacter);

        /// <summary>
        /// Close storage
        /// </summary>
        /// <param name="playerCharacter">Character who close the storage</param>
        UniTaskVoid CloseStorage(BasePlayerCharacterEntity playerCharacter);

        /// <summary>
        /// Increase items to storage
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="addingItem"></param>
        UniTask<bool> IncreaseStorageItems(StorageId storageId, CharacterItem addingItem);

        /// <summary>
        /// Decrease items from storage
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="dataId"></param>
        /// <param name="amount"></param>
        UniTask<DecreaseStorageItemsResult> DecreaseStorageItems(StorageId storageId, int dataId, short amount);

        /// <summary>
        /// Get storage items by storage Id
        /// </summary>
        /// <param name="storageId"></param>
        /// <returns></returns>
        List<CharacterItem> GetStorageItems(StorageId storageId);

        /// <summary>
        /// Set storage items to collection
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="items"></param>
        void SetStorageItems(StorageId storageId, List<CharacterItem> items);

        /// <summary>
        /// Check if storage entity is opened or not
        /// </summary>
        /// <param name="storageEntity">Checking storage entity</param>
        /// <returns></returns>
        bool IsStorageEntityOpen(StorageEntity storageEntity);

        /// <summary>
        /// Get items from storage entity
        /// </summary>
        /// <param name="storageEntity"></param>
        /// <returns></returns>
        List<CharacterItem> GetStorageEntityItems(StorageEntity storageEntity);

        /// <summary>
        /// Get storage settings by storage Id
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        Storage GetStorage(StorageId storageId, out uint objectId);

        /// <summary>
        /// Can access storage or not?
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="playerCharacter"></param>
        /// <returns></returns>
        bool CanAccessStorage(IPlayerCharacterData playerCharacter, StorageId storageId);

        /// <summary>
        /// This will be used to clear data relates to storage system
        /// </summary>
        void ClearStorage();

        /// <summary>
        /// Notify to clients which using storage
        /// </summary>
        /// <param name="storageType"></param>
        /// <param name="storageOwnerId"></param>
        void NotifyStorageItemsUpdated(StorageType storageType, string storageOwnerId);
    }
}
