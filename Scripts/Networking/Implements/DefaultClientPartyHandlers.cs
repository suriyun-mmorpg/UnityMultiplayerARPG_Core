using LiteNetLibManager;
using UnityEngine;
using ReqTypes = MultiplayerARPG.BaseGameNetworkManager.ReqTypes;

namespace MultiplayerARPG
{
    public class DefaultClientPartyHandlers : MonoBehaviour, IClientPartyHandlers
    {
        public PartyData ClientParty { get; set; }

        public bool RequestCreateParty(RequestCreatePartyMessage data, ResponseDelegate<ResponseCreatePartyMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.CreateParty, data, responseDelegate: callback);
        }

        public bool RequestChangePartyLeader(RequestChangePartyLeaderMessage data, ResponseDelegate<ResponseChangePartyLeaderMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.ChangePartyLeader, data, responseDelegate: callback);
        }

        public bool RequestChangePartySetting(RequestChangePartySettingMessage data, ResponseDelegate<ResponseChangePartySettingMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.ChangePartySetting, data, responseDelegate: callback);
        }

        public bool RequestSendPartyInvitation(RequestSendPartyInvitationMessage data, ResponseDelegate<ResponseSendPartyInvitationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.SendPartyInvitation, data, responseDelegate: callback);
        }

        public bool RequestAcceptPartyInvitation(RequestAcceptPartyInvitationMessage data, ResponseDelegate<ResponseAcceptPartyInvitationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.AcceptPartyInvitation, data, responseDelegate: callback);
        }

        public bool RequestDeclinePartyInvitation(RequestDeclinePartyInvitationMessage data, ResponseDelegate<ResponseDeclinePartyInvitationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.DeclinePartyInvitation, data, responseDelegate: callback);
        }

        public bool RequestKickMemberFromParty(RequestKickMemberFromPartyMessage data, ResponseDelegate<ResponseKickMemberFromPartyMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.KickMemberFromParty, data, responseDelegate: callback);
        }

        public bool RequestLeaveParty(RequestLeavePartyMessage data, ResponseDelegate<ResponseLeavePartyMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.LeaveParty, data, responseDelegate: callback);
        }
    }
}
