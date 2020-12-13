namespace MultiplayerARPG
{
    public interface IServerPartyHandlers
    {
        PartyData GetParty(int id);
        void SetParty(int id, PartyData partyData);
    }
}
