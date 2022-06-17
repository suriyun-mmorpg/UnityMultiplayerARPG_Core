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
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseChangeGuildMessageMessage> onResponseChangeGuildMessage2;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseChangeGuildOptionsMessage> onResponseChangeGuildOptions;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseChangeGuildAutoAcceptRequestsMessage> onResponseChangeGuildAutoAcceptRequests;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseChangeGuildRoleMessage> onResponseChangeGuildRole;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseChangeMemberGuildRoleMessage> onResponseChangeMemberGuildRole;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseIncreaseGuildSkillLevelMessage> onResponseIncreaseGuildSkillLevel;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseSendGuildRequestMessage> onResponseSendGuildRequest;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseAcceptGuildRequestMessage> onResponseAcceptGuildRequest;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseDeclineGuildRequestMessage> onResponseDeclineGuildRequest;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseGetGuildRequestsMessage> onResponseGetGuildRequests;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseFindGuildsMessage> onResponseFindGuilds;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseGetGuildInfoMessage> onResponseGetGuildInfo;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseGuildRequestNotificationMessage> onResponseGuildRequestNotification;
        public static System.Action<GuildInvitationData> onNotifyGuildInvitation;
        public static System.Action<UpdateGuildMessage.UpdateType, GuildData> onNotifyGuildUpdated;
        public static System.Action<UpdateSocialMemberMessage.UpdateType, int, SocialCharacterData> onNotifyGuildMemberUpdated;

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

        public static void ResponseChangeGuildMessage2(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeGuildMessageMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseChangeGuildMessage2 != null)
                onResponseChangeGuildMessage2.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseChangeGuildOptions(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeGuildOptionsMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseChangeGuildOptions != null)
                onResponseChangeGuildOptions.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseChangeGuildAutoAcceptRequests(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeGuildAutoAcceptRequestsMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseChangeGuildAutoAcceptRequests != null)
                onResponseChangeGuildAutoAcceptRequests.Invoke(requestHandler, responseCode, response);
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

        public static void ResponseSendGuildRequest(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSendGuildRequestMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseSendGuildRequest != null)
                onResponseSendGuildRequest.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseAcceptGuildRequest(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseAcceptGuildRequestMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseAcceptGuildRequest != null)
                onResponseAcceptGuildRequest.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseDeclineGuildRequest(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseDeclineGuildRequestMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseDeclineGuildRequest != null)
                onResponseDeclineGuildRequest.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseGetGuildRequests(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseGetGuildRequestsMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseGetGuildRequests != null)
                onResponseGetGuildRequests.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseFindGuilds(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseFindGuildsMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseFindGuilds != null)
                onResponseFindGuilds.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseGetGuildInfo(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseGetGuildInfoMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseGetGuildInfo != null)
                onResponseGetGuildInfo.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseGuildRequestNotification(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseGuildRequestNotificationMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseGuildRequestNotification != null)
                onResponseGuildRequestNotification.Invoke(requestHandler, responseCode, response);
        }

        public static void NotifyGuildInvitation(GuildInvitationData invitation)
        {
            if (onNotifyGuildInvitation != null)
                onNotifyGuildInvitation.Invoke(invitation);
        }

        public static void NotifyGuildUpdated(UpdateGuildMessage.UpdateType updateType, GuildData guild)
        {
            if (onNotifyGuildUpdated != null)
                onNotifyGuildUpdated.Invoke(updateType, guild);
            if (guild == null)
                return;
            GuildInfoCacheManager.SetCache(new GuildListEntry()
            {
                Id = guild.id,
                GuildName = guild.guildName,
                Level = guild.level,
                FieldOptions = GuildListFieldOptions.All,
                GuildMessage = guild.guildMessage,
                GuildMessage2 = guild.guildMessage2,
                Score = guild.score,
                Options = guild.options,
                AutoAcceptRequests = guild.autoAcceptRequests,
                Rank = guild.rank,
                CurrentMembers = guild.CountMember(),
                MaxMembers = guild.MaxMember(),
            });
        }

        public static void NotifyGuildMemberUpdated(UpdateSocialMemberMessage.UpdateType updateType, int socialId, SocialCharacterData character)
        {
            if (onNotifyGuildMemberUpdated != null)
                onNotifyGuildMemberUpdated.Invoke(updateType, socialId, character);
        }
    }
}
