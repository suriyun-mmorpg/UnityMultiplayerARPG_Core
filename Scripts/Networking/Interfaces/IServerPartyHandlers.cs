namespace MultiplayerARPG
{
    public interface IServerPartyHandlers
    {
        bool TryGetParty(int partyId, out PartyData partyData);
        void SetParty(int partyId, PartyData partyData);
        void DeleteParty(int partyId);
        bool HasInvitation(int partyId, string characterId);
        void AppendInvitation(int partyId, string characterId);
        void DeleteInvitation(int partyId, string characterId);
    }
}
