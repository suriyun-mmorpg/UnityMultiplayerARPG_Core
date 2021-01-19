using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static class ClientPartyActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseSendPartyInvitationMessage> onResponseSendPartyInvitation;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseAcceptPartyInvitationMessage> onResponseAcceptPartyInvitation;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseDeclinePartyInvitationMessage> onResponseDeclinePartyInvitation;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseCreatePartyMessage> onResponseCreateParty;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseChangePartyLeaderMessage> onResponseChangePartyLeader;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseKickMemberFromPartyMessage> onResponseKickMemberFromParty;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseLeavePartyMessage> onResponseLeaveParty;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseChangePartySettingMessage> onResponseChangePartySetting;
        public static System.Action<PartyInvitationData> onNotifyPartyInvitation;
        public static System.Action<PartyData> onNotifyPartyUpdated;

        public static async UniTaskVoid ResponseSendPartyInvitation(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSendPartyInvitationMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseSendPartyInvitation != null)
                onResponseSendPartyInvitation.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseAcceptPartyInvitation(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseAcceptPartyInvitationMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseAcceptPartyInvitation != null)
                onResponseAcceptPartyInvitation.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseDeclinePartyInvitation(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseDeclinePartyInvitationMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseDeclinePartyInvitation != null)
                onResponseDeclinePartyInvitation.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseCreateParty(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCreatePartyMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseCreateParty != null)
                onResponseCreateParty.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseChangePartyLeader(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangePartyLeaderMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseChangePartyLeader != null)
                onResponseChangePartyLeader.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseKickMemberFromParty(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseKickMemberFromPartyMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseKickMemberFromParty != null)
                onResponseKickMemberFromParty.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseLeaveParty(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseLeavePartyMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseLeaveParty != null)
                onResponseLeaveParty.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseChangePartySetting(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangePartySettingMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseChangePartySetting != null)
                onResponseChangePartySetting.Invoke(requestHandler, responseCode, response);
        }

        public static void NotifyPartyInvitation(PartyInvitationData invitation)
        {
            if (onNotifyPartyInvitation != null)
                onNotifyPartyInvitation.Invoke(invitation);
        }

        public static void NotifyPartyUpdated(PartyData party)
        {
            if (onNotifyPartyUpdated != null)
                onNotifyPartyUpdated.Invoke(party);
        }
    }
}
