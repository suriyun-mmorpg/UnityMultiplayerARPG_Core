using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        #region Cash shop requests
        public bool RequestCashShopInfo(ResponseDelegate<ResponseCashShopInfoMessage> callback)
        {
            return ClientSendRequest(ReqTypes.CashShopInfo, new EmptyMessage(), responseDelegate: callback);
        }

        public bool RequestCashPackageInfo(ResponseDelegate<ResponseCashPackageInfoMessage> callback)
        {
            return ClientSendRequest(ReqTypes.CashPackageInfo, new EmptyMessage(), responseDelegate: callback);
        }

        public bool RequestCashShopBuy(int dataId, ResponseDelegate<ResponseCashShopBuyMessage> callback)
        {
            return ClientSendRequest(ReqTypes.CashShopBuy, new RequestCashShopBuyMessage()
            {
                dataId = dataId,
            }, responseDelegate: callback);
        }

        public bool RequestCashPackageBuyValidation(int dataId, string receipt, ResponseDelegate<ResponseCashPackageBuyValidationMessage> callback)
        {
            return ClientSendRequest(ReqTypes.CashPackageBuyValidation, new RequestCashPackageBuyValidationMessage()
            {
                dataId = dataId,
                platform = Application.platform,
                receipt = receipt,
            }, responseDelegate: callback);
        }
        #endregion

        #region Mail requests
        public bool RequestMailList(bool onlyNewMails, ResponseDelegate<ResponseMailListMessage> callback)
        {
            return ClientSendRequest(ReqTypes.MailList, new RequestMailListMessage()
            {
                onlyNewMails = onlyNewMails,
            }, responseDelegate: callback);
        }

        public bool RequestReadMail(string mailId, ResponseDelegate<ResponseReadMailMessage> callback)
        {
            return ClientSendRequest(ReqTypes.ReadMail, new RequestReadMailMessage()
            {
                id = mailId,
            }, responseDelegate: callback);
        }

        public bool RequestClaimMailItems(string mailId, ResponseDelegate<ResponseClaimMailItemsMessage> callback)
        {
            return ClientSendRequest(ReqTypes.ClaimMailItems, new RequestClaimMailItemsMessage()
            {
                id = mailId,
            }, responseDelegate: callback);
        }

        public bool RequestDeleteMail(string mailId, ResponseDelegate<ResponseDeleteMailMessage> callback)
        {
            return ClientSendRequest(ReqTypes.DeleteMail, new RequestDeleteMailMessage()
            {
                id = mailId,
            }, responseDelegate: callback);
        }

        public bool RequestSendMail(string receiverName, string title, string content, int gold, ResponseDelegate<ResponseSendMailMessage> callback)
        {
            return ClientSendRequest(ReqTypes.SendMail, new RequestSendMailMessage()
            {
                receiverName = receiverName,
                title = title,
                content = content,
                gold = gold,
            }, responseDelegate: callback);
        }
        #endregion

        #region Storage requests
        public bool RequestGetStorageItems(string characterId, StorageType storageType, string storageOwnerId, ResponseDelegate<ResponseGetStorageItemsMessage> callback)
        {
            return ClientSendRequest(ReqTypes.GetStorageItems, new RequestGetStorageItemsMessage()
            {
                characterId = characterId,
                storageType = storageType,
                storageOwnerId = storageOwnerId,
            }, responseDelegate: callback);
        }

        public bool RequestMoveItemFromStorage(string characterId, StorageType storageType, string storageOwnerId, short storageItemIndex, short amount, short inventoryIndex, ResponseDelegate<ResponseMoveItemFromStorageMessage> callback)
        {
            return ClientSendRequest(ReqTypes.MoveItemFromStorage, new RequestMoveItemFromStorageMessage()
            {
                characterId = characterId,
                storageType = storageType,
                storageOwnerId = storageOwnerId,
                storageItemIndex = storageItemIndex,
                amount = amount,
                inventoryIndex = inventoryIndex,
            }, responseDelegate: callback);
        }

        public bool RequestMoveItemToStorage(string characterId, StorageType storageType, string storageOwnerId, short inventoryIndex, short amount, short storageItemIndex, ResponseDelegate<ResponseMoveItemToStorageMessage> callback)
        {
            return ClientSendRequest(ReqTypes.MoveItemToStorage, new RequestMoveItemToStorageMessage()
            {
                characterId = characterId,
                storageType = storageType,
                storageOwnerId = storageOwnerId,
                inventoryIndex = inventoryIndex,
                amount = amount,
                storageItemIndex = storageItemIndex,
            }, responseDelegate: callback);
        }

        public bool RequestSwapOrMergeStorageItem(string characterId, StorageType storageType, string storageOwnerId, short fromIndex, short toIndex, ResponseDelegate<ResponseSwapOrMergeStorageItemMessage> callback)
        {
            return ClientSendRequest(ReqTypes.SwapOrMergeStorageItem, new RequestSwapOrMergeStorageItemMessage()
            {
                characterId = characterId,
                storageType = storageType,
                storageOwnerId = storageOwnerId,
                fromIndex = fromIndex,
                toIndex = toIndex,
            }, responseDelegate: callback);
        }
        #endregion
    }
}
