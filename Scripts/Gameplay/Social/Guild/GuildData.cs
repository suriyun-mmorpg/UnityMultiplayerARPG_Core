using System.Collections.Generic;

namespace MultiplayerARPG
{
    public sealed class GuildData : SocialGroupData
    {
        public const byte LeaderRole = 0;

        public string guildName { get; private set; }
        public short level;
        public int exp;
        public short skillPoint;
        public string guildMessage;
        public int gold;
        private List<GuildRoleData> roles;
        private Dictionary<string, byte> memberRoles;
        private Dictionary<int, short> skillLevels;

        public int IncreaseMaxMember { get; private set; }
        public float IncreaseExpGainPercentage { get; private set; }
        public float IncreaseGoldGainPercentage { get; private set; }
        public float IncreaseShareExpGainPercentage { get; private set; }
        public float IncreaseShareGoldGainPercentage { get; private set; }
        public float DecreaseExpLostPercentage { get; private set; }

        public byte LowestMemberRole
        {
            get
            {
                if (roles == null || roles.Count < 2)
                    return 1;
                return (byte)(roles.Count - 1);
            }
        }

        public GuildData(int id, string guildName, string leaderId, GuildRoleData[] roles)
            : base(id)
        {
            this.guildName = guildName;
            level = 1;
            exp = 0;
            skillPoint = 0;
            guildMessage = string.Empty;
            this.roles = new List<GuildRoleData>(roles);
            memberRoles = new Dictionary<string, byte>();
            skillLevels = new Dictionary<int, short>();
            this.leaderId = leaderId;
            AddMember(new SocialCharacterData() { id = leaderId });
        }

        public GuildData(int id, string guildName, string leaderId)
            : this(id, guildName, leaderId, SystemSetting.GuildMemberRoles)
        {
        }

        public GuildData(int id, string guildName, BasePlayerCharacterEntity leaderCharacterEntity)
            : this(id, guildName, leaderCharacterEntity.Id)
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
                if (!BaseGameNetworkManager.IsCharacterOnline(memberId))
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

        public IEnumerable<KeyValuePair<int, short>> GetSkillLevels()
        {
            return skillLevels;
        }

        public short GetSkillLevel(int dataId)
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
                short level = (short)(skillLevels.ContainsKey(dataId) ? skillLevels[dataId] : 0);
                level += 1;
                skillPoint -= 1;
                skillLevels[dataId] = level;
                MakeCaches();
            }
        }

        public void SetSkillLevel(int dataId, short level)
        {
            if (GameInstance.GuildSkills.ContainsKey(dataId))
            {
                skillLevels[dataId] = level;
                MakeCaches();
            }
        }

        private void MakeCaches()
        {
            IncreaseMaxMember = 0;
            IncreaseExpGainPercentage = 0;
            IncreaseGoldGainPercentage = 0;
            IncreaseShareExpGainPercentage = 0;
            IncreaseShareGoldGainPercentage = 0;
            DecreaseExpLostPercentage = 0;

            GuildSkill tempGuildSkill;
            short tempLevel;
            foreach (KeyValuePair<int, short> skill in skillLevels)
            {
                tempLevel = skill.Value;
                if (!GameInstance.GuildSkills.TryGetValue(skill.Key, out tempGuildSkill) || tempLevel <= 0)
                    continue;

                IncreaseMaxMember += tempGuildSkill.GetIncreaseMaxMember(tempLevel);
                IncreaseExpGainPercentage += tempGuildSkill.GetIncreaseExpGainPercentage(tempLevel);
                IncreaseGoldGainPercentage += tempGuildSkill.GetIncreaseGoldGainPercentage(tempLevel);
                IncreaseShareExpGainPercentage += tempGuildSkill.GetIncreaseShareExpGainPercentage(tempLevel);
                IncreaseShareGoldGainPercentage += tempGuildSkill.GetIncreaseShareGoldGainPercentage(tempLevel);
                DecreaseExpLostPercentage += tempGuildSkill.GetDecreaseExpLostPercentage(tempLevel);
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
    }
}
