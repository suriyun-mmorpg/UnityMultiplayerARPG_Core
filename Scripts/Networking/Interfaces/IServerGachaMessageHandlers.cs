using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    /// <summary>
    /// These properties and functions will be called at server only
    /// </summary>
    public partial interface IServerGachaMessageHandlers
    {
        UniTaskVoid HandleRequestOpenGacha(
            RequestHandlerData requestHandler, RequestOpenGachaMessage request,
            RequestProceedResultDelegate<ResponseOpenGachaMessage> result);
    }
}
