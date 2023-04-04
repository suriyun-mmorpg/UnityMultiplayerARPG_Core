using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.SOCIAL_SYSTEM_SETTING_FILE, menuName = GameDataMenuConsts.SOCIAL_SYSTEM_SETTING_MENU, order = GameDataMenuConsts.SOCIAL_SYSTEM_SETTING_ORDER)]
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
        [SerializeField]
        private int minGuildNameLength = 2;
        [SerializeField]
        private int maxGuildNameLength = 16;
        [SerializeField]
        private int minGuildRoleNameLength = 2;
        [SerializeField]
        private int maxGuildRoleNameLength = 16;
        [SerializeField]
        private int maxGuildMessageLength = 140;
        [SerializeField]
        private int maxGuildMessage2Length = 140;
        [Tooltip("Member roles from high to low priority")]
        [SerializeField]
        private GuildRoleData[] guildMemberRoles = new GuildRoleData[] {
            new GuildRoleData() { roleName = "Master", canInvite = true, canKick = true, canUseStorage = true },
            new GuildRoleData() { roleName = "Member 1", canInvite = false, canKick = false, canUseStorage = false },
            new GuildRoleData() { roleName = "Member 2", canInvite = false, canKick = false, canUseStorage = false },
            new GuildRoleData() { roleName = "Member 3", canInvite = false, canKick = false, canUseStorage = false },
            new GuildRoleData() { roleName = "Member 4", canInvite = false, canKick = false, canUseStorage = false },
            new GuildRoleData() { roleName = "Member 5", canInvite = false, canKick = false, canUseStorage = false },
        };
        [Range(0, 100)]
        [SerializeField]
        private byte maxShareExpPercentage = 20;
        [SerializeField]
        [ArrayElementTitle("item")]
        private ItemAmount[] createGuildRequireItems = new ItemAmount[0];
        [SerializeField]
        [ArrayElementTitle("currency")]
        private CurrencyAmount[] createGuildRequireCurrencies = new CurrencyAmount[0];
        [SerializeField]
        private int createGuildRequiredGold = 1000;
        [SerializeField]
        private int[] guildExpTree;

        [Header("Exp calculator")]
        public int guildMaxLevel;
        public Int32GraphCalculator guildExpCalculator;
        public bool guildCalculateExp;

        public int MaxGuildMember { get { return maxGuildMember; } }
        public int MinGuildNameLength { get { return minGuildNameLength; } }
        public int MaxGuildNameLength { get { return maxGuildNameLength; } }
        public int MinGuildRoleNameLength { get { return minGuildRoleNameLength; } }
        public int MaxGuildRoleNameLength { get { return maxGuildRoleNameLength; } }
        public int MaxGuildMessageLength { get { return maxGuildMessageLength; } }
        public int MaxGuildMessage2Length { get { return maxGuildMessage2Length; } }
        public GuildRoleData[] GuildMemberRoles { get { return guildMemberRoles; } }
        public byte MaxShareExpPercentage { get { return maxShareExpPercentage; } }

        [System.NonSerialized]
        private Dictionary<BaseItem, int> cacheCreateGuildRequireItems;
        public Dictionary<BaseItem, int> CacheCreateGuildRequireItems
        {
            get
            {
                if (cacheCreateGuildRequireItems == null)
                    cacheCreateGuildRequireItems = GameDataHelpers.CombineItems(createGuildRequireItems, new Dictionary<BaseItem, int>());
                return cacheCreateGuildRequireItems;
            }
        }

        [System.NonSerialized]
        private Dictionary<Currency, int> cacheCreateGuildRequireCurrencies;
        public Dictionary<Currency, int> CacheCreateGuildRequireCurrencies
        {
            get
            {
                if (cacheCreateGuildRequireCurrencies == null)
                    cacheCreateGuildRequireCurrencies = GameDataHelpers.CombineCurrencies(createGuildRequireCurrencies, new Dictionary<Currency, int>());
                return cacheCreateGuildRequireCurrencies;
            }
        }

        public int CreateGuildRequiredGold { get { return createGuildRequiredGold; } }

        public int[] GuildExpTree
        {
            get
            {
                if (guildExpTree == null)
                    guildExpTree = new int[] { 0 };
                return guildExpTree;
            }
            set
            {
                if (value != null)
                    guildExpTree = value;
            }
        }

        public bool CanCreateGuild(IPlayerCharacterData character)
        {
            return CanCreateGuild(character, out _);
        }

        public bool CanCreateGuild(IPlayerCharacterData character, out UITextKeys gameMessage)
        {
            gameMessage = UITextKeys.NONE;
            if (!GameInstance.Singleton.GameplayRule.CurrenciesEnoughToCreateGuild(character, this))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_CURRENCY_AMOUNTS;
                return false;
            }
            if (createGuildRequireItems == null || createGuildRequireItems.Length == 0)
            {
                // No required items
                return true;
            }
            foreach (ItemAmount requireItem in createGuildRequireItems)
            {
                if (requireItem.item != null && character.CountNonEquipItems(requireItem.item.DataId) < requireItem.amount)
                {
                    gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS;
                    return false;
                }
            }
            return true;
        }

        public void DecreaseCreateGuildResource(IPlayerCharacterData character)
        {
            if (createGuildRequireItems != null)
            {
                foreach (ItemAmount requireItem in createGuildRequireItems)
                {
                    if (requireItem.item != null && requireItem.amount > 0)
                        character.DecreaseItems(requireItem.item.DataId, requireItem.amount);
                }
                character.FillEmptySlots();
            }
            // Decrease required gold
            GameInstance.Singleton.GameplayRule.DecreaseCurrenciesWhenCreateGuild(character, this);
        }

        public int GetNextLevelExp(int level)
        {
            return GetNextLevelExp(GuildExpTree, level);
        }

        public GuildData IncreaseGuildExp(GuildData guild, int exp)
        {
            CalculateIncreasedGuildExp(
                GuildExpTree,
                guild.level,
                guild.exp + exp,
                guild.skillPoint,
                out guild.level,
                out guild.exp,
                out guild.skillPoint);
            return guild;
        }

        public static int GetNextLevelExp(int[] expTree, int level)
        {
            if (level <= 0)
                return 0;
            if (level > expTree.Length)
                return 0;
            return expTree[level - 1];
        }

        public static bool CalculateIncreasedGuildExp(
            int[] expTree,
            int level, 
            int exp, 
            int skillPoint, 
            out int resultLevel, 
            out int resultExp, 
            out int resultSkillPoint)
        {
            bool isLevelUp = false;
            int nextLevelExp = GetNextLevelExp(expTree, level);
            while (nextLevelExp > 0 && exp >= nextLevelExp)
            {
                exp = exp - nextLevelExp;
                ++level;
                nextLevelExp = GetNextLevelExp(expTree, level);
                skillPoint += 1;
                isLevelUp = true;
            }
            resultLevel = level;
            resultExp = exp;
            resultSkillPoint = skillPoint;
            return isLevelUp;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (guildMemberRoles.Length < 2)
            {
                Logging.LogWarning(ToString(), "`Guild Member Roles` must more or equals to 2");
                guildMemberRoles = new GuildRoleData[] {
                    guildMemberRoles[0],
                    new GuildRoleData() { roleName = "Member 1", canInvite = false, canKick = false, canUseStorage = false },
                };
                EditorUtility.SetDirty(this);
            }
            else if (guildMemberRoles.Length < 1)
            {
                Logging.LogWarning(ToString(), "`Guild Member Roles` must more or equals to 2");
                guildMemberRoles = new GuildRoleData[] {
                    new GuildRoleData() { roleName = "Master", canInvite = true, canKick = true, canUseStorage = true },
                    new GuildRoleData() { roleName = "Member 1", canInvite = false, canKick = false, canUseStorage = false },
                };
                EditorUtility.SetDirty(this);
            }
            // Calculate Exp tool
            if (guildCalculateExp)
            {
                guildCalculateExp = false;
                int[] guildExpTree = new int[guildMaxLevel];
                for (int i = 1; i <= guildMaxLevel; ++i)
                {
                    guildExpTree[i - 1] = guildExpCalculator.Calculate(i, guildMaxLevel);
                }
                GuildExpTree = guildExpTree;
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
