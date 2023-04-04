using System.Collections.Generic;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial class GuildData : INetSerializable
    {
        private int increaseMaxMember;
        public int IncreaseMaxMember
        {
            get
            {
                MakeCaches();
                return increaseMaxMember;
            }
        }

        private float increaseExpGainPercentage;
        public float IncreaseExpGainPercentage
        {
            get
            {
                MakeCaches();
                return increaseExpGainPercentage;
            }
        }

        private float increaseGoldGainPercentage;
        public float IncreaseGoldGainPercentage
        {
            get
            {
                MakeCaches();
                return increaseGoldGainPercentage;
            }
        }

        private float increaseShareExpGainPercentage;
        public float IncreaseShareExpGainPercentage
        {
            get
            {
                MakeCaches();
                return increaseShareExpGainPercentage;
            }
        }

        private float increaseShareGoldGainPercentage;
        public float IncreaseShareGoldGainPercentage
        {
            get
            {
                MakeCaches();
                return increaseShareGoldGainPercentage;
            }
        }

        private float decreaseExpLostPercentage;
        public float DecreaseExpLostPercentage
        {
            get
            {
                MakeCaches();
                return decreaseExpLostPercentage;
            }
        }

        public GuildData(int id, string guildName, SocialCharacterData leaderCharacterEntity)
            : this(id, guildName, leaderCharacterEntity.id)
        {
            AddMember(leaderCharacterEntity);
        }

        public GuildData(int id, string guildName, string leaderId)
            : this(id, guildName, leaderId, SystemSetting.GuildMemberRoles)
        {

        }

        public void AddMember(BasePlayerCharacterEntity playerCharacterEntity, byte guildRole)
        {
            AddMember(CreateMemberData(playerCharacterEntity), guildRole);
        }

        public void GetSortedMembers(out SocialCharacterData[] sortedMembers, out byte[] sortedMemberRoles)
        {
            int i = 0;
            List<SocialCharacterData> offlineMembers = new List<SocialCharacterData>();
            sortedMembers = new SocialCharacterData[members.Count];
            sortedMemberRoles = new byte[members.Count];
            sortedMembers[i] = members[leaderId];
            sortedMemberRoles[i++] = LeaderRole;
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
            if (isCached)
                return;
            isCached = true;
            increaseMaxMember = 0;
            increaseExpGainPercentage = 0;
            increaseGoldGainPercentage = 0;
            increaseShareExpGainPercentage = 0;
            increaseShareGoldGainPercentage = 0;
            decreaseExpLostPercentage = 0;

            GuildSkill tempGuildSkill;
            int tempLevel;
            foreach (KeyValuePair<int, int> skill in skillLevels)
            {
                tempLevel = skill.Value;
                if (!GameInstance.GuildSkills.TryGetValue(skill.Key, out tempGuildSkill) || tempLevel <= 0)
                    continue;

                increaseMaxMember += tempGuildSkill.GetIncreaseMaxMember(tempLevel);
                increaseExpGainPercentage += tempGuildSkill.GetIncreaseExpGainPercentage(tempLevel);
                increaseGoldGainPercentage += tempGuildSkill.GetIncreaseGoldGainPercentage(tempLevel);
                increaseShareExpGainPercentage += tempGuildSkill.GetIncreaseShareExpGainPercentage(tempLevel);
                increaseShareGoldGainPercentage += tempGuildSkill.GetIncreaseShareGoldGainPercentage(tempLevel);
                decreaseExpLostPercentage += tempGuildSkill.GetDecreaseExpLostPercentage(tempLevel);
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
            return SystemSetting.GetNextLevelExp(level);
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
