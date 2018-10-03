using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
            new GuildMemberRole() { name = "Member 1", canInvite = false, canKick = false },
            new GuildMemberRole() { name = "Member 2", canInvite = false, canKick = false },
            new GuildMemberRole() { name = "Member 3", canInvite = false, canKick = false },
            new GuildMemberRole() { name = "Member 4", canInvite = false, canKick = false },
            new GuildMemberRole() { name = "Member 5", canInvite = false, canKick = false },
        };

        public int MaxGuildMember { get { return maxGuildMember; } }
        public GuildMemberRole[] GuildMemberRoles { get { return guildMemberRoles; } }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (guildMemberRoles.Length < 2)
            {
                Debug.LogWarning("[SocialSystemSetting] `guildMemberRoles` must more or equals to 2");
                guildMemberRoles = new GuildMemberRole[] {
                    guildMemberRoles[0],
                    new GuildMemberRole() { name = "Member 1", canInvite = false, canKick = false },
                };
                EditorUtility.SetDirty(this);
            }
            else if (guildMemberRoles.Length < 1)
            {
                Debug.LogWarning("[SocialSystemSetting] `guildMemberRoles` must more or equals to 2");
                guildMemberRoles = new GuildMemberRole[] {
                    new GuildMemberRole() { name = "Master", canInvite = true, canKick = true },
                    new GuildMemberRole() { name = "Member 1", canInvite = false, canKick = false },
                };
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }

    [System.Serializable]
    public struct GuildMemberRole
    {
        public string name;
        public bool canInvite;
        public bool canKick;
        public int shareExpPercentage;
    }
}
