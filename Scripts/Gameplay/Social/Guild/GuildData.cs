using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class GuildData : SocialGroupData
    {
        public const byte LeaderRole = 0;

        public string guildName { get; private set; }
        public int level;
        public int exp;
        public int skillPoint;
        public string guildMessage;
        protected List<GuildRole> roles;
        protected Dictionary<string, byte> memberRoles;

        public byte LowestMemberRole
        {
            get
            {
                if (roles == null || roles.Count < 2)
                    return 1;
                return (byte)(roles.Count - 1);
            }
        }

        public GuildData(int id, string guildName, string leaderId)
            : base(id)
        {
            this.guildName = guildName;
            level = 1;
            exp = 0;
            skillPoint = 0;
            guildMessage = string.Empty;
            roles = new List<GuildRole>(SystemSetting.GuildMemberRoles);
            memberRoles = new Dictionary<string, byte>();
            this.leaderId = leaderId;
            AddMember(new SocialCharacterData() { id = leaderId });
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

        public List<GuildRole> GetRoles()
        {
            return roles;
        }

        public GuildRole GetRole(byte guildRole)
        {
            if (!IsRoleAvailable(guildRole))
            {
                if (guildRole == LeaderRole)
                    return new GuildRole() { roleName = "Master", canInvite = true, canKick = true };
                else
                    return new GuildRole() { roleName = "Member", canInvite = false, canKick = false };
            }
            return roles[guildRole];
        }

        public void SetRole(byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            SetRole(guildRole, new GuildRole()
            {
                roleName = roleName,
                canInvite = canInvite,
                canKick = canKick,
                shareExpPercentage = shareExpPercentage,
            });
        }

        public void SetRole(byte guildRole, GuildRole role)
        {
            if (guildRole >= 0 && guildRole < roles.Count)
                roles[guildRole] = role;
        }

        public void GetSortedMembers(out SocialCharacterData[] sortedMembers, out byte[] sortedMemberRoles)
        {
            var i = 0;
            var offlineMembers = new List<SocialCharacterData>();
            sortedMembers = new SocialCharacterData[members.Count];
            sortedMemberRoles = new byte[members.Count];
            sortedMembers[i] = members[leaderId];
            sortedMemberRoles[i++] = LeaderRole;
            SocialCharacterData tempMember;
            foreach (var memberId in members.Keys)
            {
                if (memberId.Equals(leaderId))
                    continue;
                tempMember = members[memberId];
                if (!tempMember.isOnline)
                {
                    offlineMembers.Add(tempMember);
                    continue;
                }
                sortedMembers[i] = tempMember;
                sortedMemberRoles[i++] = memberRoles.ContainsKey(tempMember.id) ? memberRoles[tempMember.id] : LowestMemberRole;
            }
            foreach (var offlineMember in offlineMembers)
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
    }
}
