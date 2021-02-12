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

        public static void ResponseSendGuildInvitation(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSendGuildInvitationMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseSendGuildInvitation != null)
                onResponseSendGuildInvitation.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseAcceptGuildInvitation(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseAcceptGuildInvitationMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseAcceptGuildInvitation != null)
                onResponseAcceptGuildInvitation.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseDeclineGuildInvitation(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseDeclineGuildInvitationMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseDeclineGuildInvitation != null)
                onResponseDeclineGuildInvitation.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseCreateGuild(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCreateGuildMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseCreateGuild != null)
                onResponseCreateGuild.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseChangeGuildLeader(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeGuildLeaderMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseChangeGuildLeader != null)
                onResponseChangeGuildLeader.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseKickMemberFromGuild(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseKickMemberFromGuildMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseKickMemberFromGuild != null)
                onResponseKickMemberFromGuild.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseLeaveGuild(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseLeaveGuildMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseLeaveGuild != null)
                onResponseLeaveGuild.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseChangeGuildMessage(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeGuildMessageMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseChangeGuildMessage != null)
                onResponseChangeGuildMessage.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseChangeGuildRole(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeGuildRoleMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseChangeGuildRole != null)
                onResponseChangeGuildRole.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseChangeMemberGuildRole(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeMemberGuildRoleMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseChangeMemberGuildRole != null)
                onResponseChangeMemberGuildRole.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseIncreaseGuildSkillLevel(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseIncreaseGuildSkillLevelMessage response)
        {
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
