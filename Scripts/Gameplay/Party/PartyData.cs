using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class PartyData
    {
        public int id { get; private set; }
        public bool shareExp { get; private set; }
        public bool shareItem { get; private set; }
        private string leaderId;
        private Dictionary<string, PartyMemberData> members;
        private PartyMemberData tempMemberData;

        public PartyData(int id, bool shareExp, bool shareItem, BasePlayerCharacterEntity leaderCharacterEntity)
        {
            this.id = id;
            this.shareExp = shareExp;
            this.shareItem = shareItem;
            members = new Dictionary<string, PartyMemberData>();
            members.Add(leaderCharacterEntity.Id, CreatePartyMemberData(leaderCharacterEntity, true));
            leaderId = leaderCharacterEntity.Id;
        }

        public PartyMemberData CreatePartyMemberData(BasePlayerCharacterEntity playerCharacterEntity, bool isLeader)
        {
            tempMemberData = new PartyMemberData();
            tempMemberData.id = playerCharacterEntity.Id;
            tempMemberData.characterName = playerCharacterEntity.CharacterName;
            tempMemberData.dataId = playerCharacterEntity.DataId;
            tempMemberData.level = playerCharacterEntity.Level;
            tempMemberData.isLeader = isLeader;
            tempMemberData.isVisible = true;
            tempMemberData.currentHp = playerCharacterEntity.CurrentHp;
            tempMemberData.maxHp = playerCharacterEntity.CacheMaxHp;
            tempMemberData.currentMp = playerCharacterEntity.CurrentMp;
            tempMemberData.maxMp = playerCharacterEntity.CacheMaxMp;
            return tempMemberData;
        }

        public void SetMember(BasePlayerCharacterEntity playerCharacterEntity)
        {
            SetMember(playerCharacterEntity, IsLeader(playerCharacterEntity));
        }

        public void SetMember(BasePlayerCharacterEntity playerCharacterEntity, bool isLeader)
        {
            members[playerCharacterEntity.Id] = CreatePartyMemberData(playerCharacterEntity, isLeader);
        }

        public void SetMemberInvisible(string characterId)
        {
            PartyMemberData member;
            if (members.TryGetValue(characterId, out member))
            {
                member.isVisible = false;
                members[characterId] = member;
            }
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
                members.Add(playerCharacterEntity.Id, CreatePartyMemberData(playerCharacterEntity, false));
                return true;
            }
            return false;
        }

        public bool RemoveMember(string characterId)
        {
            return members.Remove(characterId);
        }

        public bool IsMember(string characterId)
        {
            return members.ContainsKey(characterId);
        }

        public int CountMember()
        {
            return members.Count;
        }

        public IEnumerable<string> GetMemberIds()
        {
            return members.Keys;
        }

        public IEnumerable<PartyMemberData> GetMembers()
        {
            return members.Values;
        }

        public PartyMemberData GetMemberById(string characterId)
        {
            return members[characterId];
        }
    }
}
