using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class LanRpgServerPartyMessageHandlers : MonoBehaviour, IServerPartyMessageHandlers
    {
        public static int Id { get; set; }

        public async UniTaskVoid HandleRequestAcceptPartyInvitation(RequestHandlerData requestHandler, RequestAcceptPartyInvitationMessage request, RequestProceedResultDelegate<ResponseAcceptPartyInvitationMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out BasePlayerCharacterEntity playerCharacter))
            {
                result.InvokeError(new ResponseAcceptPartyInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanAcceptPartyInvitation(request.partyId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseAcceptPartyInvitationMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            playerCharacter.PartyId = request.partyId;
            validateResult.Party.AddMember(playerCharacter);
            GameInstance.ServerPartyHandlers.SetParty(request.partyId, validateResult.Party);
            GameInstance.ServerPartyHandlers.RemovePartyInvitation(request.partyId, playerCharacter.Id);
            GameInstance.ServerGameMessageHandlers.SendSetFullPartyData(requestHandler.ConnectionId, validateResult.Party);
            GameInstance.ServerGameMessageHandlers.SendAddPartyMemberToMembers(validateResult.Party, SocialCharacterData.Create(playerCharacter));
            // Send message to inviter
            GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(request.inviterId, UITextKeys.UI_PARTY_INVITATION_ACCEPTED);
            // Response to invitee
            result.InvokeSuccess(new ResponseAcceptPartyInvitationMessage()
            {
                message = UITextKeys.UI_PARTY_INVITATION_ACCEPTED,
            });
        }

        public async UniTaskVoid HandleRequestDeclinePartyInvitation(RequestHandlerData requestHandler, RequestDeclinePartyInvitationMessage request, RequestProceedResultDelegate<ResponseDeclinePartyInvitationMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out BasePlayerCharacterEntity playerCharacter))
            {
                result.InvokeError(new ResponseDeclinePartyInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanDeclinePartyInvitation(request.partyId, playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseDeclinePartyInvitationMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            GameInstance.ServerPartyHandlers.RemovePartyInvitation(request.partyId, playerCharacter.Id);
            // Send message to inviter
            GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(request.inviterId, UITextKeys.UI_PARTY_INVITATION_DECLINED);
            // Response to invitee
            result.InvokeSuccess(new ResponseDeclinePartyInvitationMessage()
            {
                message = UITextKeys.UI_PARTY_INVITATION_DECLINED,
            });
        }

        public async UniTaskVoid HandleRequestSendPartyInvitation(RequestHandlerData requestHandler, RequestSendPartyInvitationMessage request, RequestProceedResultDelegate<ResponseSendPartyInvitationMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out BasePlayerCharacterEntity playerCharacter))
            {
                result.InvokeError(new ResponseSendPartyInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.inviteeId, out BasePlayerCharacterEntity inviteeCharacter))
            {
                result.InvokeError(new ResponseSendPartyInvitationMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanSendPartyInvitation(playerCharacter, inviteeCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseSendPartyInvitationMessage()
                {
                    message = validateResult.GameMessage,
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
            result.InvokeSuccess(new ResponseSendPartyInvitationMessage());
        }

        public async UniTaskVoid HandleRequestCreateParty(RequestHandlerData requestHandler, RequestCreatePartyMessage request, RequestProceedResultDelegate<ResponseCreatePartyMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseCreatePartyMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = playerCharacter.CanCreateParty();
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseCreatePartyMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            PartyData party = new PartyData(++Id, request.shareExp, request.shareItem, SocialCharacterData.Create(playerCharacter));
            GameInstance.ServerPartyHandlers.SetParty(party.id, party);
            playerCharacter.PartyId = party.id;
            GameInstance.ServerGameMessageHandlers.SendSetFullPartyData(requestHandler.ConnectionId, party);
            result.InvokeSuccess(new ResponseCreatePartyMessage());
        }

        public async UniTaskVoid HandleRequestChangePartyLeader(RequestHandlerData requestHandler, RequestChangePartyLeaderMessage request, RequestProceedResultDelegate<ResponseChangePartyLeaderMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangePartyLeaderMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanChangePartyLeader(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangePartyLeaderMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            validateResult.Party.SetLeader(request.memberId);
            GameInstance.ServerPartyHandlers.SetParty(validateResult.PartyId, validateResult.Party);
            GameInstance.ServerGameMessageHandlers.SendSetPartyLeaderToMembers(validateResult.Party);
            result.InvokeSuccess(new ResponseChangePartyLeaderMessage());
        }

        public async UniTaskVoid HandleRequestKickMemberFromParty(RequestHandlerData requestHandler, RequestKickMemberFromPartyMessage request, RequestProceedResultDelegate<ResponseKickMemberFromPartyMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseKickMemberFromPartyMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanKickMemberFromParty(playerCharacter, request.memberId);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseKickMemberFromPartyMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.memberId, out IPlayerCharacterData memberCharacter) &&
                GameInstance.ServerUserHandlers.TryGetConnectionId(request.memberId, out long memberConnectionId))
            {
                memberCharacter.ClearParty();
                GameInstance.ServerGameMessageHandlers.SendClearPartyData(memberConnectionId, validateResult.PartyId);
            }
            validateResult.Party.RemoveMember(request.memberId);
            GameInstance.ServerPartyHandlers.SetParty(validateResult.PartyId, validateResult.Party);
            GameInstance.ServerGameMessageHandlers.SendRemovePartyMemberToMembers(validateResult.Party, request.memberId);
            result.InvokeSuccess(new ResponseKickMemberFromPartyMessage());
        }

        public async UniTaskVoid HandleRequestLeaveParty(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseLeavePartyMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseLeavePartyMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanLeaveParty(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseLeavePartyMessage()
                {
                    message = validateResult.GameMessage,
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
            result.InvokeSuccess(new ResponseLeavePartyMessage());
        }

        public async UniTaskVoid HandleRequestChangePartySetting(RequestHandlerData requestHandler, RequestChangePartySettingMessage request, RequestProceedResultDelegate<ResponseChangePartySettingMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseChangePartySettingMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ValidatePartyRequestResult validateResult = GameInstance.ServerPartyHandlers.CanChangePartySetting(playerCharacter);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseChangePartySettingMessage()
                {
                    message = validateResult.GameMessage,
                });
                return;
            }
            validateResult.Party.Setting(request.shareExp, request.shareItem);
            GameInstance.ServerPartyHandlers.SetParty(validateResult.PartyId, validateResult.Party);
            GameInstance.ServerGameMessageHandlers.SendSetPartySettingToMembers(validateResult.Party);
            result.InvokeSuccess(new ResponseChangePartySettingMessage());
        }
    }
}
