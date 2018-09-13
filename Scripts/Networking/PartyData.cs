using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class PartyData
    {
        public int id { get; private set; }
        public bool shareExp { get; private set; }
        public bool shareItem { get; private set; }
        private string leaderId;
        private Dictionary<string, string> members;

        public PartyData(int id, bool shareExp, bool shareItem, BasePlayerCharacterEntity leaderCharacterEntity)
        {
            this.id = id;
            this.shareExp = shareExp;
            this.shareItem = shareItem;
            members = new Dictionary<string, string>();
            members.Add(leaderCharacterEntity.Id, leaderCharacterEntity.CharacterName);
            leaderId = leaderCharacterEntity.Id;
        }

        public void Setting(bool shareExp, bool shareItem)
        {
            this.shareExp = shareExp;
            this.shareItem = shareItem;
        }

        public bool IsLeader(BasePlayerCharacterEntity playerCharacterEntity)
        {
            return playerCharacterEntity.Equals(leaderId);
        }

        public bool AddMember(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (!members.ContainsKey(playerCharacterEntity.Id))
            {
                members.Add(playerCharacterEntity.Id, playerCharacterEntity.CharacterName);
                return true;
            }
            return false;
        }

        public bool RemoveMember(string characterId)
        {
            return members.Remove(characterId);
        }

        public int CountMember()
        {
            return members.Count;
        }

        public IEnumerable<string> GetMemberIds()
        {
            return members.Keys;
        }

        public IEnumerable<string> GetMemberNames()
        {
            return members.Values;
        }

        public string GetMemberNameById(string characterId)
        {
            return members[characterId];
        }
    }
}
