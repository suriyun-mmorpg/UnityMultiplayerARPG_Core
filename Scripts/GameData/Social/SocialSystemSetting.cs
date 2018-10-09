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
        private GuildRoleData[] guildMemberRoles = new GuildRoleData[] {
            new GuildRoleData() { roleName = "Master", canInvite = true, canKick = true },
            new GuildRoleData() { roleName = "Member 1", canInvite = false, canKick = false },
            new GuildRoleData() { roleName = "Member 2", canInvite = false, canKick = false },
            new GuildRoleData() { roleName = "Member 3", canInvite = false, canKick = false },
            new GuildRoleData() { roleName = "Member 4", canInvite = false, canKick = false },
            new GuildRoleData() { roleName = "Member 5", canInvite = false, canKick = false },
        };
        [SerializeField]
        public ItemAmount[] createGuildRequireItems;
        [SerializeField]
        private int createGuildRequiredGold = 1000;

        public int MaxGuildMember { get { return maxGuildMember; } }
        public GuildRoleData[] GuildMemberRoles { get { return guildMemberRoles; } }
        private Dictionary<Item, short> cacheCreateGuildRequireItems;
        public Dictionary<Item, short> CreateGuildRequireItems
        {
            get
            {
                if (cacheCreateGuildRequireItems == null)
                    cacheCreateGuildRequireItems = GameDataHelpers.MakeItemAmountsDictionary(createGuildRequireItems, new Dictionary<Item, short>());
                return cacheCreateGuildRequireItems;
            }
        }
        public int CreateGuildRequiredGold { get { return createGuildRequiredGold; } }

        public bool CanCreateGuild(IPlayerCharacterData character)
        {
            if (character.Gold < createGuildRequiredGold)
                return false;
            if (createGuildRequireItems == null || createGuildRequireItems.Length == 0)
                return true;
            foreach (var requireItem in createGuildRequireItems)
            {
                if (requireItem.item != null && character.CountNonEquipItems(requireItem.item.DataId) < requireItem.amount)
                    return false;
            }
            return true;
        }

        public void ReduceCreateGuildResource(IPlayerCharacterData character)
        {
            foreach (var requireItem in createGuildRequireItems)
            {
                if (requireItem.item != null && requireItem.amount > 0)
                    character.DecreaseItems(requireItem.item.DataId, requireItem.amount);
            }
            character.Gold -= createGuildRequiredGold;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (guildMemberRoles.Length < 2)
            {
                Debug.LogWarning("[SocialSystemSetting] `guildMemberRoles` must more or equals to 2");
                guildMemberRoles = new GuildRoleData[] {
                    guildMemberRoles[0],
                    new GuildRoleData() { roleName = "Member 1", canInvite = false, canKick = false },
                };
                EditorUtility.SetDirty(this);
            }
            else if (guildMemberRoles.Length < 1)
            {
                Debug.LogWarning("[SocialSystemSetting] `guildMemberRoles` must more or equals to 2");
                guildMemberRoles = new GuildRoleData[] {
                    new GuildRoleData() { roleName = "Master", canInvite = true, canKick = true },
                    new GuildRoleData() { roleName = "Member 1", canInvite = false, canKick = false },
                };
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }

    [System.Serializable]
    public struct GuildRoleData
    {
        public string roleName;
        public bool canInvite;
        public bool canKick;
        public byte shareExpPercentage;
    }
}
