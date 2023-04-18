using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class LanRpgServerGuildMessageHandlers : MonoBehaviour, IServerGuildMessageHandlers
    {
        public static int Id { get; set; }

        public async UniTaskVoid HandleRequestAcceptGuildInvitation(RequestHandlerData requestHandler, RequestAcceptGuildInvitationMessage request, RequestProceedResultDelegate<ResponseAcceptGuildInvitationMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out BasePlayerCharacterEntity playerCharacter))
            {
                result.InvokeError(new ResponseAcceptGuildInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanAcceptGuildInvitation(request.guildId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseAcceptGuildInvitationMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            playerCharacter.GuildId = request.guildId;
            validateResult.Guild.AddMember(playerCharacter);
            GameInstance.ServerGuildHandlers.SetGuild(request.guildId, validateResult.Guild);
            GameInstance.ServerGuildHandlers.RemoveGuildInvitation(request.guildId, playerCharacter.Id);
            GameInstance.ServerGameMessageHandlers.SendSetFullGuildData(requestHandler.ConnectionId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendAddGuildMemberToMembers(validateResult.Guild, SocialCharacterData.Create(playerCharacter));
            // Send message to inviter
            GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(request.inviterId, UITextKeys.UI_GUILD_INVITATION_ACCEPTED);
            // Response to invitee
            result.InvokeSuccess(new ResponseAcceptGuildInvitationMessage()
            {
                message = UITextKeys.UI_GUILD_INVITATION_ACCEPTED,
            });
        }

        public async UniTaskVoid HandleRequestDeclineGuildInvitation(RequestHandlerData requestHandler, RequestDeclineGuildInvitationMessage request, RequestProceedResultDelegate<ResponseDeclineGuildInvitationMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out BasePlayerCharacterEntity playerCharacter))
            {
                result.InvokeError(new ResponseDeclineGuildInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanDeclineGuildInvitation(request.guildId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseDeclineGuildInvitationMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            GameInstance.ServerGuildHandlers.RemoveGuildInvitation(request.guildId, playerCharacter.Id);
            // Send message to inviter
            GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(request.inviterId, UITextKeys.UI_GUILD_INVITATION_DECLINED);
            // Response to invitee
            result.InvokeSuccess(new ResponseDeclineGuildInvitationMessage()
            {
                message = UITextKeys.UI_GUILD_INVITATION_DECLINED,
            });
        }

        public async UniTaskVoid HandleRequestSendGuildInvitation(RequestHandlerData requestHandler, RequestSendGuildInvitationMessage request, RequestProceedResultDelegate<ResponseSendGuildInvitationMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out BasePlayerCharacterEntity playerCharacter))
            {
                result.InvokeError(new ResponseSendGuildInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.inviteeId, out BasePlayerCharacterEntity inviteeCharacter))
            {
                result.InvokeError(new ResponseSendGuildInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanSendGuildInvitation(playerCharacter, inviteeCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseSendGuildInvitationMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            GameInstance.ServerGuildHandlers.AppendGuildInvitation(playerCharacter.GuildId, request.inviteeId);
            GameInstance.ServerGameMessageHandlers.SendNotifyGuildInvitation(inviteeCharacter.ConnectionId, new GuildInvitationData()
            {
                InviterId = playerCharacter.Id,
                InviterName = playerCharacter.CharacterName,
                InviterLevel = playerCharacter.Level,
                GuildId = validateResult.GuildId,
                GuildName = validateResult.Guild.guildName,
                GuildLevel = validateResult.Guild.level,
            });
            result.InvokeSuccess(new ResponseSendGuildInvitationMessage());
        }

        public async UniTaskVoid HandleRequestCreateGuild(RequestHandlerData requestHandler, RequestCreateGuildMessage request, RequestProceedResultDelegate<ResponseCreateGuildMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseCreateGuildMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = playerCharacter.CanCreateGuild(request.guildName);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseCreateGuildMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            GuildData guild = new GuildData(++Id, request.guildName, SocialCharacterData.Create(playerCharacter), GameInstance.Singleton.SocialSystemSetting.GuildMemberRoles);
            GameInstance.Singleton.SocialSystemSetting.DecreaseCreateGuildResource(playerCharacter);
            GameInstance.ServerGuildHandlers.SetGuild(guild.id, guild);
            playerCharacter.GuildId = guild.id;
            playerCharacter.GuildRole = guild.GetMemberRole(playerCharacter.Id);
            playerCharacter.SharedGuildExp = 0;
            GameInstance.ServerGameMessageHandlers.SendSetFullGuildData(requestHandler.ConnectionId, guild);
            result.InvokeSuccess(new ResponseCreateGuildMessage());
        }

        public async UniTaskVoid HandleRequestChangeGuildLeader(RequestHandlerData requestHandler, RequestChangeGuildLeaderMessage request, RequestProceedResultDelegate<ResponseChangeGuildLeaderMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeGuildLeaderMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildLeader(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildLeaderMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            validateResult.Guild.SetLeader(request.memberId);
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendSetGuildLeaderToMembers(validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendSetGuildMemberRoleToMembers(validateResult.Guild, request.memberId, 0);
            result.InvokeSuccess(new ResponseChangeGuildLeaderMessage());
        }

        public async UniTaskVoid HandleRequestKickMemberFromGuild(RequestHandlerData requestHandler, RequestKickMemberFromGuildMessage request, RequestProceedResultDelegate<ResponseKickMemberFromGuildMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseKickMemberFromGuildMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanKickMemberFromGuild(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseKickMemberFromGuildMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.memberId, out IPlayerCharacterData memberCharacter) &&
                GameInstance.ServerUserHandlers.TryGetConnectionId(request.memberId, out long memberConnectionId))
            {
                memberCharacter.ClearGuild();
                GameInstance.ServerGameMessageHandlers.SendClearGuildData(memberConnectionId, validateResult.GuildId);
            }
            validateResult.Guild.RemoveMember(request.memberId);
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendRemoveGuildMemberToMembers(validateResult.Guild, request.memberId);
            result.InvokeSuccess(new ResponseKickMemberFromGuildMessage());
        }

        public async UniTaskVoid HandleRequestLeaveGuild(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseLeaveGuildMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseLeaveGuildMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanLeaveGuild(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseLeaveGuildMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            if (validateResult.Guild.IsLeader(playerCharacter.Id))
            {
                IPlayerCharacterData memberCharacter;
                long memberConnectionId;
                foreach (string memberId in validateResult.Guild.GetMemberIds())
                {
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(memberId, out memberCharacter) &&
                        GameInstance.ServerUserHandlers.TryGetConnectionId(memberId, out memberConnectionId))
                    {
                        memberCharacter.ClearGuild();
                        GameInstance.ServerGameMessageHandlers.SendClearGuildData(memberConnectionId, validateResult.GuildId);
                    }
                }
                GameInstance.ServerGuildHandlers.RemoveGuild(validateResult.GuildId);
            }
            else
            {
                playerCharacter.ClearGuild();
                validateResult.Guild.RemoveMember(playerCharacter.Id);
                GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
                GameInstance.ServerGameMessageHandlers.SendRemoveGuildMemberToMembers(validateResult.Guild, playerCharacter.Id);
                GameInstance.ServerGameMessageHandlers.SendClearGuildData(requestHandler.ConnectionId, validateResult.GuildId);
            }
            result.InvokeSuccess(new ResponseLeaveGuildMessage());
        }

        public async UniTaskVoid HandleRequestChangeGuildMessage(RequestHandlerData requestHandler, RequestChangeGuildMessageMessage request, RequestProceedResultDelegate<ResponseChangeGuildMessageMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeGuildMessageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildMessage(playerCharacter, request.message);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildMessageMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            validateResult.Guild.guildMessage = request.message;
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendSetGuildMessageToMembers(validateResult.Guild);
            result.InvokeSuccess(new ResponseChangeGuildMessageMessage());
        }

        public async UniTaskVoid HandleRequestChangeGuildMessage2(RequestHandlerData requestHandler, RequestChangeGuildMessageMessage request, RequestProceedResultDelegate<ResponseChangeGuildMessageMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeGuildMessageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildMessage2(playerCharacter, request.message);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildMessageMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            validateResult.Guild.guildMessage2 = request.message;
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendSetGuildMessageToMembers(validateResult.Guild);
            result.InvokeSuccess(new ResponseChangeGuildMessageMessage());
        }

        public async UniTaskVoid HandleRequestChangeGuildOptions(RequestHandlerData requestHandler, RequestChangeGuildOptionsMessage request, RequestProceedResultDelegate<ResponseChangeGuildOptionsMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeGuildOptionsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildOptions(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildOptionsMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            validateResult.Guild.options = request.options;
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendSetGuildOptionsToMembers(validateResult.Guild);
            result.InvokeSuccess(new ResponseChangeGuildOptionsMessage());
        }

        public async UniTaskVoid HandleRequestChangeGuildAutoAcceptRequests(RequestHandlerData requestHandler, RequestChangeGuildAutoAcceptRequestsMessage request, RequestProceedResultDelegate<ResponseChangeGuildAutoAcceptRequestsMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeGuildAutoAcceptRequestsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildOptions(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildAutoAcceptRequestsMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            validateResult.Guild.autoAcceptRequests = request.autoAcceptRequests;
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendSetGuildAutoAcceptRequestsToMembers(validateResult.Guild);
            result.InvokeSuccess(new ResponseChangeGuildAutoAcceptRequestsMessage());
        }

        public async UniTaskVoid HandleRequestChangeGuildRole(RequestHandlerData requestHandler, RequestChangeGuildRoleMessage request, RequestProceedResultDelegate<ResponseChangeGuildRoleMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeGuildRoleMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (request.guildRoleData.shareExpPercentage > GameInstance.Singleton.SocialSystemSetting.MaxShareExpPercentage)
                request.guildRoleData.shareExpPercentage = GameInstance.Singleton.SocialSystemSetting.MaxShareExpPercentage;
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildRole(playerCharacter, request.guildRole, request.guildRoleData.roleName);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeGuildRoleMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            validateResult.Guild.SetRole(request.guildRole, request.guildRoleData);
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Change characters guild role
            IPlayerCharacterData memberCharacter;
            foreach (string memberId in validateResult.Guild.GetMemberIds())
            {
                if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(memberId, out memberCharacter))
                    memberCharacter.GuildRole = validateResult.Guild.GetMemberRole(memberCharacter.Id);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildRoleToMembers(validateResult.Guild, request.guildRole, request.guildRoleData);
            result.InvokeSuccess(new ResponseChangeGuildRoleMessage());
        }

        public async UniTaskVoid HandleRequestChangeMemberGuildRole(RequestHandlerData requestHandler, RequestChangeMemberGuildRoleMessage request, RequestProceedResultDelegate<ResponseChangeMemberGuildRoleMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangeMemberGuildRoleMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildMemberRole(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangeMemberGuildRoleMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            validateResult.Guild.SetMemberRole(request.memberId, request.guildRole);
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.memberId, out IPlayerCharacterData memberCharacter))
                memberCharacter.GuildRole = validateResult.Guild.GetMemberRole(memberCharacter.Id);
            GameInstance.ServerGameMessageHandlers.SendSetGuildMemberRoleToMembers(validateResult.Guild, request.memberId, request.guildRole);
            result.InvokeSuccess(new ResponseChangeMemberGuildRoleMessage());
        }

        public async UniTaskVoid HandleRequestIncreaseGuildSkillLevel(RequestHandlerData requestHandler, RequestIncreaseGuildSkillLevelMessage request, RequestProceedResultDelegate<ResponseIncreaseGuildSkillLevelMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseIncreaseGuildSkillLevelMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanIncreaseGuildSkillLevel(playerCharacter, request.dataId);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseIncreaseGuildSkillLevelMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            validateResult.Guild.AddSkillLevel(request.dataId);
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendSetGuildSkillLevelToMembers(validateResult.Guild, request.dataId);
            GameInstance.ServerGameMessageHandlers.SendSetGuildLevelExpSkillPointToMembers(validateResult.Guild);
            result.InvokeSuccess(new ResponseIncreaseGuildSkillLevelMessage());
        }

        public async UniTaskVoid HandleRequestSendGuildRequest(RequestHandlerData requestHandler, RequestSendGuildRequestMessage request, RequestProceedResultDelegate<ResponseSendGuildRequestMessage> result)
        {
            result.Invoke(AckResponseCode.Unimplemented, new ResponseSendGuildRequestMessage());
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestAcceptGuildRequest(RequestHandlerData requestHandler, RequestAcceptGuildRequestMessage request, RequestProceedResultDelegate<ResponseAcceptGuildRequestMessage> result)
        {
            result.Invoke(AckResponseCode.Unimplemented, new ResponseAcceptGuildRequestMessage());
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestDeclineGuildRequest(RequestHandlerData requestHandler, RequestDeclineGuildRequestMessage request, RequestProceedResultDelegate<ResponseDeclineGuildRequestMessage> result)
        {
            result.Invoke(AckResponseCode.Unimplemented, new ResponseDeclineGuildRequestMessage());
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestGetGuildRequests(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseGetGuildRequestsMessage> result)
        {
            result.Invoke(AckResponseCode.Unimplemented, new ResponseGetGuildRequestsMessage());
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestFindGuilds(RequestHandlerData requestHandler, RequestFindGuildsMessage request, RequestProceedResultDelegate<ResponseFindGuildsMessage> result)
        {
            result.Invoke(AckResponseCode.Unimplemented, new ResponseFindGuildsMessage());
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestGetGuildInfo(RequestHandlerData requestHandler, RequestGetGuildInfoMessage request, RequestProceedResultDelegate<ResponseGetGuildInfoMessage> result)
        {
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseGetGuildInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            if (!GameInstance.ServerGuildHandlers.TryGetGuild(request.guildId, out GuildData guild))
            {
                result.InvokeError(new ResponseGetGuildInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_GUILD_NOT_FOUND,
                });
                return;
            }

            result.InvokeSuccess(new ResponseGetGuildInfoMessage()
            {
                guild = new GuildListEntry()
                {
                    Id = guild.id,
                    GuildName = guild.guildName,
                    Level = guild.level,
                    FieldOptions = GuildListFieldOptions.Options,
                    Options = guild.options,
                }
            });
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestGuildRequestNotification(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseGuildRequestNotificationMessage> result)
        {
            result.Invoke(AckResponseCode.Unimplemented, new ResponseGuildRequestNotificationMessage());
            await UniTask.Yield();
        }
    }
}
