using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientMailHandlers : MonoBehaviour, IClientMailHandlers
    {
        public bool RequestMailList(RequestMailListMessage data, ResponseDelegate<ResponseMailListMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.MailList, data, responseDelegate: callback);
        }

        public bool RequestReadMail(RequestReadMailMessage data, ResponseDelegate<ResponseReadMailMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.ReadMail, data, responseDelegate: callback);
        }

        public bool RequestClaimMailItems(RequestClaimMailItemsMessage data, ResponseDelegate<ResponseClaimMailItemsMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.ClaimMailItems, data, responseDelegate: callback);
        }

        public bool RequestDeleteMail(RequestDeleteMailMessage data, ResponseDelegate<ResponseDeleteMailMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.DeleteMail, data, responseDelegate: callback);
        }

        public bool RequestSendMail(RequestSendMailMessage data, ResponseDelegate<ResponseSendMailMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.SendMail, data, responseDelegate: callback);
        }
    }
}
