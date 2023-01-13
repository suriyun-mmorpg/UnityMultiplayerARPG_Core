using UnityEngine;

namespace MultiplayerARPG
{
    public enum GuildSkillType
    {
        Active,
        Passive,
    }

    [CreateAssetMenu(fileName = GameDataMenuConsts.GUILD_SKILL_FILE, menuName = GameDataMenuConsts.GUILD_SKILL_MENU, order = GameDataMenuConsts.GUILD_SKILL_ORDER)]
    public partial class GuildSkill : BaseGameData
    {
        [Category("Skill Settings")]
        [Range(1, 100)]
        public int maxLevel = 1;
        public GuildSkillType skillType;

        [Category(2, "Activation Settings")]
        public IncrementalFloat coolDownDuration;

        [Category(3, "Buff/Bonus Settings")]
        public IncrementalInt increaseMaxMember;
        public IncrementalFloat increaseExpGainPercentage;
        public IncrementalFloat increaseGoldGainPercentage;
        public IncrementalFloat increaseShareExpGainPercentage;
        public IncrementalFloat increaseShareGoldGainPercentage;
        public IncrementalFloat decreaseExpLostPercentage;
        public Buff buff;

        public GuildSkillType GetSkillType()
        {
            return skillType;
        }

        public int GetIncreaseMaxMember(int level)
        {
            return increaseMaxMember.GetAmount(level);
        }

        public float GetIncreaseExpGainPercentage(int level)
        {
            return increaseExpGainPercentage.GetAmount(level);
        }

        public float GetIncreaseGoldGainPercentage(int level)
        {
            return increaseGoldGainPercentage.GetAmount(level);
        }

        public float GetIncreaseShareExpGainPercentage(int level)
        {
            return increaseShareExpGainPercentage.GetAmount(level);
        }

        public float GetIncreaseShareGoldGainPercentage(int level)
        {
            return increaseShareGoldGainPercentage.GetAmount(level);
        }

        public float GetDecreaseExpLostPercentage(int level)
        {
            return decreaseExpLostPercentage.GetAmount(level);
        }
        
        public bool IsActive
        {
            get { return skillType == GuildSkillType.Active; }
        }

        public Buff Buff
        {
            get { return buff; }
        }

        public bool CanLevelUp(IPlayerCharacterData character, int level)
        {
            if (character == null)
                return false;

            GuildData guildData;
            if (!GameInstance.ServerGuildHandlers.TryGetGuild(character.GuildId, out guildData))
                return false;

            return guildData.skillPoint > 0 && level < maxLevel;
        }

        public bool CanUse(ICharacterData character, int level)
        {
            if (character == null)
                return false;
            if (level <= 0)
                return false;
            int skillUsageIndex = character.IndexOfSkillUsage(DataId, SkillUsageType.GuildSkill);
            if (skillUsageIndex >= 0 && character.SkillUsages[skillUsageIndex].coolDownRemainsDuration > 0f)
                return false;
            return true;
        }

        public float GetCoolDownDuration(int level)
        {
            float duration = coolDownDuration.GetAmount(level);
            if (duration < 0f)
                duration = 0f;
            return duration;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            buff.PrepareRelatesData();
        }
    }
}
