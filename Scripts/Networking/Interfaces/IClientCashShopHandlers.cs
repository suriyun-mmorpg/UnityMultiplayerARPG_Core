using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientCashShopHandlers
    {
        bool RequestCashShopInfo(ResponseDelegate<ResponseCashShopInfoMessage> callback);
        bool RequestCashPackageInfo(ResponseDelegate<ResponseCashPackageInfoMessage> callback);
        bool RequestCashShopBuy(RequestCashShopBuyMessage data, ResponseDelegate<ResponseCashShopBuyMessage> callback);
        bool RequestCashPackageBuyValidation(RequestCashPackageBuyValidationMessage data, ResponseDelegate<ResponseCashPackageBuyValidationMessage> callback);
    }
}
