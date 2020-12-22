using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientCashShopHandlers : MonoBehaviour, IClientCashShopHandlers
    {
        public bool RequestCashShopInfo(ResponseDelegate<ResponseCashShopInfoMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.CashShopInfo, new EmptyMessage(), responseDelegate: callback);
        }

        public bool RequestCashPackageInfo(ResponseDelegate<ResponseCashPackageInfoMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.CashPackageInfo, new EmptyMessage(), responseDelegate: callback);
        }

        public bool RequestCashShopBuy(RequestCashShopBuyMessage data, ResponseDelegate<ResponseCashShopBuyMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.CashShopBuy, data, responseDelegate: callback);
        }

        public bool RequestCashPackageBuyValidation(RequestCashPackageBuyValidationMessage data, ResponseDelegate<ResponseCashPackageBuyValidationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.CashPackageBuyValidation, data, responseDelegate: callback);
        }
    }
}
