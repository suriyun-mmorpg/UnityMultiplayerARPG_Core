using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IServerGuildMessageHandlers
    {
        IServerPlayerCharacterHandlers ServerPlayerCharacterHandlers { get; set; }
        IServerGuildHandlers ServerGuildHandlers { get; set; }

        UniTaskVoid HandleRequestCreateGuild(
            RequestHandlerData requestHandler, RequestCreateGuildMessage request,
            RequestProceedResultDelegate<ResponseCreateGuildMessage> result);

        UniTaskVoid HandleRequestChangeGuildLeader(
            RequestHandlerData requestHandler, RequestChangeGuildLeaderMessage request,
            RequestProceedResultDelegate<ResponseChangeGuildLeaderMessage> result);

        UniTaskVoid HandleRequestChangeGuildMessage(
            RequestHandlerData requestHandler, RequestChangeGuildMessageMessage request,
            RequestProceedResultDelegate<ResponseChangeGuildMessageMessage> result);

        UniTaskVoid HandleRequestChangeGuildRole(
            RequestHandlerData requestHandler, RequestChangeGuildRoleMessage request,
            RequestProceedResultDelegate<ResponseChangeGuildRoleMessage> result);

        UniTaskVoid HandleRequestChangeMemberGuildRole(
            RequestHandlerData requestHandler, RequestChangeMemberGuildRoleMessage request,
            RequestProceedResultDelegate<ResponseChangeMemberGuildRoleMessage> result);

        UniTaskVoid HandleRequestSendGuildInvitation(
            RequestHandlerData requestHandler, RequestSendGuildInvitationMessage request,
            RequestProceedResultDelegate<ResponseSendGuildInvitationMessage> result);

        UniTaskVoid HandleRequestAcceptGuildInvitation(
            RequestHandlerData requestHandler, RequestAcceptGuildInvitationMessage request,
            RequestProceedResultDelegate<ResponseAcceptGuildInvitationMessage> result);

        UniTaskVoid HandleRequestDeclineGuildInvitation(
            RequestHandlerData requestHandler, RequestDeclineGuildInvitationMessage request,
            RequestProceedResultDelegate<ResponseDeclineGuildInvitationMessage> result);

        UniTaskVoid HandleRequestKickMemberFromGuild(
            RequestHandlerData requestHandler, RequestKickMemberFromGuildMessage request,
            RequestProceedResultDelegate<ResponseKickMemberFromGuildMessage> result);

        UniTaskVoid HandleRequestLeaveGuild(
            RequestHandlerData requestHandler, RequestLeaveGuildMessage request,
            RequestProceedResultDelegate<ResponseLeaveGuildMessage> result);

        UniTaskVoid HandleRequestIncreaseGuildSkillLevel(
            RequestHandlerData requestHandler, RequestIncreaseGuildSkillLevelMessage request,
            RequestProceedResultDelegate<ResponseIncreaseGuildSkillLevelMessage> result);
    }
}
