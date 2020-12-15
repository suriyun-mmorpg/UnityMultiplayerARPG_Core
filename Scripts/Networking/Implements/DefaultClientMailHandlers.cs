using LiteNetLibManager;
using UnityEngine;
using ReqTypes = MultiplayerARPG.BaseGameNetworkManager.ReqTypes;

namespace MultiplayerARPG
{
    public class DefaultClientMailHandlers : MonoBehaviour, IClientMailHandlers
    {
        public bool RequestMailList(RequestMailListMessage data, ResponseDelegate<ResponseMailListMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.MailList, data, responseDelegate: callback);
        }

        public bool RequestReadMail(RequestReadMailMessage data, ResponseDelegate<ResponseReadMailMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.ReadMail, data, responseDelegate: callback);
        }

        public bool RequestClaimMailItems(RequestClaimMailItemsMessage data, ResponseDelegate<ResponseClaimMailItemsMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.ClaimMailItems, data, responseDelegate: callback);
        }

        public bool RequestDeleteMail(RequestDeleteMailMessage data, ResponseDelegate<ResponseDeleteMailMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.DeleteMail, data, responseDelegate: callback);
        }

        public bool RequestSendMail(RequestSendMailMessage data, ResponseDelegate<ResponseSendMailMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.SendMail, data, responseDelegate: callback);
        }
    }
}
