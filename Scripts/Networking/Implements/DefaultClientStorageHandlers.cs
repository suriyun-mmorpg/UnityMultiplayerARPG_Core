using LiteNetLibManager;
using UnityEngine;
using ReqTypes = MultiplayerARPG.BaseGameNetworkManager.ReqTypes;

namespace MultiplayerARPG
{
    public class DefaultClientStorageHandlers : MonoBehaviour, IClientStorageHandlers
    {
        public bool RequestGetStorageItems(RequestGetStorageItemsMessage data, ResponseDelegate<ResponseGetStorageItemsMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.GetStorageItems, data, responseDelegate: callback);
        }

        public bool RequestMoveItemFromStorage(RequestMoveItemFromStorageMessage data, ResponseDelegate<ResponseMoveItemFromStorageMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.MoveItemFromStorage, data, responseDelegate: callback);
        }

        public bool RequestMoveItemToStorage(RequestMoveItemToStorageMessage data, ResponseDelegate<ResponseMoveItemToStorageMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.MoveItemToStorage, data, responseDelegate: callback);
        }

        public bool RequestSwapOrMergeStorageItem(RequestSwapOrMergeStorageItemMessage data, ResponseDelegate<ResponseSwapOrMergeStorageItemMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.SwapOrMergeStorageItem, data, responseDelegate: callback);
        }
    }
}
