using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientGuildHandlers
    {
        GuildData ClientGuild { get; set; }
        bool RequestCreateGuild(RequestCreateGuildMessage data, ResponseDelegate<ResponseCreateGuildMessage> callback);
        bool RequestChangeGuildLeader(RequestChangeGuildLeaderMessage data, ResponseDelegate<ResponseChangeGuildLeaderMessage> callback);
        bool RequestChangeGuildMessage(RequestChangeGuildMessageMessage data, ResponseDelegate<ResponseChangeGuildMessageMessage> callback);
        bool RequestChangeGuildRole(RequestChangeGuildRoleMessage data, ResponseDelegate<ResponseChangeGuildRoleMessage> callback);
        bool RequestChangeMemberGuildRole(RequestChangeMemberGuildRoleMessage data, ResponseDelegate<ResponseChangeMemberGuildRoleMessage> callback);
        bool RequestSendGuildInvitation(RequestSendGuildInvitationMessage data, ResponseDelegate<ResponseSendGuildInvitationMessage> callback);
        bool RequestAcceptGuildInvitation(RequestAcceptGuildInvitationMessage data, ResponseDelegate<ResponseAcceptGuildInvitationMessage> callback);
        bool RequestDeclineGuildInvitation(RequestDeclineGuildInvitationMessage data, ResponseDelegate<ResponseDeclineGuildInvitationMessage> callback);
        bool RequestKickMemberFromGuild(RequestKickMemberFromGuildMessage data, ResponseDelegate<ResponseKickMemberFromGuildMessage> callback);
        bool RequestLeaveGuild(RequestLeaveGuildMessage data, ResponseDelegate<ResponseLeaveGuildMessage> callback);
        bool RequestIncreaseGuildSkillLevel(RequestIncreaseGuildSkillLevelMessage data, ResponseDelegate<ResponseIncreaseGuildSkillLevelMessage> callback);
    }
}
