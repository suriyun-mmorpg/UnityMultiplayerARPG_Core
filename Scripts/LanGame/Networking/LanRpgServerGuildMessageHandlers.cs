using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class LanRpgServerGuildMessageHandlers : MonoBehaviour, IServerGuildMessageHandlers
    {
        public static int Id { get; set; }

        public async UniTaskVoid HandleRequestAcceptGuildInvitation(RequestHandlerData requestHandler, RequestAcceptGuildInvitationMessage request, RequestProceedResultDelegate<ResponseAcceptGuildInvitationMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseAcceptGuildInvitationMessage()
                {
                    error = ResponseAcceptGuildInvitationMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanAcceptGuildInvitation(request.guildId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseAcceptGuildInvitationMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotFoundGuild:
                        error = ResponseAcceptGuildInvitationMessage.Error.GuildNotFound;
                        break;
                    case GameMessage.Type.NotFoundGuildInvitation:
                        error = ResponseAcceptGuildInvitationMessage.Error.InvitationNotFound;
                        break;
                    case GameMessage.Type.JoinedAnotherGuild:
                        error = ResponseAcceptGuildInvitationMessage.Error.AlreadyJoined;
                        break;
                    default:
                        error = ResponseAcceptGuildInvitationMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseAcceptGuildInvitationMessage()
                {
                    error = error,
                });
                return;
            }
            playerCharacter.GuildId = request.guildId;
            validateResult.Guild.AddMember(playerCharacter);
            GameInstance.ServerGuildHandlers.SetGuild(request.guildId, validateResult.Guild);
            GameInstance.ServerGuildHandlers.RemoveGuildInvitation(request.guildId, playerCharacter.Id);
            GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.GuildInvitationAccepted);
            GameInstance.ServerGameMessageHandlers.SendSetGuildData(requestHandler.ConnectionId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendAddGuildMembersToOne(requestHandler.ConnectionId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendAddGuildMembersToMembers(validateResult.Guild, playerCharacter.Id, playerCharacter.CharacterName, playerCharacter.DataId, playerCharacter.Level);
            result.Invoke(AckResponseCode.Success, new ResponseAcceptGuildInvitationMessage());
        }

        public async UniTaskVoid HandleRequestDeclineGuildInvitation(RequestHandlerData requestHandler, RequestDeclineGuildInvitationMessage request, RequestProceedResultDelegate<ResponseDeclineGuildInvitationMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseDeclineGuildInvitationMessage()
                {
                    error = ResponseDeclineGuildInvitationMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanDeclineGuildInvitation(request.guildId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseDeclineGuildInvitationMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotFoundGuild:
                        error = ResponseDeclineGuildInvitationMessage.Error.GuildNotFound;
                        break;
                    case GameMessage.Type.NotFoundGuildInvitation:
                        error = ResponseDeclineGuildInvitationMessage.Error.InvitationNotFound;
                        break;
                    default:
                        error = ResponseDeclineGuildInvitationMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseDeclineGuildInvitationMessage()
                {
                    error = error,
                });
                return;
            }
            GameInstance.ServerGuildHandlers.RemoveGuildInvitation(request.guildId, playerCharacter.Id);
            GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.GuildInvitationDeclined);
            result.Invoke(AckResponseCode.Success, new ResponseDeclineGuildInvitationMessage());
        }

        public async UniTaskVoid HandleRequestSendGuildInvitation(RequestHandlerData requestHandler, RequestSendGuildInvitationMessage request, RequestProceedResultDelegate<ResponseSendGuildInvitationMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseSendGuildInvitationMessage()
                {
                    error = ResponseSendGuildInvitationMessage.Error.NotLoggedIn,
                });
                return;
            }
            BasePlayerCharacterEntity inviteeCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.inviteeId, out inviteeCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSendGuildInvitationMessage()
                {
                    error = ResponseSendGuildInvitationMessage.Error.InviteeNotFound,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanSendGuildInvitation(playerCharacter, inviteeCharacter);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseSendGuildInvitationMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.CannotSendGuildInvitation:
                        error = ResponseSendGuildInvitationMessage.Error.NotAllowed;
                        break;
                    case GameMessage.Type.CharacterJoinedAnotherGuild:
                        error = ResponseSendGuildInvitationMessage.Error.InviteeNotAvailable;
                        break;
                    default:
                        error = ResponseSendGuildInvitationMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseSendGuildInvitationMessage()
                {
                    error = error,
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
            result.Invoke(AckResponseCode.Success, new ResponseSendGuildInvitationMessage());
        }

        public async UniTaskVoid HandleRequestCreateGuild(RequestHandlerData requestHandler, RequestCreateGuildMessage request, RequestProceedResultDelegate<ResponseCreateGuildMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseCreateGuildMessage()
                {
                    error = ResponseCreateGuildMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = playerCharacter.CanCreateGuild(request.guildName);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseCreateGuildMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.JoinedAnotherGuild:
                        error = ResponseCreateGuildMessage.Error.AlreadyJoined;
                        break;
                    default:
                        error = ResponseCreateGuildMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseCreateGuildMessage()
                {
                    error = error,
                });
                return;
            }
            GuildData guild = new GuildData(++Id, request.guildName, playerCharacter);
            GameInstance.Singleton.SocialSystemSetting.DecreaseCreateGuildResource(playerCharacter);
            GameInstance.ServerGuildHandlers.SetGuild(guild.id, guild);
            playerCharacter.GuildId = guild.id;
            playerCharacter.GuildRole = guild.GetMemberRole(playerCharacter.Id);
            playerCharacter.SharedGuildExp = 0;
            playerCharacter.GuildName = request.guildName;
            GameInstance.ServerGameMessageHandlers.SendSetGuildData(requestHandler.ConnectionId, guild);
            GameInstance.ServerGameMessageHandlers.SendAddGuildMembersToOne(requestHandler.ConnectionId, guild);
            result.Invoke(AckResponseCode.Success, new ResponseCreateGuildMessage());
        }

        public async UniTaskVoid HandleRequestChangeGuildLeader(RequestHandlerData requestHandler, RequestChangeGuildLeaderMessage request, RequestProceedResultDelegate<ResponseChangeGuildLeaderMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangeGuildLeaderMessage()
                {
                    error = ResponseChangeGuildLeaderMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildLeader(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseChangeGuildLeaderMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseChangeGuildLeaderMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotGuildLeader:
                        error = ResponseChangeGuildLeaderMessage.Error.NotAllowed;
                        break;
                    case GameMessage.Type.CharacterNotJoinedGuild:
                        error = ResponseChangeGuildLeaderMessage.Error.MemberNotFound;
                        break;
                    default:
                        error = ResponseChangeGuildLeaderMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseChangeGuildLeaderMessage()
                {
                    error = error,
                });
                return;
            }
            validateResult.Guild.SetLeader(request.memberId);
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendSetGuildLeaderToMembers(validateResult.Guild);
            result.Invoke(AckResponseCode.Success, new ResponseChangeGuildLeaderMessage());
        }

        public async UniTaskVoid HandleRequestKickMemberFromGuild(RequestHandlerData requestHandler, RequestKickMemberFromGuildMessage request, RequestProceedResultDelegate<ResponseKickMemberFromGuildMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseKickMemberFromGuildMessage()
                {
                    error = ResponseKickMemberFromGuildMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanKickMemberFromGuild(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseKickMemberFromGuildMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseKickMemberFromGuildMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.CannotKickGuildLeader:
                    case GameMessage.Type.CannotKickYourSelfFromGuild:
                    case GameMessage.Type.NotGuildLeader:
                        error = ResponseKickMemberFromGuildMessage.Error.NotAllowed;
                        break;
                    case GameMessage.Type.CharacterNotJoinedGuild:
                        error = ResponseKickMemberFromGuildMessage.Error.MemberNotFound;
                        break;
                    default:
                        error = ResponseKickMemberFromGuildMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseKickMemberFromGuildMessage()
                {
                    error = error,
                });
                return;
            }
            BasePlayerCharacterEntity memberEntity;
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.memberId, out memberEntity))
            {
                memberEntity.ClearGuild();
                GameInstance.ServerGameMessageHandlers.SendClearGuildData(memberEntity.ConnectionId, validateResult.GuildId);
            }
            validateResult.Guild.RemoveMember(request.memberId);
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendRemoveGuildMemberToMembers(validateResult.Guild, request.memberId);
            result.Invoke(AckResponseCode.Success, new ResponseKickMemberFromGuildMessage());
        }

        public async UniTaskVoid HandleRequestLeaveGuild(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseLeaveGuildMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseLeaveGuildMessage()
                {
                    error = ResponseLeaveGuildMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanLeaveGuild(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseLeaveGuildMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseLeaveGuildMessage.Error.NotJoined;
                        break;
                    default:
                        error = ResponseLeaveGuildMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseLeaveGuildMessage()
                {
                    error = error,
                });
                return;
            }
            if (validateResult.Guild.IsLeader(playerCharacter.Id))
            {
                BasePlayerCharacterEntity memberEntity;
                foreach (string memberId in validateResult.Guild.GetMemberIds())
                {
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(memberId, out memberEntity))
                    {
                        memberEntity.ClearGuild();
                        GameInstance.ServerGameMessageHandlers.SendClearGuildData(memberEntity.ConnectionId, validateResult.GuildId);
                    }
                }
                GameInstance.ServerGuildHandlers.RemoveGuild(validateResult.GuildId);
            }
            else
            {
                playerCharacter.ClearGuild();
                GameInstance.ServerGameMessageHandlers.SendClearGuildData(playerCharacter.ConnectionId, validateResult.GuildId);
                validateResult.Guild.RemoveMember(playerCharacter.Id);
                GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
                GameInstance.ServerGameMessageHandlers.SendRemoveGuildMemberToMembers(validateResult.Guild, playerCharacter.Id);
            }
            result.Invoke(AckResponseCode.Success, new ResponseLeaveGuildMessage());
        }

        public async UniTaskVoid HandleRequestChangeGuildMessage(RequestHandlerData requestHandler, RequestChangeGuildMessageMessage request, RequestProceedResultDelegate<ResponseChangeGuildMessageMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangeGuildMessageMessage()
                {
                    error = ResponseChangeGuildMessageMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildMessage(playerCharacter, request.message);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseChangeGuildMessageMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseChangeGuildMessageMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotGuildLeader:
                        error = ResponseChangeGuildMessageMessage.Error.NotAllowed;
                        break;
                    default:
                        error = ResponseChangeGuildMessageMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseChangeGuildMessageMessage()
                {
                    error = error,
                });
                return;
            }
            validateResult.Guild.guildMessage = request.message;
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendSetGuildMessageToMembers(validateResult.Guild);
            result.Invoke(AckResponseCode.Success, new ResponseChangeGuildMessageMessage());
        }

        public async UniTaskVoid HandleRequestChangeGuildRole(RequestHandlerData requestHandler, RequestChangeGuildRoleMessage request, RequestProceedResultDelegate<ResponseChangeGuildRoleMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangeGuildRoleMessage()
                {
                    error = ResponseChangeGuildRoleMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildRole(playerCharacter, request.guildRole, request.name);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseChangeGuildRoleMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseChangeGuildRoleMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotGuildLeader:
                        error = ResponseChangeGuildRoleMessage.Error.NotAllowed;
                        break;
                    default:
                        error = ResponseChangeGuildRoleMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseChangeGuildRoleMessage()
                {
                    error = error,
                });
                return;
            }
            validateResult.Guild.SetRole(request.guildRole, request.name, request.canInvite, request.canKick, request.shareExpPercentage);
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            // Change characters guild role
            IPlayerCharacterData memberCharacter;
            foreach (string memberId in validateResult.Guild.GetMemberIds())
            {
                if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(memberId, out memberCharacter))
                    memberCharacter.GuildRole = validateResult.Guild.GetMemberRole(memberCharacter.Id);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildRoleToMembers(validateResult.Guild, request.guildRole, request.name, request.canInvite, request.canKick, request.shareExpPercentage);
            result.Invoke(AckResponseCode.Success, new ResponseChangeGuildRoleMessage());
        }

        public async UniTaskVoid HandleRequestChangeMemberGuildRole(RequestHandlerData requestHandler, RequestChangeMemberGuildRoleMessage request, RequestProceedResultDelegate<ResponseChangeMemberGuildRoleMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangeMemberGuildRoleMessage()
                {
                    error = ResponseChangeMemberGuildRoleMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanChangeGuildMemberRole(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseChangeMemberGuildRoleMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseChangeMemberGuildRoleMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotGuildLeader:
                        error = ResponseChangeMemberGuildRoleMessage.Error.NotAllowed;
                        break;
                    default:
                        error = ResponseChangeMemberGuildRoleMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseChangeMemberGuildRoleMessage()
                {
                    error = error,
                });
                return;
            }
            validateResult.Guild.SetMemberRole(request.memberId, request.guildRole);
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            IPlayerCharacterData memberCharacter;
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.memberId, out memberCharacter))
                memberCharacter.GuildRole = validateResult.Guild.GetMemberRole(memberCharacter.Id);
            GameInstance.ServerGameMessageHandlers.SendSetGuildMemberRoleToMembers(validateResult.Guild, request.memberId, request.guildRole);
            result.Invoke(AckResponseCode.Success, new ResponseChangeMemberGuildRoleMessage());
        }

        public async UniTaskVoid HandleRequestIncreaseGuildSkillLevel(RequestHandlerData requestHandler, RequestIncreaseGuildSkillLevelMessage request, RequestProceedResultDelegate<ResponseIncreaseGuildSkillLevelMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseGuildSkillLevelMessage()
                {
                    error = ResponseIncreaseGuildSkillLevelMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidateGuildRequestResult validateResult = GameInstance.ServerGuildHandlers.CanIncreaseGuildSkillLevel(playerCharacter, request.dataId);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseIncreaseGuildSkillLevelMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedGuild:
                        error = ResponseIncreaseGuildSkillLevelMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotGuildLeader:
                        error = ResponseIncreaseGuildSkillLevelMessage.Error.NotAllowed;
                        break;
                    default:
                        error = ResponseIncreaseGuildSkillLevelMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseGuildSkillLevelMessage()
                {
                    error = error,
                });
                return;
            }
            validateResult.Guild.AddSkillLevel(request.dataId);
            GameInstance.ServerGuildHandlers.SetGuild(validateResult.GuildId, validateResult.Guild);
            GameInstance.ServerGameMessageHandlers.SendSetGuildSkillLevelToMembers(validateResult.Guild, request.dataId);
            GameInstance.ServerGameMessageHandlers.SendSetGuildLevelExpSkillPointToMembers(validateResult.Guild);
            result.Invoke(AckResponseCode.Success, new ResponseIncreaseGuildSkillLevelMessage());
        }
    }
}
