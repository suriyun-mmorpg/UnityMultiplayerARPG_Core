using System.Collections.Concurrent;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        public static readonly ConcurrentDictionary<int, PartyData> Parties = new ConcurrentDictionary<int, PartyData>();
        public static readonly ConcurrentDictionary<long, PartyData> UpdatingPartyMembers = new ConcurrentDictionary<long, PartyData>();

        public bool TryGetParty(int partyId, out PartyData partyData)
        {
            return Parties.TryGetValue(partyId, out partyData);
        }

        public void SetParty(int partyId, PartyData partyData)
        {
            if (Parties.ContainsKey(partyId))
                Parties[partyId] = partyData;
            else
                Parties.TryAdd(partyId, partyData);
        }

        public void RemoveParty(int partyId)
        {
            Parties.TryRemove(partyId, out _);
        }

        public bool HasPartyInvitation(int partyId, string characterId)
        {
            throw new System.NotImplementedException();
        }

        public void AppendPartyInvitation(int partyId, string characterId)
        {
            throw new System.NotImplementedException();
        }

        public void RemovePartyInvitation(int partyId, string characterId)
        {
            throw new System.NotImplementedException();
        }

        public void ClearParty()
        {
            Parties.Clear();
            UpdatingPartyMembers.Clear();
        }
    }
}
