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
        /// Open storage
        /// </summary>
        /// <param name="playerCharacterEntity">Character who open the storage</param>
        void OpenStorage(BasePlayerCharacterEntity playerCharacterEntity);

        /// <summary>
        /// Close storage
        /// </summary>
        /// <param name="playerCharacterEntity">Character who close the storage</param>
        void CloseStorage(BasePlayerCharacterEntity playerCharacterEntity);

        /// <summary>
        /// Increase items to storage
        /// </summary>
        /// <param name="playerCharacterEntity"></param>
        /// <param name="storageId"></param>
        /// <param name="addingItem"></param>
        UniTask<bool> IncreaseStorageItems(StorageId storageId, CharacterItem addingItem);

        /// <summary>
        /// Decrease items from storage
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="dataId"></param>
        /// <param name="amount"></param>
        /// <param name="decreaseItems"></param>
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
        /// <returns></returns>
        Storage GetStorage(StorageId storageId);

        /// <summary>
        /// Can access storage or not?
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="playerCharacter"></param>
        /// <returns></returns>
        bool CanAccessStorage(StorageId storageId, IPlayerCharacterData playerCharacter);
    }
}
