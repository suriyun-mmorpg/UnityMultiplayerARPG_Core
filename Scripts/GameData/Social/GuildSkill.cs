using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum GuildSkillType
    {
        Active,
        Passive,
    }

    [CreateAssetMenu(fileName = "Guild Skill", menuName = "Create GameData/Guild Skill", order = -4698)]
    public partial class GuildSkill : BaseGameData
    {
        [Header("Guild Skill Configs")]
        public GuildSkillType skillType;

        [Range(1, 100)]
        public short maxLevel = 1;

        [Header("Bonus")]
        public IncrementalInt increaseMaxMember;
        public IncrementalFloat increaseExpGainPercentage;
        public IncrementalFloat increaseGoldGainPercentage;
        public IncrementalFloat increaseShareExpGainPercentage;
        public IncrementalFloat increaseShareGoldGainPercentage;
        public IncrementalFloat decreaseExpLostPercentage;

        [Header("Cool Down")]
        public IncrementalFloat coolDownDuration;

        [Header("Buffs")]
        public Buff buff;

        public GuildSkillType GetSkillType()
        {
            return skillType;
        }

        public int GetIncreaseMaxMember(short level)
        {
            return increaseMaxMember.GetAmount(level);
        }

        public float GetIncreaseExpGainPercentage(short level)
        {
            return increaseExpGainPercentage.GetAmount(level);
        }

        public float GetIncreaseGoldGainPercentage(short level)
        {
            return increaseGoldGainPercentage.GetAmount(level);
        }

        public float GetIncreaseShareExpGainPercentage(short level)
        {
            return increaseShareExpGainPercentage.GetAmount(level);
        }

        public float GetIncreaseShareGoldGainPercentage(short level)
        {
            return increaseShareGoldGainPercentage.GetAmount(level);
        }

        public float GetDecreaseExpLostPercentage(short level)
        {
            return decreaseExpLostPercentage.GetAmount(level);
        }
        
        public bool IsBuff()
        {
            return skillType == GuildSkillType.Active;
        }

        public Buff GetBuff()
        {
            return buff;
        }

        public bool CanLevelUp(IPlayerCharacterData character, short level)
        {
            if (character == null)
                return false;

            BaseGameNetworkManager gameManager = BaseGameNetworkManager.Singleton;
            if (gameManager == null)
                return false;

            GuildData guildData = null;
            if (!gameManager.TryGetGuild(character.GuildId, out guildData))
                return false;

            return guildData.skillPoint > 0 && level < maxLevel;
        }

        public bool CanUse(ICharacterData character, short level)
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

        public float GetCoolDownDuration(short level)
        {
            float duration = coolDownDuration.GetAmount(level);
            if (duration < 0f)
                duration = 0f;
            return duration;
        }
    }
}
