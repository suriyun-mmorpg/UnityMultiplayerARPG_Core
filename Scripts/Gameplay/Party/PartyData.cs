using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class PartyData
    {
        public int id { get; private set; }
        public bool shareExp { get; private set; }
        public bool shareItem { get; private set; }
        private PartyMemberData tempMemberData;
        private string leaderId;
        private Dictionary<string, PartyMemberData> members;

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
            tempMemberData.currentHp = playerCharacterEntity.CurrentHp;
            tempMemberData.maxHp = playerCharacterEntity.CacheMaxHp;
            tempMemberData.currentMp = playerCharacterEntity.CurrentMp;
            tempMemberData.maxMp = playerCharacterEntity.CacheMaxMp;
            tempMemberData.isLeader = isLeader;
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
