using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class LanRpgMailMessageHandlers : MonoBehaviour, IServerMailMessageHandlers
    {
        public IServerPlayerCharacterHandlers ServerPlayerCharacterHandlers { get; set; }

        public UniTaskVoid HandleRequestMailList(
            RequestHandlerData requestHandler, RequestMailListMessage request,
            RequestProceedResultDelegate<ResponseMailListMessage> result)
        {
            result.Invoke(AckResponseCode.Success, new ResponseMailListMessage());
            return default;
        }

        public UniTaskVoid HandleRequestReadMail(
            RequestHandlerData requestHandler, RequestReadMailMessage request,
            RequestProceedResultDelegate<ResponseReadMailMessage> result)
        {
            result.Invoke(AckResponseCode.Error, new ResponseReadMailMessage()
            {
                error = ResponseReadMailMessage.Error.NotAvailable,
            });
            return default;
        }

        public UniTaskVoid HandleRequestClaimMailItems(
            RequestHandlerData requestHandler, RequestClaimMailItemsMessage request,
            RequestProceedResultDelegate<ResponseClaimMailItemsMessage> result)
        {
            result.Invoke(AckResponseCode.Error, new ResponseClaimMailItemsMessage()
            {
                error = ResponseClaimMailItemsMessage.Error.NotAvailable,
            });
            return default;
        }

        public UniTaskVoid HandleRequestDeleteMail(
            RequestHandlerData requestHandler, RequestDeleteMailMessage request,
            RequestProceedResultDelegate<ResponseDeleteMailMessage> result)
        {
            result.Invoke(AckResponseCode.Error, new ResponseDeleteMailMessage()
            {
                error = ResponseDeleteMailMessage.Error.NotAvailable,
            });
            return default;
        }

        public UniTaskVoid HandleRequestSendMail(
            RequestHandlerData requestHandler, RequestSendMailMessage request,
            RequestProceedResultDelegate<ResponseSendMailMessage> result)
        {
            result.Invoke(AckResponseCode.Error, new ResponseSendMailMessage()
            {
                error = ResponseSendMailMessage.Error.NotAvailable,
            });
            return default;
        }
    }
}
