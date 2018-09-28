using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Social System Setting", menuName = "Create GameData/Social System Setting")]
    public partial class SocialSystemSetting : ScriptableObject
    {
        [Header("Party Configs")]
        public int maxPartyMember = 8;
        public bool partyMemberCanInvite = false;
        public bool partyMemberCanKick = false;

        [Header("Guild Configs")]
        public int maxGuildMember = 50;
        [Tooltip("Member roles from high to low priority")]
        public GuildMemberRole[] guildMemberRoles = new GuildMemberRole[] {
            new GuildMemberRole() { name = "Master", canInvite = true, canKick = true },
            new GuildMemberRole() { name = "Member", canInvite = false, canKick = false }
        };
    }

    [System.Serializable]
    public struct GuildMemberRole
    {
        public string name;
        public bool canInvite;
        public bool canKick;
    }
}
