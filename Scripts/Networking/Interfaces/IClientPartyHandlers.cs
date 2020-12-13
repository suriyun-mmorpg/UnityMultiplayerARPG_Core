using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientPartyHandlers
    {
        PartyData ClientParty { get; set; }

        bool RequestCreateParty(RequestCreatePartyMessage data, ResponseDelegate<ResponseCreatePartyMessage> callback);
        bool RequestChangePartyLeader(RequestChangePartyLeaderMessage data, ResponseDelegate<ResponseChangePartyLeaderMessage> callback);
        bool RequestChangePartyMessage(RequestChangePartySettingMessage data, ResponseDelegate<ResponseChangePartySettingMessage> callback);
        bool RequestSendPartyInvitation(RequestSendPartyInvitationMessage data, ResponseDelegate<ResponseSendPartyInvitationMessage> callback);
        bool RequestAcceptPartyInvitation(RequestAcceptPartyInvitationMessage data, ResponseDelegate<ResponseAcceptPartyInvitationMessage> callback);
        bool RequestDeclinePartyInvitation(RequestDeclinePartyInvitationMessage data, ResponseDelegate<ResponseDeclinePartyInvitationMessage> callback);
        bool RequestKickMemberFromParty(RequestKickMemberFromPartyMessage data, ResponseDelegate<ResponseKickMemberFromPartyMessage> callback);
        bool RequestLeaveParty(RequestLeavePartyMessage data, ResponseDelegate<ResponseLeavePartyMessage> callback);
    }
}
