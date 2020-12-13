using Cysharp.Threading.Tasks;

namespace MultiplayerARPG
{
    public interface IServerPartyHandlers
    {
        UniTask<bool> TryGetParty(int partyId, out PartyData partyData);
        UniTask SetParty(int partyId, PartyData partyData);
        UniTask DeleteParty(int partyId);
        UniTask<PartyData> CreateParty(IPlayerCharacterData playerCharacter, bool shareExp, bool shareItem);
        UniTask<bool> HasInvitation(int partyId, string characterId);
        UniTask AppendInvitation(int partyId, string characterId);
        UniTask DeleteInvitation(int partyId, string characterId);
    }
}
