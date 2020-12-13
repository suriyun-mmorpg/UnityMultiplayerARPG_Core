using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class NetworkingPartyUtils
    {
        public static async UniTask<ValidatePartyRequestResult> CanCreateParty(this IPlayerCharacterData playerCharacter)
        {
            await UniTask.Yield();
            GameMessage.Type gameMessageType;
            if (playerCharacter.PartyId > 0)
            {
                gameMessageType = GameMessage.Type.JoinedAnotherParty;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidatePartyRequestResult(true, gameMessageType);
        }

        public static async UniTask<ValidatePartyRequestResult> CanChangePartyLeader(this IServerPartyHandlers serverPartyHandlers, IPlayerCharacterData playerCharacter, string memberId)
        {
            GameMessage.Type gameMessageType;
            int partyId = playerCharacter.PartyId;
            PartyData party;
            if (partyId <= 0 || !await serverPartyHandlers.TryGetParty(partyId, out party))
            {
                gameMessageType = GameMessage.Type.NotJoinedParty;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            if (!party.IsLeader(playerCharacter))
            {
                gameMessageType = GameMessage.Type.NotPartyLeader;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            if (!party.ContainsMemberId(memberId))
            {
                gameMessageType = GameMessage.Type.CharacterNotJoinedParty;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidatePartyRequestResult(true, gameMessageType, partyId, party);
        }

        public static async UniTask<ValidatePartyRequestResult> CanChangePartySetting(this IServerPartyHandlers serverPartyHandlers, IPlayerCharacterData playerCharacter)
        {
            GameMessage.Type gameMessageType;
            int partyId = playerCharacter.PartyId;
            PartyData party;
            if (partyId <= 0 || !await serverPartyHandlers.TryGetParty(partyId, out party))
            {
                gameMessageType = GameMessage.Type.NotJoinedParty;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            if (!party.IsLeader(playerCharacter))
            {
                gameMessageType = GameMessage.Type.NotPartyLeader;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidatePartyRequestResult(true, gameMessageType, partyId, party);
        }

        public static async UniTask<ValidatePartyRequestResult> CanSendPartyInvitation(this IServerPartyHandlers serverPartyHandlers, IPlayerCharacterData inviterCharacter, IPlayerCharacterData inviteeCharacter)
        {
            GameMessage.Type gameMessageType;
            int partyId = inviterCharacter.PartyId;
            PartyData party;
            if (partyId <= 0 || !await serverPartyHandlers.TryGetParty(partyId, out party))
            {
                gameMessageType = GameMessage.Type.NotJoinedParty;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            if (!party.CanInvite(inviterCharacter.Id))
            {
                gameMessageType = GameMessage.Type.CannotSendPartyInvitation;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            if (inviteeCharacter.PartyId > 0)
            {
                gameMessageType = GameMessage.Type.CharacterJoinedAnotherParty;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidatePartyRequestResult(true, gameMessageType, partyId, party);
        }

        public static async UniTask<ValidatePartyRequestResult> CanAcceptPartyInvitation(this IServerPartyHandlers serverPartyHandlers, int partyId, IPlayerCharacterData inviteeCharacter)
        {
            GameMessage.Type gameMessageType;
            PartyData party;
            if (!await serverPartyHandlers.HasInvitation(partyId, inviteeCharacter.Id))
            {
                gameMessageType = GameMessage.Type.NotFoundPartyInvitation;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            if (inviteeCharacter.PartyId > 0)
            {
                gameMessageType = GameMessage.Type.JoinedAnotherParty;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            if (partyId <= 0 || !await serverPartyHandlers.TryGetParty(partyId, out party))
            {
                gameMessageType = GameMessage.Type.NotFoundParty;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            if (party.CountMember() >= party.MaxMember())
            {
                gameMessageType = GameMessage.Type.PartyMemberReachedLimit;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidatePartyRequestResult(true, gameMessageType, partyId, party);
        }

        public static async UniTask<ValidatePartyRequestResult> CanDeclinePartyInvitation(this IServerPartyHandlers serverPartyHandlers, int partyId, IPlayerCharacterData inviteeCharacter)
        {
            GameMessage.Type gameMessageType;
            PartyData party;
            if (!await serverPartyHandlers.HasInvitation(partyId, inviteeCharacter.Id))
            {
                gameMessageType = GameMessage.Type.NotFoundPartyInvitation;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            if (partyId <= 0 || !await serverPartyHandlers.TryGetParty(partyId, out party))
            {
                gameMessageType = GameMessage.Type.NotFoundParty;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidatePartyRequestResult(true, gameMessageType, partyId, party);
        }

        public static async UniTask<ValidatePartyRequestResult> CanKickMemberFromParty(this IServerPartyHandlers serverPartyHandlers, IPlayerCharacterData playerCharacter, string memberId)
        {
            GameMessage.Type gameMessageType;
            int partyId = playerCharacter.PartyId;
            PartyData party;
            if (partyId <= 0 || !await serverPartyHandlers.TryGetParty(partyId, out party))
            {
                gameMessageType = GameMessage.Type.NotJoinedParty;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            if (party.IsLeader(memberId))
            {
                gameMessageType = GameMessage.Type.CannotKickPartyLeader;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            if (!party.CanKick(playerCharacter.Id))
            {
                gameMessageType = GameMessage.Type.CannotKickPartyMember;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            if (playerCharacter.Id.Equals(memberId))
            {
                gameMessageType = GameMessage.Type.CannotKickYourSelfFromParty;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            if (!party.ContainsMemberId(memberId))
            {
                gameMessageType = GameMessage.Type.CharacterNotJoinedParty;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidatePartyRequestResult(true, gameMessageType, partyId, party);
        }

        public static async UniTask<ValidatePartyRequestResult> CanLeaveParty(this IServerPartyHandlers serverPartyHandlers, IPlayerCharacterData playerCharacter)
        {
            GameMessage.Type gameMessageType;
            int partyId = playerCharacter.PartyId;
            PartyData party;
            if (partyId <= 0 || !await serverPartyHandlers.TryGetParty(partyId, out party))
            {
                gameMessageType = GameMessage.Type.NotJoinedParty;
                return new ValidatePartyRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidatePartyRequestResult(true, gameMessageType, partyId, party);
        }
    }
}
