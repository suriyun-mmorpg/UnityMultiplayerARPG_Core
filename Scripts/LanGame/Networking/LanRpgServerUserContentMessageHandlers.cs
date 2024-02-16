using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class LanRpgServerUserContentMessageHandlers : MonoBehaviour, IServerUserContentMessageHandlers
    {
        public UniTaskVoid HandleRequestAvailableContents(RequestHandlerData requestHandler, RequestAvailableContentsMessage request, RequestProceedResultDelegate<ResponseAvailableContentsMessage> result)
        {
            // Do nothing
            return default;
        }

        public UniTaskVoid HandleRequestUnlockContent(RequestHandlerData requestHandler, RequestUnlockContentMessage request, RequestProceedResultDelegate<ResponseUnlockContentMessage> result)
        {
            // Do nothing
            return default;
        }
    }
}
