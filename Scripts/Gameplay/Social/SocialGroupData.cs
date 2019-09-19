using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    public class SocialGroupData
    {
        protected Dictionary<string, SocialCharacterData> members;
        protected SocialCharacterData tempMemberData;

        public int id { get; protected set; }
        public string leaderId { get; protected set; }

        public static SocialSystemSetting SystemSetting { get { return GameInstance.Singleton.SocialSystemSetting; } }
        
        public SocialGroupData(int id)
        {
            this.id = id;
            members = new Dictionary<string, SocialCharacterData>();
        }

        public SocialGroupData(int id, string leaderId) : this(id)
        {
            this.leaderId = leaderId;
            AddMember(new SocialCharacterData() { id = leaderId });
        }

        public SocialCharacterData CreateMemberData(BasePlayerCharacterEntity playerCharacterEntity)
        {
            tempMemberData = new SocialCharacterData();
            tempMemberData.id = playerCharacterEntity.Id;
            tempMemberData.characterName = playerCharacterEntity.CharacterName;
            tempMemberData.dataId = playerCharacterEntity.DataId;
            tempMemberData.level = playerCharacterEntity.Level;
            tempMemberData.currentHp = playerCharacterEntity.CurrentHp;
            tempMemberData.maxHp = playerCharacterEntity.MaxHp;
            tempMemberData.currentMp = playerCharacterEntity.CurrentMp;
            tempMemberData.maxMp = playerCharacterEntity.MaxMp;
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
            SocialCharacterData oldMemberData = members[memberData.id];
            oldMemberData.characterName = memberData.characterName;
            oldMemberData.dataId = memberData.dataId;
            oldMemberData.level = memberData.level;
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
            SocialCharacterData oldMemberData = members[memberData.id];
            oldMemberData.characterName = memberData.characterName;
            oldMemberData.dataId = memberData.dataId;
            oldMemberData.level = memberData.level;
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

        public virtual void ClearMembers()
        {
            members.Clear();
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

        public string[] GetMemberIds()
        {
            return members.Keys.ToArray();
        }

        public SocialCharacterData[] GetMembers()
        {
            return members.Values.ToArray();
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

        public SocialCharacterData GetLeader()
        {
            return members[leaderId];
        }
    }
}
