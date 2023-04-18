using System.Collections.Generic;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial class GuildData : INetSerializable
    {
        private int _increaseMaxMember;
        public int IncreaseMaxMember
        {
            get
            {
                MakeCaches();
                return _increaseMaxMember;
            }
        }

        private float _increaseExpGainPercentage;
        public float IncreaseExpGainPercentage
        {
            get
            {
                MakeCaches();
                return _increaseExpGainPercentage;
            }
        }

        private float _increaseGoldGainPercentage;
        public float IncreaseGoldGainPercentage
        {
            get
            {
                MakeCaches();
                return _increaseGoldGainPercentage;
            }
        }

        private float _increaseShareExpGainPercentage;
        public float IncreaseShareExpGainPercentage
        {
            get
            {
                MakeCaches();
                return _increaseShareExpGainPercentage;
            }
        }

        private float _increaseShareGoldGainPercentage;
        public float IncreaseShareGoldGainPercentage
        {
            get
            {
                MakeCaches();
                return _increaseShareGoldGainPercentage;
            }
        }

        private float _decreaseExpLostPercentage;
        public float DecreaseExpLostPercentage
        {
            get
            {
                MakeCaches();
                return _decreaseExpLostPercentage;
            }
        }

        public void AddMember(BasePlayerCharacterEntity playerCharacterEntity, byte guildRole)
        {
            AddMember(CreateMemberData(playerCharacterEntity), guildRole);
        }

        public void GetSortedMembers(out SocialCharacterData[] sortedMembers, out byte[] sortedMemberRoles)
        {
            int i = 0;
            sortedMembers = new SocialCharacterData[members.Count];
            sortedMemberRoles = new byte[members.Count];
            if (members.Count <= 0)
                return;
            sortedMembers[i] = members[leaderId];
            sortedMemberRoles[i++] = LeaderRole;
            List<SocialCharacterData> offlineMembers = new List<SocialCharacterData>();
            SocialCharacterData tempMember;
            foreach (string memberId in members.Keys)
            {
                if (memberId.Equals(leaderId))
                    continue;
                tempMember = members[memberId];
                if (!GameInstance.ClientOnlineCharacterHandlers.IsCharacterOnline(memberId))
                {
                    offlineMembers.Add(tempMember);
                    continue;
                }
                sortedMembers[i] = tempMember;
                sortedMemberRoles[i++] = memberRoles.ContainsKey(tempMember.id) ? memberRoles[tempMember.id] : LowestMemberRole;
            }
            foreach (SocialCharacterData offlineMember in offlineMembers)
            {
                sortedMembers[i] = offlineMember;
                sortedMemberRoles[i++] = memberRoles.ContainsKey(offlineMember.id) ? memberRoles[offlineMember.id] : LowestMemberRole;
            }
        }

        public bool IsSkillReachedMaxLevel(int dataId)
        {
            if (GameInstance.GuildSkills.ContainsKey(dataId) && skillLevels.ContainsKey(dataId))
                return skillLevels[dataId] >= GameInstance.GuildSkills[dataId].maxLevel;
            return false;
        }

        private void MakeCaches()
        {
            if (_isCached)
                return;
            _isCached = true;
            _increaseMaxMember = 0;
            _increaseExpGainPercentage = 0;
            _increaseGoldGainPercentage = 0;
            _increaseShareExpGainPercentage = 0;
            _increaseShareGoldGainPercentage = 0;
            _decreaseExpLostPercentage = 0;

            GuildSkill tempGuildSkill;
            int tempLevel;
            foreach (KeyValuePair<int, int> skill in skillLevels)
            {
                tempLevel = skill.Value;
                if (!GameInstance.GuildSkills.TryGetValue(skill.Key, out tempGuildSkill) || tempLevel <= 0)
                    continue;

                _increaseMaxMember += tempGuildSkill.GetIncreaseMaxMember(tempLevel);
                _increaseExpGainPercentage += tempGuildSkill.GetIncreaseExpGainPercentage(tempLevel);
                _increaseGoldGainPercentage += tempGuildSkill.GetIncreaseGoldGainPercentage(tempLevel);
                _increaseShareExpGainPercentage += tempGuildSkill.GetIncreaseShareExpGainPercentage(tempLevel);
                _increaseShareGoldGainPercentage += tempGuildSkill.GetIncreaseShareGoldGainPercentage(tempLevel);
                _decreaseExpLostPercentage += tempGuildSkill.GetDecreaseExpLostPercentage(tempLevel);
            }
        }

        public int MaxMember()
        {
            return SystemSetting.MaxGuildMember + IncreaseMaxMember;
        }

        public bool CanInvite(string characterId)
        {
            return GetRole(GetMemberRole(characterId)).canInvite;
        }

        public bool CanKick(string characterId)
        {
            return GetRole(GetMemberRole(characterId)).canKick;
        }

        public bool CanUseStorage(string characterId)
        {
            return GetRole(GetMemberRole(characterId)).canUseStorage;
        }

        public byte ShareExpPercentage(string characterId)
        {
            return GetRole(GetMemberRole(characterId)).shareExpPercentage;
        }

        public int GetNextLevelExp()
        {
            return GetNextLevelExp(SystemSetting.GuildExpTree, level);
        }

        public override void Serialize(NetDataWriter writer)
        {
            base.Serialize(writer);
            writer.Put(guildName);
            writer.PutPackedInt(level);
            writer.PutPackedInt(exp);
            writer.PutPackedInt(skillPoint);
            writer.Put(guildMessage);
            writer.Put(guildMessage2);
            writer.PutPackedInt(score);
            writer.PutPackedInt(gold);
            writer.Put(options);
            writer.Put(autoAcceptRequests);
            writer.PutPackedInt(rank);
            writer.PutList(roles);
            writer.PutDictionary(memberRoles);
            writer.PutDictionary(skillLevels);
        }

        public override void Deserialize(NetDataReader reader)
        {
            base.Deserialize(reader);
            guildName = reader.GetString();
            level = reader.GetPackedInt();
            exp = reader.GetPackedInt();
            skillPoint = reader.GetPackedInt();
            guildMessage = reader.GetString();
            guildMessage2 = reader.GetString();
            score = reader.GetPackedInt();
            gold = reader.GetPackedInt();
            options = reader.GetString();
            autoAcceptRequests = reader.GetBool();
            rank = reader.GetPackedInt();
            roles = reader.GetList<GuildRoleData>();
            memberRoles = reader.GetDictionary<string, byte>();
            skillLevels = reader.GetDictionary<int, int>();
        }
    }
}
