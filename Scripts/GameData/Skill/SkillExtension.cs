using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class SkillExtension
    {
        #region Skill Extension
        public static short GetRequireCharacterLevel(this BaseSkill skill, short level)
        {
            if (skill == null)
                return 0;
            return skill.requirement.characterLevel.GetAmount((short)(level + 1));
        }

        public static int GetMaxLevel(this BaseSkill skill)
        {
            if (skill == null)
                return 0;
            return skill.maxLevel;
        }

        public static bool IsAvailable(this BaseSkill skill, ICharacterData skillLearner)
        {
            if (skill == null)
                return false;
            short skillLevel;
            return skillLearner.GetCaches().Skills.TryGetValue(skill, out skillLevel) && skillLevel > 0;
        }

        public static short GetAdjustedLevel(this BaseSkill skill, short level)
        {
            if (skill == null)
                return 0;
            if (level > skill.maxLevel)
                level = skill.maxLevel;
            return level;
        }

        public static float GetCastDuration(this BaseSkill skill, short level)
        {
            if (skill == null)
                return 0;
            level = skill.GetAdjustedLevel(level);
            return skill.castDuration.GetAmount(level);
        }

        public static int GetConsumeMp(this BaseSkill skill, short level)
        {
            if (skill == null)
                return 0;
            level = skill.GetAdjustedLevel(level);
            return skill.consumeMp.GetAmount(level);
        }

        public static float GetCoolDownDuration(this BaseSkill skill, short level)
        {
            if (skill == null)
                return 0f;
            level = skill.GetAdjustedLevel(level);
            float duration = skill.coolDownDuration.GetAmount(level);
            if (duration < 0f)
                duration = 0f;
            return duration;
        }
        #endregion
    }
}
