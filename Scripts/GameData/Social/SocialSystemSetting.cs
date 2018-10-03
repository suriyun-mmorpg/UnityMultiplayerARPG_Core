using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Social System Setting", menuName = "Create GameData/Social System Setting")]
    public partial class SocialSystemSetting : ScriptableObject
    {
        [Header("Party Configs")]
        [SerializeField]
        private int maxPartyMember = 8;
        [SerializeField]
        private bool partyMemberCanInvite = false;
        [SerializeField]
        private bool partyMemberCanKick = false;

        public int MaxPartyMember { get { return maxPartyMember; } }
        public bool PartyMemberCanInvite { get { return partyMemberCanInvite; } }
        public bool PartyMemberCanKick { get { return partyMemberCanKick; } }

        [Header("Guild Configs")]
        [SerializeField]
        private int maxGuildMember = 50;
        [Tooltip("Member roles from high to low priority")]
        [SerializeField]
        private GuildMemberRole[] guildMemberRoles = new GuildMemberRole[] {
            new GuildMemberRole() { name = "Master", canInvite = true, canKick = true },
            new GuildMemberRole() { name = "Member", canInvite = false, canKick = false }
        };

        public int MaxGuildMember { get { return maxGuildMember; } }

        public bool IsGuildMemberRoleAvailable(byte guildRole)
        {
            return guildMemberRoles != null && guildRole < guildMemberRoles.Length;
        }

        public GuildMemberRole GetGuildMemberRole(byte guildRole)
        {
            if (!IsGuildMemberRoleAvailable(guildRole))
            {
                if (guildRole == GuildLeaderRole)
                    return new GuildMemberRole() { name = "Master", canInvite = true, canKick = true };
                else
                    return new GuildMemberRole() { name = "Member", canInvite = false, canKick = false };
            }
            return guildMemberRoles[guildRole];
        }

        public byte GuildLeaderRole
        {
            get { return 0; }
        }

        public byte LowestGuildMemberRole
        {
            get
            {
                if (guildMemberRoles == null || guildMemberRoles.Length < 2)
                    return 1;
                return (byte)(guildMemberRoles.Length - 1);
            }
        }
    }

    [System.Serializable]
    public struct GuildMemberRole
    {
        public string name;
        public bool canInvite;
        public bool canKick;
    }
}
