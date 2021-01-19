using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static class ClientGuildActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseSendGuildInvitationMessage> onResponseSendGuildInvitation;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseAcceptGuildInvitationMessage> onResponseAcceptGuildInvitation;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseDeclineGuildInvitationMessage> onResponseDeclineGuildInvitation;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseCreateGuildMessage> onResponseCreateGuild;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseChangeGuildLeaderMessage> onResponseChangeGuildLeader;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseKickMemberFromGuildMessage> onResponseKickMemberFromGuild;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseLeaveGuildMessage> onResponseLeaveGuild;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseChangeGuildMessageMessage> onResponseChangeGuildMessage;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseChangeGuildRoleMessage> onResponseChangeGuildRole;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseChangeMemberGuildRoleMessage> onResponseChangeMemberGuildRole;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseIncreaseGuildSkillLevelMessage> onResponseIncreaseGuildSkillLevel;
        public static System.Action<GuildInvitationData> onNotifyGuildInvitation;
        public static System.Action<GuildData> onNotifyGuildUpdated;

        public static async UniTaskVoid ResponseSendGuildInvitation(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSendGuildInvitationMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseSendGuildInvitation != null)
                onResponseSendGuildInvitation.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseAcceptGuildInvitation(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseAcceptGuildInvitationMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseAcceptGuildInvitation != null)
                onResponseAcceptGuildInvitation.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseDeclineGuildInvitation(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseDeclineGuildInvitationMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseDeclineGuildInvitation != null)
                onResponseDeclineGuildInvitation.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseCreateGuild(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCreateGuildMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseCreateGuild != null)
                onResponseCreateGuild.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseChangeGuildLeader(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeGuildLeaderMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseChangeGuildLeader != null)
                onResponseChangeGuildLeader.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseKickMemberFromGuild(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseKickMemberFromGuildMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseKickMemberFromGuild != null)
                onResponseKickMemberFromGuild.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseLeaveGuild(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseLeaveGuildMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseLeaveGuild != null)
                onResponseLeaveGuild.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseChangeGuildMessage(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeGuildMessageMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseChangeGuildMessage != null)
                onResponseChangeGuildMessage.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseChangeGuildRole(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeGuildRoleMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseChangeGuildRole != null)
                onResponseChangeGuildRole.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseChangeMemberGuildRole(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeMemberGuildRoleMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseChangeMemberGuildRole != null)
                onResponseChangeMemberGuildRole.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseIncreaseGuildSkillLevel(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseIncreaseGuildSkillLevelMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseIncreaseGuildSkillLevel != null)
                onResponseIncreaseGuildSkillLevel.Invoke(requestHandler, responseCode, response);
        }

        public static void NotifyGuildInvitation(GuildInvitationData invitation)
        {
            if (onNotifyGuildInvitation != null)
                onNotifyGuildInvitation.Invoke(invitation);
        }

        public static void NotifyGuildUpdated(GuildData guild)
        {
            if (onNotifyGuildUpdated != null)
                onNotifyGuildUpdated.Invoke(guild);
        }
    }
}
