using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientPartyHandlers : MonoBehaviour, IClientPartyHandlers
    {
        public PartyData ClientParty { get; set; }

        public bool RequestCreateParty(RequestCreatePartyMessage data, ResponseDelegate<ResponseCreatePartyMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.CreateParty, data, responseDelegate: callback);
        }

        public bool RequestChangePartyLeader(RequestChangePartyLeaderMessage data, ResponseDelegate<ResponseChangePartyLeaderMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.ChangePartyLeader, data, responseDelegate: callback);
        }

        public bool RequestChangePartySetting(RequestChangePartySettingMessage data, ResponseDelegate<ResponseChangePartySettingMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.ChangePartySetting, data, responseDelegate: callback);
        }

        public bool RequestSendPartyInvitation(RequestSendPartyInvitationMessage data, ResponseDelegate<ResponseSendPartyInvitationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.SendPartyInvitation, data, responseDelegate: callback);
        }

        public bool RequestAcceptPartyInvitation(RequestAcceptPartyInvitationMessage data, ResponseDelegate<ResponseAcceptPartyInvitationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.AcceptPartyInvitation, data, responseDelegate: callback);
        }

        public bool RequestDeclinePartyInvitation(RequestDeclinePartyInvitationMessage data, ResponseDelegate<ResponseDeclinePartyInvitationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.DeclinePartyInvitation, data, responseDelegate: callback);
        }

        public bool RequestKickMemberFromParty(RequestKickMemberFromPartyMessage data, ResponseDelegate<ResponseKickMemberFromPartyMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.KickMemberFromParty, data, responseDelegate: callback);
        }

        public bool RequestLeaveParty(RequestLeavePartyMessage data, ResponseDelegate<ResponseLeavePartyMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.LeaveParty, data, responseDelegate: callback);
        }
    }
}
