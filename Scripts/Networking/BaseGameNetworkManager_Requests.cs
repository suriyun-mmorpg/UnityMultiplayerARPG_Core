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

        public bool RequestCashShopBuy(RequestCashShopBuyMessage data, ResponseDelegate<ResponseCashShopBuyMessage> callback)
        {
            return ClientSendRequest(ReqTypes.CashShopBuy, data, responseDelegate: callback);
        }

        public bool RequestCashPackageBuyValidation(RequestCashPackageBuyValidationMessage data, ResponseDelegate<ResponseCashPackageBuyValidationMessage> callback)
        {
            return ClientSendRequest(ReqTypes.CashPackageBuyValidation, data, responseDelegate: callback);
        }
        #endregion

        #region Mail requests
        public bool RequestMailList(RequestMailListMessage data, ResponseDelegate<ResponseMailListMessage> callback)
        {
            return ClientSendRequest(ReqTypes.MailList, data, responseDelegate: callback);
        }

        public bool RequestReadMail(RequestReadMailMessage data, ResponseDelegate<ResponseReadMailMessage> callback)
        {
            return ClientSendRequest(ReqTypes.ReadMail, data, responseDelegate: callback);
        }

        public bool RequestClaimMailItems(RequestClaimMailItemsMessage data, ResponseDelegate<ResponseClaimMailItemsMessage> callback)
        {
            return ClientSendRequest(ReqTypes.ClaimMailItems, data, responseDelegate: callback);
        }

        public bool RequestDeleteMail(RequestDeleteMailMessage data, ResponseDelegate<ResponseDeleteMailMessage> callback)
        {
            return ClientSendRequest(ReqTypes.DeleteMail, data, responseDelegate: callback);
        }

        public bool RequestSendMail(RequestSendMailMessage data, ResponseDelegate<ResponseSendMailMessage> callback)
        {
            return ClientSendRequest(ReqTypes.SendMail, data, responseDelegate: callback);
        }
        #endregion

        #region Storage requests
        public bool RequestGetStorageItems(RequestGetStorageItemsMessage data, ResponseDelegate<ResponseGetStorageItemsMessage> callback)
        {
            return ClientSendRequest(ReqTypes.GetStorageItems, data, responseDelegate: callback);
        }

        public bool RequestMoveItemFromStorage(RequestMoveItemFromStorageMessage data, ResponseDelegate<ResponseMoveItemFromStorageMessage> callback)
        {
            return ClientSendRequest(ReqTypes.MoveItemFromStorage, data, responseDelegate: callback);
        }

        public bool RequestMoveItemToStorage(RequestMoveItemToStorageMessage data, ResponseDelegate<ResponseMoveItemToStorageMessage> callback)
        {
            return ClientSendRequest(ReqTypes.MoveItemToStorage, data, responseDelegate: callback);
        }

        public bool RequestSwapOrMergeStorageItem(RequestSwapOrMergeStorageItemMessage data, ResponseDelegate<ResponseSwapOrMergeStorageItemMessage> callback)
        {
            return ClientSendRequest(ReqTypes.SwapOrMergeStorageItem, data, responseDelegate: callback);
        }
        #endregion

        #region Inventory requests
        public bool RequestSwapOrMergeItem(RequestSwapOrMergeItemMessage data, ResponseDelegate<ResponseSwapOrMergeItemMessage> callback)
        {
            return ClientSendRequest(ReqTypes.SwapOrMergeItem, data, responseDelegate: callback);
        }

        public bool RequestEquipWeapon(RequestEquipWeaponMessage data, ResponseDelegate<ResponseEquipWeaponMessage> callback)
        {
            return ClientSendRequest(ReqTypes.EquipWeapon, data, responseDelegate: callback);
        }

        public bool RequestEquipArmor(RequestEquipArmorMessage data, ResponseDelegate<ResponseEquipArmorMessage> callback)
        {
            return ClientSendRequest(ReqTypes.EquipArmor, data, responseDelegate: callback);
        }

        public bool RequestUnEquipWeapon(RequestUnEquipWeaponMessage data, ResponseDelegate<ResponseUnEquipWeaponMessage> callback)
        {
            return ClientSendRequest(ReqTypes.UnEquipWeapon, data, responseDelegate: callback);
        }

        public bool RequestUnEquipArmor(RequestUnEquipArmorMessage data, ResponseDelegate<ResponseUnEquipArmorMessage> callback)
        {
            return ClientSendRequest(ReqTypes.UnEquipArmor, data, responseDelegate: callback);
        }
        #endregion
    }
}
