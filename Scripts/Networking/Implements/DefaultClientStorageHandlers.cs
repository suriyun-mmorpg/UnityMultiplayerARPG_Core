using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientStorageHandlers : MonoBehaviour, IClientStorageHandlers
    {
        public StorageType StorageType { get; set; }
        public string StorageOwnerId { get; set; }

        public bool RequestMoveItemFromStorage(RequestMoveItemFromStorageMessage data, ResponseDelegate<ResponseMoveItemFromStorageMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.MoveItemFromStorage, data, responseDelegate: callback);
        }

        public bool RequestMoveItemToStorage(RequestMoveItemToStorageMessage data, ResponseDelegate<ResponseMoveItemToStorageMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.MoveItemToStorage, data, responseDelegate: callback);
        }

        public bool RequestSwapOrMergeStorageItem(RequestSwapOrMergeStorageItemMessage data, ResponseDelegate<ResponseSwapOrMergeStorageItemMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.SwapOrMergeStorageItem, data, responseDelegate: callback);
        }
    }
}
