using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        public const int PartyInvitationDuration = 10000;
        public static readonly ConcurrentDictionary<int, PartyData> Parties = new ConcurrentDictionary<int, PartyData>();
        public static readonly ConcurrentDictionary<long, PartyData> UpdatingPartyMembers = new ConcurrentDictionary<long, PartyData>();
        public static readonly ConcurrentDictionary<string, int> PartyInvitations = new ConcurrentDictionary<string, int>();

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
            return PartyInvitations.ContainsKey(GetPartyInvitationId(partyId, characterId));
        }

        public void AppendPartyInvitation(int partyId, string characterId)
        {
            RemovePartyInvitation(partyId, characterId);
            PartyInvitations.TryAdd(GetPartyInvitationId(partyId, characterId), partyId);
            DelayRemovePartyInvitation(partyId, characterId).Forget();
        }

        public void RemovePartyInvitation(int partyId, string characterId)
        {
            PartyInvitations.TryRemove(GetPartyInvitationId(partyId, characterId), out _);
        }

        public void ClearParty()
        {
            Parties.Clear();
            UpdatingPartyMembers.Clear();
            PartyInvitations.Clear();
        }

        private string GetPartyInvitationId(int partyId, string characterId)
        {
            return $"{partyId}_{characterId}";
        }

        private async UniTaskVoid DelayRemovePartyInvitation(int partyId, string characterId)
        {
            await UniTask.Delay(PartyInvitationDuration);
            RemovePartyInvitation(partyId, characterId);
        }
    }
}
