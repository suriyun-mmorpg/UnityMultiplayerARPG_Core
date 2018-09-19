using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class PartyData
    {
        public int id { get; private set; }
        public bool shareExp { get; private set; }
        public bool shareItem { get; private set; }
        public string leaderId { get; private set; }
        private Dictionary<string, PartyMemberData> members;
        private PartyMemberData tempMemberData;
        
        public PartyData(int id, bool shareExp, bool shareItem, string leaderId)
        {
            this.id = id;
            this.shareExp = shareExp;
            this.shareItem = shareItem;
            this.leaderId = leaderId;
            members = new Dictionary<string, PartyMemberData>();
        }

        public PartyData(int id, bool shareExp, bool shareItem, BasePlayerCharacterEntity leaderCharacterEntity) : this(id, shareExp, shareItem, string.Empty)
        {
            AddMember(leaderCharacterEntity);
            leaderId = leaderCharacterEntity.Id;
        }

        public PartyMemberData CreatePartyMemberData(BasePlayerCharacterEntity playerCharacterEntity)
        {
            tempMemberData = new PartyMemberData();
            tempMemberData.id = playerCharacterEntity.Id;
            tempMemberData.characterName = playerCharacterEntity.CharacterName;
            tempMemberData.dataId = playerCharacterEntity.DataId;
            tempMemberData.level = playerCharacterEntity.Level;
            tempMemberData.isVisible = true;
            tempMemberData.currentHp = playerCharacterEntity.CurrentHp;
            tempMemberData.maxHp = playerCharacterEntity.CacheMaxHp;
            tempMemberData.currentMp = playerCharacterEntity.CurrentMp;
            tempMemberData.maxMp = playerCharacterEntity.CacheMaxMp;
            return tempMemberData;
        }

        public void UpdateMemberVisible(string characterId, bool isVisible)
        {
            PartyMemberData member;
            if (members.TryGetValue(characterId, out member))
            {
                member.isVisible = isVisible;
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
            return IsLeader(playerCharacterEntity.Id);
        }

        public bool IsLeader(string characterId)
        {
            return characterId.Equals(leaderId);
        }

        public void AddMember(BasePlayerCharacterEntity playerCharacterEntity)
        {
            AddMember(CreatePartyMemberData(playerCharacterEntity));
        }

        public void AddMember(PartyMemberData partyMemberData)
        {
            if (!members.ContainsKey(partyMemberData.id))
            {
                members.Add(partyMemberData.id, partyMemberData);
                return;
            }
            var oldPartyMemberData = members[partyMemberData.id];
            oldPartyMemberData.characterName = partyMemberData.characterName;
            oldPartyMemberData.dataId = partyMemberData.dataId;
            oldPartyMemberData.level = partyMemberData.level;
            members[partyMemberData.id] = oldPartyMemberData;
        }

        public void UpdateMember(BasePlayerCharacterEntity playerCharacterEntity)
        {
            UpdateMember(CreatePartyMemberData(playerCharacterEntity));
        }

        public void UpdateMember(PartyMemberData partyMemberData)
        {
            if (!members.ContainsKey(partyMemberData.id))
                return;
            var oldPartyMemberData = members[partyMemberData.id];
            oldPartyMemberData.characterName = partyMemberData.characterName;
            oldPartyMemberData.dataId = partyMemberData.dataId;
            oldPartyMemberData.level = partyMemberData.level;
            oldPartyMemberData.currentHp = partyMemberData.currentHp;
            oldPartyMemberData.maxHp = partyMemberData.maxHp;
            oldPartyMemberData.currentMp = partyMemberData.currentMp;
            oldPartyMemberData.maxMp = partyMemberData.maxMp;
            members[partyMemberData.id] = oldPartyMemberData;
        }

        public bool RemoveMember(BasePlayerCharacterEntity playerCharacterEntity)
        {
            return RemoveMember(playerCharacterEntity.Id);
        }

        public bool RemoveMember(string characterId)
        {
            return members.Remove(characterId);
        }

        public bool IsMember(BasePlayerCharacterEntity playerCharacterEntity)
        {
            return IsMember(playerCharacterEntity.Id);
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
