using LiteNetLibManager;
using UnityEngine;
using ReqTypes = MultiplayerARPG.BaseGameNetworkManager.ReqTypes;

namespace MultiplayerARPG
{
    public class DefaultClientGuildHandlers : MonoBehaviour, IClientGuildHandlers
    {
        public GuildData ClientGuild { get; set; }

        public bool RequestCreateGuild(RequestCreateGuildMessage data, ResponseDelegate<ResponseCreateGuildMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.CreateGuild, data, responseDelegate: callback);
        }

        public bool RequestChangeGuildLeader(RequestChangeGuildLeaderMessage data, ResponseDelegate<ResponseChangeGuildLeaderMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.ChangeGuildLeader, data, responseDelegate: callback);
        }

        public bool RequestChangeGuildMessage(RequestChangeGuildMessageMessage data, ResponseDelegate<ResponseChangeGuildMessageMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.ChangeGuildMessage, data, responseDelegate: callback);
        }

        public bool RequestChangeGuildRole(RequestChangeGuildRoleMessage data, ResponseDelegate<ResponseChangeGuildRoleMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.ChangeGuildRole, data, responseDelegate: callback);
        }

        public bool RequestChangeMemberGuildRole(RequestChangeMemberGuildRoleMessage data, ResponseDelegate<ResponseChangeMemberGuildRoleMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.ChangeMemberGuildRole, data, responseDelegate: callback);
        }

        public bool RequestSendGuildInvitation(RequestSendGuildInvitationMessage data, ResponseDelegate<ResponseSendGuildInvitationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.SendGuildInvitation, data, responseDelegate: callback);
        }

        public bool RequestAcceptGuildInvitation(RequestAcceptGuildInvitationMessage data, ResponseDelegate<ResponseAcceptGuildInvitationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.AcceptGuildInvitation, data, responseDelegate: callback);
        }

        public bool RequestDeclineGuildInvitation(RequestDeclineGuildInvitationMessage data, ResponseDelegate<ResponseDeclineGuildInvitationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.DeclineGuildInvitation, data, responseDelegate: callback);
        }

        public bool RequestKickMemberFromGuild(RequestKickMemberFromGuildMessage data, ResponseDelegate<ResponseKickMemberFromGuildMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.KickMemberFromGuild, data, responseDelegate: callback);
        }

        public bool RequestLeaveGuild(RequestLeaveGuildMessage data, ResponseDelegate<ResponseLeaveGuildMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.LeaveGuild, data, responseDelegate: callback);
        }

        public bool RequestIncreaseGuildSkillLevel(RequestIncreaseGuildSkillLevelMessage data, ResponseDelegate<ResponseIncreaseGuildSkillLevelMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.IncreaseGuildSkillLevel, data, responseDelegate: callback);
        }
    }
}
