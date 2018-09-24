using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class SocialGroupData
    {
        protected Dictionary<string, SocialCharacterData> members;
        protected Dictionary<string, float> lastOnlineTimes;
        protected SocialCharacterData tempMemberData;

        public SocialGroupData()
        {
            members = new Dictionary<string, SocialCharacterData>();
            lastOnlineTimes = new Dictionary<string, float>();
        }

        public void NotifyMemberOnline(string characterId, float time)
        {
            if (members.ContainsKey(characterId))
                lastOnlineTimes[characterId] = time;
        }

        public void UpdateMemberOnline(string characterId, float time)
        {
            SocialCharacterData member;
            float lastOnlineTime;
            member.isOnline = members.TryGetValue(characterId, out member) &&
                lastOnlineTimes.TryGetValue(characterId, out lastOnlineTime) &&
                time - lastOnlineTime <= 2f;
            members[characterId] = member;
        }

        public SocialCharacterData CreateMemberData(BasePlayerCharacterEntity playerCharacterEntity)
        {
            tempMemberData = new SocialCharacterData();
            tempMemberData.id = playerCharacterEntity.Id;
            tempMemberData.characterName = playerCharacterEntity.CharacterName;
            tempMemberData.dataId = playerCharacterEntity.DataId;
            tempMemberData.level = playerCharacterEntity.Level;
            tempMemberData.isOnline = true;
            tempMemberData.currentHp = playerCharacterEntity.CurrentHp;
            tempMemberData.maxHp = playerCharacterEntity.CacheMaxHp;
            tempMemberData.currentMp = playerCharacterEntity.CurrentMp;
            tempMemberData.maxMp = playerCharacterEntity.CacheMaxMp;
            return tempMemberData;
        }

        public void AddMember(BasePlayerCharacterEntity playerCharacterEntity)
        {
            AddMember(CreateMemberData(playerCharacterEntity));
        }

        public void AddMember(SocialCharacterData partyMemberData)
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
            UpdateMember(CreateMemberData(playerCharacterEntity));
        }

        public void UpdateMember(SocialCharacterData partyMemberData)
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

        public IEnumerable<SocialCharacterData> GetMembers()
        {
            return members.Values;
        }

        public SocialCharacterData GetMemberById(string characterId)
        {
            return members[characterId];
        }
    }
}
