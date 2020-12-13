using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IServerPartyMessageHandlers
    {
        IServerPlayerCharacterHandlers ServerPlayerCharacterHandlers { get; set; }
        IServerPartyHandlers ServerPartyHandlers { get; set; }

        UniTaskVoid HandleRequestCreateParty(
            RequestHandlerData requestHandler, RequestCreatePartyMessage request,
            RequestProceedResultDelegate<ResponseCreatePartyMessage> result);

        UniTaskVoid HandleRequestChangePartyLeader(
            RequestHandlerData requestHandler, RequestChangePartyLeaderMessage request,
            RequestProceedResultDelegate<ResponseChangePartyLeaderMessage> result);

        UniTaskVoid HandleRequestChangePartySetting(
            RequestHandlerData requestHandler, RequestChangePartySettingMessage request,
            RequestProceedResultDelegate<ResponseChangePartySettingMessage> result);

        UniTaskVoid HandleRequestSendPartyInvitation(
            RequestHandlerData requestHandler, RequestSendPartyInvitationMessage request,
            RequestProceedResultDelegate<ResponseSendPartyInvitationMessage> result);

        UniTaskVoid HandleRequestAcceptPartyInvitation(
            RequestHandlerData requestHandler, RequestAcceptPartyInvitationMessage request,
            RequestProceedResultDelegate<ResponseAcceptPartyInvitationMessage> result);

        UniTaskVoid HandleRequestDeclinePartyInvitation(
            RequestHandlerData requestHandler, RequestDeclinePartyInvitationMessage request,
            RequestProceedResultDelegate<ResponseDeclinePartyInvitationMessage> result);

        UniTaskVoid HandleRequestKickMemberFromParty(
            RequestHandlerData requestHandler, RequestKickMemberFromPartyMessage request,
            RequestProceedResultDelegate<ResponseKickMemberFromPartyMessage> result);

        UniTaskVoid HandleRequestLeaveParty(
            RequestHandlerData requestHandler, RequestLeavePartyMessage request,
            RequestProceedResultDelegate<ResponseLeavePartyMessage> result);
    }
}
