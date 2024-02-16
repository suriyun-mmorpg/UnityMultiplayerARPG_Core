using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial interface IServerUserContentMessageHandlers
    {
        UniTaskVoid HandleRequestAvailableContents(
            RequestHandlerData requestHandler, RequestAvailableContentsMessage request,
            RequestProceedResultDelegate<ResponseAvailableContentsMessage> result);

        UniTaskVoid HandleRequestUnlockContent(
            RequestHandlerData requestHandler, RequestUnlockContentMessage request,
            RequestProceedResultDelegate<ResponseUnlockContentMessage> result);
    }
}