using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultServerPartyHandlers : MonoBehaviour, IServerPartyHandlers
    {
        public const int PartyInvitationDuration = 10000;
        public static readonly ConcurrentDictionary<int, PartyData> Parties = new ConcurrentDictionary<int, PartyData>();
        public static readonly ConcurrentDictionary<long, PartyData> UpdatingPartyMembers = new ConcurrentDictionary<long, PartyData>();
        public static readonly HashSet<string> PartyInvitations = new HashSet<string>();

        public int PartiesCount { get { return Parties.Count; } }

        public bool TryGetParty(int partyId, out PartyData partyData)
        {
            return Parties.TryGetValue(partyId, out partyData);
        }

        public bool ContainsParty(int partyId)
        {
            return Parties.ContainsKey(partyId);
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
            return PartyInvitations.Contains(GetPartyInvitationId(partyId, characterId));
        }

        public void AppendPartyInvitation(int partyId, string characterId)
        {
            RemovePartyInvitation(partyId, characterId);
            PartyInvitations.Add(GetPartyInvitationId(partyId, characterId));
            DelayRemovePartyInvitation(partyId, characterId).Forget();
        }

        public void RemovePartyInvitation(int partyId, string characterId)
        {
            PartyInvitations.Remove(GetPartyInvitationId(partyId, characterId));
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

        public IEnumerable<PartyData> GetParties()
        {
            return Parties.Values;
        }
    }
}