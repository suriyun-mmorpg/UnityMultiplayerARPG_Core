using System.Collections.Generic;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public sealed class GuildData : SocialGroupData, INetSerializable
    {
        public const byte LeaderRole = 0;

        public string guildName;
        public int level;
        public int exp;
        public int skillPoint;
        public string guildMessage;
        public string guildMessage2;
        public int gold;
        public int score;
        public string options;
        public bool autoAcceptRequests;
        public int rank;
        private List<GuildRoleData> roles;
        private Dictionary<string, byte> memberRoles;
        private Dictionary<int, int> skillLevels;
        private bool isCached;

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

        public byte LowestMemberRole
        {
            get
            {
                if (roles == null || roles.Count < 2)
                    return 1;
                return (byte)(roles.Count - 1);
            }
        }

        public GuildData()
            : base()
        {
            level = 1;
            exp = 0;
            skillPoint = 0;
            guildMessage = string.Empty;
            guildMessage2 = string.Empty;
            score = 0;
            gold = 0;
            options = string.Empty;
            autoAcceptRequests = false;
            rank = 0;
            memberRoles = new Dictionary<string, byte>();
            skillLevels = new Dictionary<int, int>();
        }

        public GuildData(int id)
            : this()
        {
            this.id = id;
        }

        public GuildData(int id, string leaderId)
            : this(id)
        {
            this.leaderId = leaderId;
            AddMember(new SocialCharacterData() { id = leaderId });
        }

        public GuildData(int id, string guildName, string leaderId, GuildRoleData[] roles)
            : this(id, leaderId)
        {
            this.guildName = guildName;
            this.roles = new List<GuildRoleData>(roles);
        }

        public GuildData(int id, string guildName, string leaderId)
            : this(id, guildName, leaderId, SystemSetting.GuildMemberRoles)
        {

        }

        public GuildData(int id, string guildName, SocialCharacterData leaderCharacterEntity)
            : this(id, guildName, leaderCharacterEntity.id)
        {
            AddMember(leaderCharacterEntity);
        }

        public void AddMember(BasePlayerCharacterEntity playerCharacterEntity, byte guildRole)
        {
            AddMember(CreateMemberData(playerCharacterEntity), guildRole);
        }

        public void AddMember(SocialCharacterData memberData, byte guildRole)
        {
            base.AddMember(memberData);
            SetMemberRole(memberData.id, guildRole);
        }

        public void UpdateMember(SocialCharacterData memberData, byte guildRole)
        {
            base.UpdateMember(memberData);
            SetMemberRole(memberData.id, guildRole);
        }

        public override void AddMember(SocialCharacterData memberData)
        {
            base.AddMember(memberData);
            SetMemberRole(memberData.id, IsLeader(memberData.id) ? LeaderRole : LowestMemberRole);
        }

        public override bool RemoveMember(string characterId)
        {
            memberRoles.Remove(characterId);
            return base.RemoveMember(characterId);
        }

        public override void ClearMembers()
        {
            memberRoles.Clear();
            base.ClearMembers();
        }

        public override void SetLeader(string characterId)
        {
            if (members.ContainsKey(characterId))
            {
                memberRoles[leaderId] = LowestMemberRole;
                leaderId = characterId;
                memberRoles[leaderId] = LeaderRole;
            }
        }

        public bool TryGetMemberRole(string characterId, out byte role)
        {
            return memberRoles.TryGetValue(characterId, out role);
        }

        public void SetMemberRole(string characterId, byte guildRole)
        {
            if (members.ContainsKey(characterId))
            {
                if (!IsRoleAvailable(guildRole))
                    guildRole = IsLeader(characterId) ? LeaderRole : LowestMemberRole;
                // Validate role
                if (guildRole == LeaderRole && !IsLeader(characterId))
                    memberRoles[characterId] = LowestMemberRole;
                else
                    memberRoles[characterId] = guildRole;
            }
        }

        public bool IsRoleAvailable(byte guildRole)
        {
            return roles != null && guildRole >= 0 && guildRole < roles.Count;
        }

        public List<GuildRoleData> GetRoles()
        {
            return roles;
        }

        public GuildRoleData GetRole(byte guildRole)
        {
            if (!IsRoleAvailable(guildRole))
            {
                if (guildRole == LeaderRole)
                    return new GuildRoleData() { roleName = "Master", canInvite = true, canKick = true };
                else
                    return new GuildRoleData() { roleName = "Member", canInvite = false, canKick = false };
            }
            return roles[guildRole];
        }

        public void SetRole(byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            if (shareExpPercentage > SystemSetting.MaxShareExpPercentage)
                shareExpPercentage = SystemSetting.MaxShareExpPercentage;

            SetRole(guildRole, new GuildRoleData()
            {
                roleName = roleName,
                canInvite = canInvite,
                canKick = canKick,
                shareExpPercentage = shareExpPercentage,
            });
        }

        public void SetRole(byte guildRole, GuildRoleData role)
        {
            if (guildRole >= 0 && guildRole < roles.Count)
                roles[guildRole] = role;
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

        public byte GetMemberRole(string characterId)
        {
            byte result;
            if (memberRoles.ContainsKey(characterId))
            {
                result = memberRoles[characterId];
                // Validate member role
                if (result == LeaderRole && !IsLeader(characterId))
                    result = memberRoles[characterId] = LowestMemberRole;
            }
            else
            {
                result = IsLeader(characterId) ? LeaderRole : LowestMemberRole;
            }
            return result;
        }

        public IEnumerable<KeyValuePair<int, int>> GetSkillLevels()
        {
            return skillLevels;
        }

        public int GetSkillLevel(int dataId)
        {
            if (GameInstance.GuildSkills.ContainsKey(dataId) && skillLevels.ContainsKey(dataId))
                return skillLevels[dataId];
            return 0;
        }

        public bool IsSkillReachedMaxLevel(int dataId)
        {
            if (GameInstance.GuildSkills.ContainsKey(dataId) && skillLevels.ContainsKey(dataId))
                return skillLevels[dataId] >= GameInstance.GuildSkills[dataId].maxLevel;
            return false;
        }

        public void AddSkillLevel(int dataId)
        {
            if (GameInstance.GuildSkills.ContainsKey(dataId))
            {
                int level = (skillLevels.ContainsKey(dataId) ? skillLevels[dataId] : 0);
                level += 1;
                skillPoint -= 1;
                skillLevels[dataId] = level;
                isCached = false;
            }
        }

        public void SetSkillLevel(int dataId, int level)
        {
            if (GameInstance.GuildSkills.ContainsKey(dataId))
            {
                skillLevels[dataId] = level;
                isCached = false;
            }
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
