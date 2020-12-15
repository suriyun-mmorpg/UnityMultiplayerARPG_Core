using LiteNetLibManager;
using UnityEngine;
using ReqTypes = MultiplayerARPG.BaseGameNetworkManager.ReqTypes;

namespace MultiplayerARPG
{
    public class DefaultClientCashShopHandlers : MonoBehaviour, IClientCashShopHandlers
    {
        public bool RequestCashShopInfo(ResponseDelegate<ResponseCashShopInfoMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.CashShopInfo, new EmptyMessage(), responseDelegate: callback);
        }

        public bool RequestCashPackageInfo(ResponseDelegate<ResponseCashPackageInfoMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.CashPackageInfo, new EmptyMessage(), responseDelegate: callback);
        }

        public bool RequestCashShopBuy(RequestCashShopBuyMessage data, ResponseDelegate<ResponseCashShopBuyMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.CashShopBuy, data, responseDelegate: callback);
        }

        public bool RequestCashPackageBuyValidation(RequestCashPackageBuyValidationMessage data, ResponseDelegate<ResponseCashPackageBuyValidationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.CashPackageBuyValidation, data, responseDelegate: callback);
        }
    }
}
