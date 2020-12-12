using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    /// <summary>
    /// These properties and functions will be called at server only
    /// </summary>
    public interface IServerCashShopMessageHandlers
    {
        IServerPlayerCharacterHandlers ServerPlayerCharacterHandlers { get; set; }

        UniTaskVoid HandleRequestCashShopInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashShopInfoMessage> result);

        UniTaskVoid HandleRequestCashPackageInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashPackageInfoMessage> result);

        UniTaskVoid HandleRequestCashShopBuy(
            RequestHandlerData requestHandler, RequestCashShopBuyMessage request,
            RequestProceedResultDelegate<ResponseCashShopBuyMessage> result);

        UniTaskVoid HandleRequestCashPackageBuyValidation(
            RequestHandlerData requestHandler, RequestCashPackageBuyValidationMessage request,
            RequestProceedResultDelegate<ResponseCashPackageBuyValidationMessage> result);
    }
}
