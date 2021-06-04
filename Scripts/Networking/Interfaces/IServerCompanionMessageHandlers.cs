using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial interface IServerCompanionMessageHandlers
    {
        UniTaskVoid HandleRequestGetCompanions(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseGetCompanionsMessage> result);

        UniTaskVoid HandleRequestSelectCompanion(
            RequestHandlerData requestHandler, RequestSelectCompanionMessage request,
            RequestProceedResultDelegate<ResponseSelectCompanionMessage> result);
    }
}
