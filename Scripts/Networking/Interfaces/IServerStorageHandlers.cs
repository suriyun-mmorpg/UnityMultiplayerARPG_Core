using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    /// <summary>
    /// These properties and functions will be called at server only
    /// </summary>
    public partial interface IServerStorageHandlers
    {
        /// <summary>
        /// Get all storages and all items which cached in current server
        /// </summary>
        /// <returns></returns>
        IDictionary<StorageId, List<CharacterItem>> GetAllStorageItems();

        /// <summary>
        /// Open storage
        /// </summary>
        /// <param name="connectionId">Client who open the storage</param>
        /// <param name="playerCharacter">Character which open the storage</param>
        /// <param name="storageEntity">The storage entity</param>
        /// <param name="storageId">Opening storage ID</param>
        UniTaskVoid OpenStorage(long connectionId, IPlayerCharacterData playerCharacter, IActivatableEntity storageEntity, StorageId storageId);

        /// <summary>
        /// Close storage
        /// </summary>
        /// <param name="connectionId">Client who close the storage</param>
        /// <param name="storageId">Which storage</param>
        UniTaskVoid CloseStorage(long connectionId, StorageId storageId);

        /// <summary>
        /// Close all storage
        /// </summary>
        /// <param name="connectionId">Client who close the storage</param>
        UniTaskVoid CloseAllStorages(long connectionId);

        /// <summary>
        /// Decrease items from storage, return items which going to drop on ground
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="convertItems"></param>
        /// <returns></returns>
        UniTask<List<CharacterItem>> ConvertStorageItems(StorageId storageId, List<StorageConvertItemsEntry> convertItems);

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
