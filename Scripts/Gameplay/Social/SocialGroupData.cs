using System.Collections.Generic;

namespace MultiplayerARPG
{
    public abstract class SocialGroupData
    {
        protected Dictionary<string, SocialCharacterData> members;
        protected Dictionary<string, float> lastOnlineTimes;
        protected SocialCharacterData tempMemberData;

        public int id { get; private set; }
        public string leaderId { get; private set; }

        public static SocialSystemSetting SystemSetting { get { return GameInstance.Singleton.SocialSystemSetting; } }

        public SocialGroupData(int id, string leaderId)
        {
            this.id = id;
            this.leaderId = leaderId;
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
            if (members.TryGetValue(characterId, out member) &&
                lastOnlineTimes.TryGetValue(characterId, out lastOnlineTime) &&
                time - lastOnlineTime <= 2f)
            {
                // Online, add online flag
                member.memberFlags = (byte)(member.memberFlags | SocialCharacterData.FLAG_ONLINE);
            }
            else
            {
                // Offline, remove online flag
                member.memberFlags = (byte)(member.memberFlags & ~SocialCharacterData.FLAG_ONLINE);
            }
            members[characterId] = member;
        }

        public SocialCharacterData CreateMemberData(BasePlayerCharacterEntity playerCharacterEntity)
        {
            tempMemberData = new SocialCharacterData();
            tempMemberData.id = playerCharacterEntity.Id;
            tempMemberData.characterName = playerCharacterEntity.CharacterName;
            tempMemberData.dataId = playerCharacterEntity.DataId;
            tempMemberData.level = playerCharacterEntity.Level;
            tempMemberData.memberFlags = SocialCharacterData.FLAG_ONLINE;
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

        public virtual void AddMember(SocialCharacterData memberData)
        {
            if (!members.ContainsKey(memberData.id))
            {
                members.Add(memberData.id, memberData);
                return;
            }
            var oldMemberData = members[memberData.id];
            oldMemberData.characterName = memberData.characterName;
            oldMemberData.dataId = memberData.dataId;
            oldMemberData.level = memberData.level;
            oldMemberData.memberFlags = GetMemberFlags(oldMemberData);
            members[memberData.id] = oldMemberData;
        }

        public void UpdateMember(BasePlayerCharacterEntity playerCharacterEntity)
        {
            UpdateMember(CreateMemberData(playerCharacterEntity));
        }

        public virtual void UpdateMember(SocialCharacterData memberData)
        {
            if (!members.ContainsKey(memberData.id))
                return;
            var oldMemberData = members[memberData.id];
            oldMemberData.characterName = memberData.characterName;
            oldMemberData.dataId = memberData.dataId;
            oldMemberData.level = memberData.level;
            oldMemberData.memberFlags = GetMemberFlags(oldMemberData);
            oldMemberData.currentHp = memberData.currentHp;
            oldMemberData.maxHp = memberData.maxHp;
            oldMemberData.currentMp = memberData.currentMp;
            oldMemberData.maxMp = memberData.maxMp;
            members[memberData.id] = oldMemberData;
        }

        public bool RemoveMember(BasePlayerCharacterEntity playerCharacterEntity)
        {
            return RemoveMember(playerCharacterEntity.Id);
        }

        public virtual bool RemoveMember(string characterId)
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

        public bool ContainsMemberId(string characterId)
        {
            return members.ContainsKey(characterId);
        }

        public IEnumerable<string> GetMemberIds()
        {
            return members.Keys;
        }

        public IEnumerable<SocialCharacterData> GetMembers()
        {
            return members.Values;
        }

        public bool TryGetMember(string id, out SocialCharacterData result)
        {
            return members.TryGetValue(id, out result);
        }

        public bool IsLeader(BasePlayerCharacterEntity playerCharacterEntity)
        {
            return IsLeader(playerCharacterEntity.Id);
        }

        public bool IsLeader(string characterId)
        {
            return characterId.Equals(leaderId);
        }

        public virtual void SetLeader(string characterId)
        {
            if (members.ContainsKey(characterId))
                leaderId = characterId;
        }

        public abstract byte GetMemberFlags(SocialCharacterData memberData);
    }
}
