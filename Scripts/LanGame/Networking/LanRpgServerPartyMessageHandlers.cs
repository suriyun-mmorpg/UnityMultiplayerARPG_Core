using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class LanRpgServerPartyMessageHandlers : MonoBehaviour, IServerPartyMessageHandlers
    {
        public static int Id { get; set; }

        public async UniTaskVoid HandleRequestAcceptPartyInvitation(RequestHandlerData requestHandler, RequestAcceptPartyInvitationMessage request, RequestProceedResultDelegate<ResponseAcceptPartyInvitationMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseAcceptPartyInvitationMessage()
                {
                    error = ResponseAcceptPartyInvitationMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanAcceptPartyInvitation(request.partyId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseAcceptPartyInvitationMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotFoundParty:
                        error = ResponseAcceptPartyInvitationMessage.Error.PartyNotFound;
                        break;
                    case GameMessage.Type.NotFoundPartyInvitation:
                        error = ResponseAcceptPartyInvitationMessage.Error.InvitationNotFound;
                        break;
                    case GameMessage.Type.JoinedAnotherParty:
                        error = ResponseAcceptPartyInvitationMessage.Error.AlreadyJoined;
                        break;
                    default:
                        error = ResponseAcceptPartyInvitationMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseAcceptPartyInvitationMessage()
                {
                    error = error,
                });
                return;
            }
            playerCharacter.PartyId = request.partyId;
            validateResult.Party.AddMember(playerCharacter);
            GameInstance.ServerPartyHandlers.SetParty(request.partyId, validateResult.Party);
            GameInstance.ServerPartyHandlers.RemovePartyInvitation(request.partyId, playerCharacter.Id);
            GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.PartyInvitationAccepted);
            GameInstance.ServerGameMessageHandlers.SendSetPartyData(requestHandler.ConnectionId, validateResult.Party);
            GameInstance.ServerGameMessageHandlers.SendAddPartyMembersToOne(requestHandler.ConnectionId, validateResult.Party);
            GameInstance.ServerGameMessageHandlers.SendAddPartyMembersToMembers(validateResult.Party, playerCharacter.Id, playerCharacter.CharacterName, playerCharacter.DataId, playerCharacter.Level);
            result.Invoke(AckResponseCode.Success, new ResponseAcceptPartyInvitationMessage());
        }

        public async UniTaskVoid HandleRequestDeclinePartyInvitation(RequestHandlerData requestHandler, RequestDeclinePartyInvitationMessage request, RequestProceedResultDelegate<ResponseDeclinePartyInvitationMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseDeclinePartyInvitationMessage()
                {
                    error = ResponseDeclinePartyInvitationMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanDeclinePartyInvitation(request.partyId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseDeclinePartyInvitationMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotFoundParty:
                        error = ResponseDeclinePartyInvitationMessage.Error.PartyNotFound;
                        break;
                    case GameMessage.Type.NotFoundPartyInvitation:
                        error = ResponseDeclinePartyInvitationMessage.Error.InvitationNotFound;
                        break;
                    default:
                        error = ResponseDeclinePartyInvitationMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseDeclinePartyInvitationMessage()
                {
                    error = error,
                });
                return;
            }
            GameInstance.ServerPartyHandlers.RemovePartyInvitation(request.partyId, playerCharacter.Id);
            GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.PartyInvitationDeclined);
            result.Invoke(AckResponseCode.Success, new ResponseDeclinePartyInvitationMessage());
        }

        public async UniTaskVoid HandleRequestSendPartyInvitation(RequestHandlerData requestHandler, RequestSendPartyInvitationMessage request, RequestProceedResultDelegate<ResponseSendPartyInvitationMessage> result)
        {
            await UniTask.Yield();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseSendPartyInvitationMessage()
                {
                    error = ResponseSendPartyInvitationMessage.Error.NotLoggedIn,
                });
                return;
            }
            BasePlayerCharacterEntity inviteeCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.inviteeId, out inviteeCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSendPartyInvitationMessage()
                {
                    error = ResponseSendPartyInvitationMessage.Error.InviteeNotFound,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanSendPartyInvitation(playerCharacter, inviteeCharacter);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseSendPartyInvitationMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.CannotSendPartyInvitation:
                        error = ResponseSendPartyInvitationMessage.Error.NotAllowed;
                        break;
                    case GameMessage.Type.CharacterJoinedAnotherParty:
                        error = ResponseSendPartyInvitationMessage.Error.InviteeNotAvailable;
                        break;
                    default:
                        error = ResponseSendPartyInvitationMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseSendPartyInvitationMessage()
                {
                    error = error,
                });
                return;
            }
            GameInstance.ServerPartyHandlers.AppendPartyInvitation(playerCharacter.PartyId, request.inviteeId);
            GameInstance.ServerGameMessageHandlers.SendNotifyPartyInvitation(inviteeCharacter.ConnectionId, new PartyInvitationData()
            {
                InviterId = playerCharacter.Id,
                InviterName = playerCharacter.CharacterName,
                InviterLevel = playerCharacter.Level,
                PartyId = validateResult.PartyId,
                ShareExp = validateResult.Party.shareExp,
                ShareItem = validateResult.Party.shareItem,
            });
            result.Invoke(AckResponseCode.Success, new ResponseSendPartyInvitationMessage());
        }

        public async UniTaskVoid HandleRequestCreateParty(RequestHandlerData requestHandler, RequestCreatePartyMessage request, RequestProceedResultDelegate<ResponseCreatePartyMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseCreatePartyMessage()
                {
                    error = ResponseCreatePartyMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = playerCharacter.CanCreateParty();
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseCreatePartyMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.JoinedAnotherParty:
                        error = ResponseCreatePartyMessage.Error.AlreadyJoined;
                        break;
                    default:
                        error = ResponseCreatePartyMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseCreatePartyMessage()
                {
                    error = error,
                });
                return;
            }
            PartyData party = new PartyData(++Id, request.shareExp, request.shareItem, SocialCharacterData.Create(playerCharacter));
            GameInstance.ServerPartyHandlers.SetParty(party.id, party);
            playerCharacter.PartyId = party.id;
            GameInstance.ServerGameMessageHandlers.SendSetPartyData(requestHandler.ConnectionId, party);
            GameInstance.ServerGameMessageHandlers.SendAddPartyMembersToOne(requestHandler.ConnectionId, party);
            result.Invoke(AckResponseCode.Success, new ResponseCreatePartyMessage());
        }

        public async UniTaskVoid HandleRequestChangePartyLeader(RequestHandlerData requestHandler, RequestChangePartyLeaderMessage request, RequestProceedResultDelegate<ResponseChangePartyLeaderMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangePartyLeaderMessage()
                {
                    error = ResponseChangePartyLeaderMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanChangePartyLeader(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseChangePartyLeaderMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedParty:
                        error = ResponseChangePartyLeaderMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotPartyLeader:
                        error = ResponseChangePartyLeaderMessage.Error.NotAllowed;
                        break;
                    case GameMessage.Type.CharacterNotJoinedParty:
                        error = ResponseChangePartyLeaderMessage.Error.MemberNotFound;
                        break;
                    default:
                        error = ResponseChangePartyLeaderMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseChangePartyLeaderMessage()
                {
                    error = error,
                });
                return;
            }
            validateResult.Party.SetLeader(request.memberId);
            GameInstance.ServerPartyHandlers.SetParty(validateResult.PartyId, validateResult.Party);
            GameInstance.ServerGameMessageHandlers.SendSetPartyLeaderToMembers(validateResult.Party);
            result.Invoke(AckResponseCode.Success, new ResponseChangePartyLeaderMessage());
        }

        public async UniTaskVoid HandleRequestKickMemberFromParty(RequestHandlerData requestHandler, RequestKickMemberFromPartyMessage request, RequestProceedResultDelegate<ResponseKickMemberFromPartyMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseKickMemberFromPartyMessage()
                {
                    error = ResponseKickMemberFromPartyMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanKickMemberFromParty(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseKickMemberFromPartyMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedParty:
                        error = ResponseKickMemberFromPartyMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.CannotKickPartyLeader:
                    case GameMessage.Type.CannotKickYourSelfFromParty:
                    case GameMessage.Type.NotPartyLeader:
                        error = ResponseKickMemberFromPartyMessage.Error.NotAllowed;
                        break;
                    case GameMessage.Type.CharacterNotJoinedParty:
                        error = ResponseKickMemberFromPartyMessage.Error.MemberNotFound;
                        break;
                    default:
                        error = ResponseKickMemberFromPartyMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseKickMemberFromPartyMessage()
                {
                    error = error,
                });
                return;
            }
            IPlayerCharacterData memberCharacter;
            long memberConnectionId;
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.memberId, out memberCharacter) &&
                GameInstance.ServerUserHandlers.TryGetConnectionId(request.memberId, out memberConnectionId))
            {
                memberCharacter.ClearParty();
                GameInstance.ServerGameMessageHandlers.SendClearPartyData(memberConnectionId, validateResult.PartyId);
            }
            validateResult.Party.RemoveMember(request.memberId);
            GameInstance.ServerPartyHandlers.SetParty(validateResult.PartyId, validateResult.Party);
            GameInstance.ServerGameMessageHandlers.SendRemovePartyMemberToMembers(validateResult.Party, request.memberId);
            result.Invoke(AckResponseCode.Success, new ResponseKickMemberFromPartyMessage());
        }

        public async UniTaskVoid HandleRequestLeaveParty(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseLeavePartyMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseLeavePartyMessage()
                {
                    error = ResponseLeavePartyMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanLeaveParty(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseLeavePartyMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedParty:
                        error = ResponseLeavePartyMessage.Error.NotJoined;
                        break;
                    default:
                        error = ResponseLeavePartyMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseLeavePartyMessage()
                {
                    error = error,
                });
                return;
            }
            if (validateResult.Party.IsLeader(playerCharacter.Id))
            {
                IPlayerCharacterData memberCharacter;
                long memberConnectionId;
                foreach (string memberId in validateResult.Party.GetMemberIds())
                {
                    if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(memberId, out memberCharacter) &&
                        GameInstance.ServerUserHandlers.TryGetConnectionId(memberId, out memberConnectionId))
                    {
                        memberCharacter.ClearParty();
                        GameInstance.ServerGameMessageHandlers.SendClearPartyData(memberConnectionId, validateResult.PartyId);
                    }
                }
                GameInstance.ServerPartyHandlers.RemoveParty(validateResult.PartyId);
            }
            else
            {
                playerCharacter.ClearParty();
                validateResult.Party.RemoveMember(playerCharacter.Id);
                GameInstance.ServerPartyHandlers.SetParty(validateResult.PartyId, validateResult.Party);
                GameInstance.ServerGameMessageHandlers.SendRemovePartyMemberToMembers(validateResult.Party, playerCharacter.Id);
                GameInstance.ServerGameMessageHandlers.SendClearPartyData(requestHandler.ConnectionId, validateResult.PartyId);
            }
            result.Invoke(AckResponseCode.Success, new ResponseLeavePartyMessage());
        }

        public async UniTaskVoid HandleRequestChangePartySetting(RequestHandlerData requestHandler, RequestChangePartySettingMessage request, RequestProceedResultDelegate<ResponseChangePartySettingMessage> result)
        {
            await UniTask.Yield();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseChangePartySettingMessage()
                {
                    error = ResponseChangePartySettingMessage.Error.NotLoggedIn,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanChangePartySetting(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, validateResult.GameMessageType);
                ResponseChangePartySettingMessage.Error error;
                switch (validateResult.GameMessageType)
                {
                    case GameMessage.Type.NotJoinedParty:
                        error = ResponseChangePartySettingMessage.Error.NotJoined;
                        break;
                    case GameMessage.Type.NotPartyLeader:
                        error = ResponseChangePartySettingMessage.Error.NotAllowed;
                        break;
                    default:
                        error = ResponseChangePartySettingMessage.Error.NotAvailable;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseChangePartySettingMessage()
                {
                    error = error,
                });
                return;
            }
            validateResult.Party.Setting(request.shareExp, request.shareItem);
            GameInstance.ServerPartyHandlers.SetParty(validateResult.PartyId, validateResult.Party);
            GameInstance.ServerGameMessageHandlers.SendSetPartySettingToMembers(validateResult.Party);
            result.Invoke(AckResponseCode.Success, new ResponseChangePartySettingMessage());
        }
    }
}
