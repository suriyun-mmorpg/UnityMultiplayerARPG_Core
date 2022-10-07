using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial interface IServerOnlineCharacterMessageHandlers
    {
        UniTaskVoid HandleRequestGetOnlineCharacterData(
            RequestHandlerData requestHandler, RequestGetOnlineCharacterDataMessage request,
            RequestProceedResultDelegate<ResponseGetOnlineCharacterDataMessage> result);
    }
}
