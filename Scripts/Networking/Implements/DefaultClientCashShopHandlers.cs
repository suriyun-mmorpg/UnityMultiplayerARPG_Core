using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientCashShopHandlers : MonoBehaviour, IClientCashShopHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public bool RequestCashShopInfo(ResponseDelegate<ResponseCashShopInfoMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.CashShopInfo, new EmptyMessage(), responseDelegate: callback);
        }

        public bool RequestCashPackageInfo(ResponseDelegate<ResponseCashPackageInfoMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.CashPackageInfo, new EmptyMessage(), responseDelegate: callback);
        }

        public bool RequestCashShopBuy(RequestCashShopBuyMessage data, ResponseDelegate<ResponseCashShopBuyMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.CashShopBuy, data, responseDelegate: callback);
        }

        public bool RequestCashPackageBuyValidation(RequestCashPackageBuyValidationMessage data, ResponseDelegate<ResponseCashPackageBuyValidationMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.CashPackageBuyValidation, data, responseDelegate: callback);
        }
    }
}
