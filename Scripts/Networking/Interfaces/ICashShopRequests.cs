using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface ICashShopRequests
    {
        bool RequestCashShopInfo(ResponseDelegate<ResponseCashShopInfoMessage> callback);
        bool RequestCashPackageInfo(ResponseDelegate<ResponseCashPackageInfoMessage> callback);
        bool RequestCashShopBuy(int dataId, ResponseDelegate<ResponseCashShopBuyMessage> callback);
        bool RequestCashPackageBuyValidation(int dataId, string receipt, ResponseDelegate<ResponseCashPackageBuyValidationMessage> callback);
    }
}
