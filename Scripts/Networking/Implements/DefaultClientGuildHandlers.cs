using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientGuildHandlers : MonoBehaviour, IClientGuildHandlers
    {
        public GuildData ClientGuild { get; set; }

        public bool RequestCreateGuild(RequestCreateGuildMessage data, ResponseDelegate<ResponseCreateGuildMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.CreateGuild, data, responseDelegate: callback);
        }

        public bool RequestChangeGuildLeader(RequestChangeGuildLeaderMessage data, ResponseDelegate<ResponseChangeGuildLeaderMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.ChangeGuildLeader, data, responseDelegate: callback);
        }

        public bool RequestChangeGuildMessage(RequestChangeGuildMessageMessage data, ResponseDelegate<ResponseChangeGuildMessageMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.ChangeGuildMessage, data, responseDelegate: callback);
        }

        public bool RequestChangeGuildRole(RequestChangeGuildRoleMessage data, ResponseDelegate<ResponseChangeGuildRoleMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.ChangeGuildRole, data, responseDelegate: callback);
        }

        public bool RequestChangeMemberGuildRole(RequestChangeMemberGuildRoleMessage data, ResponseDelegate<ResponseChangeMemberGuildRoleMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.ChangeMemberGuildRole, data, responseDelegate: callback);
        }

        public bool RequestSendGuildInvitation(RequestSendGuildInvitationMessage data, ResponseDelegate<ResponseSendGuildInvitationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.SendGuildInvitation, data, responseDelegate: callback);
        }

        public bool RequestAcceptGuildInvitation(RequestAcceptGuildInvitationMessage data, ResponseDelegate<ResponseAcceptGuildInvitationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.AcceptGuildInvitation, data, responseDelegate: callback);
        }

        public bool RequestDeclineGuildInvitation(RequestDeclineGuildInvitationMessage data, ResponseDelegate<ResponseDeclineGuildInvitationMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.DeclineGuildInvitation, data, responseDelegate: callback);
        }

        public bool RequestKickMemberFromGuild(RequestKickMemberFromGuildMessage data, ResponseDelegate<ResponseKickMemberFromGuildMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.KickMemberFromGuild, data, responseDelegate: callback);
        }

        public bool RequestLeaveGuild(ResponseDelegate<ResponseLeaveGuildMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.LeaveGuild, new EmptyMessage(), responseDelegate: callback);
        }

        public bool RequestIncreaseGuildSkillLevel(RequestIncreaseGuildSkillLevelMessage data, ResponseDelegate<ResponseIncreaseGuildSkillLevelMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.IncreaseGuildSkillLevel, data, responseDelegate: callback);
        }
    }
}
