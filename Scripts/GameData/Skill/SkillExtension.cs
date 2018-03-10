using System.Collections;
using System.Collections.Generic;

public static class SkillExtension
{
    #region Skill Extension
    public static bool IsAttack(this Skill skill)
    {
        if (skill == null)
            return false;
        return skill.skillAttackType != SkillAttackType.None;
    }

    public static bool IsBuff(this Skill skill)
    {
        if (skill == null)
            return false;
        return skill.skillBuffType != SkillBuffType.None;
    }

    public static bool IsDebuff(this Skill skill)
    {
        if (skill == null)
            return false;
        return skill.IsAttack() && skill.isDebuff;
    }

    public static int GetRequireCharacterLevel(this Skill skill, int level)
    {
        if (skill == null)
            return 0;
        return skill.requirement.baseCharacterLevel + (int)(skill.requirement.characterLevelIncreaseEachLevel * level);
    }

    public static int GetMaxLevel(this Skill skill)
    {
        if (skill == null)
            return 0;
        return skill.maxLevel;
    }

    public static bool CanLevelUp(this Skill skill, ICharacterData character, int level)
    {
        if (skill == null || character == null)
            return false;

        var isPass = true;
        var skillLevelsDict = new Dictionary<Skill, int>();
        var skillLevels = character.Skills;
        foreach (var skillLevel in skillLevels)
        {
            if (skillLevel.GetSkill() == null)
                continue;
            skillLevelsDict[skillLevel.GetSkill()] = skillLevel.level;
        }
        var requireSkillLevels = skill.TempRequireSkillLevels;
        foreach (var requireSkillLevel in requireSkillLevels)
        {
            if (!skillLevelsDict.ContainsKey(requireSkillLevel.Key) || 
                skillLevelsDict[requireSkillLevel.Key] < requireSkillLevel.Value)
            {
                isPass = false;
                break;
            }
        }

        return character.Level >= skill.GetRequireCharacterLevel(level) && isPass;
    }

    public static int GetAdjustedLevel(this Skill skill, int level)
    {
        if (skill == null)
            return 0;
        if (level > skill.maxLevel)
            level = skill.maxLevel;
        return level;
    }

    public static int GetConsumeMp(this Skill skill, int level)
    {
        if (skill == null)
            return 0;
        level = skill.GetAdjustedLevel(level);
        return skill.baseConsumeMp + (int)(skill.consumeMpIncreaseEachLevel * level);
    }

    public static float GetCoolDownDuration(this Skill skill, int level)
    {
        if (skill == null)
            return 0f;
        level = skill.GetAdjustedLevel(level);
        return skill.baseCoolDownDuration + skill.coolDownDurationIncreaseEachLevel * level;
    }

    public static float GetBuffDuration(this Skill skill, int level)
    {
        if (skill.IsBuff())
            return 0f;
        level = skill.GetAdjustedLevel(level);
        var duration = skill.buff.GetDuration(level);
        if (duration < 0f)
            duration = 0f;
        return duration;
    }

    public static int GetBuffRecoveryHp(this Skill skill, int level)
    {
        if (skill.IsBuff())
            return 0;
        level = skill.GetAdjustedLevel(level);
        return skill.buff.GetRecoveryHp(level);
    }

    public static int GetBuffRecoveryMp(this Skill skill, int level)
    {
        if (skill.IsBuff())
            return 0;
        level = skill.GetAdjustedLevel(level);
        return skill.buff.GetRecoveryMp(level);
    }

    public static CharacterStats GetBuffStats(this Skill skill, int level)
    {
        if (skill.IsBuff())
            return new CharacterStats();
        level = skill.GetAdjustedLevel(level);
        return skill.buff.GetStats(level);
    }

    public static float GetDebuffDuration(this Skill skill, int level)
    {
        if (skill.IsDebuff())
            return 0f;
        level = skill.GetAdjustedLevel(level);
        var duration = skill.debuff.GetDuration(level);
        if (duration < 0f)
            duration = 0f;
        return duration;
    }

    public static int GetDebuffRecoveryHp(this Skill skill, int level)
    {
        if (skill.IsDebuff())
            return 0;
        level = skill.GetAdjustedLevel(level);
        return skill.debuff.GetRecoveryHp(level);
    }

    public static int GetDebuffRecoveryMp(this Skill skill, int level)
    {
        if (skill.IsDebuff())
            return 0;
        level = skill.GetAdjustedLevel(level);
        return skill.debuff.GetRecoveryMp(level);
    }

    public static CharacterStats GetDebuffStats(this Skill skill, int level)
    {
        if (skill.IsDebuff())
            return new CharacterStats();
        level = skill.GetAdjustedLevel(level);
        return skill.debuff.GetStats(level);
    }
    #endregion

    #region Buff Extension
    public static float GetDuration(this SkillBuff skillBuff, int level)
    {
        return skillBuff.baseDuration + skillBuff.durationIncreaseEachLevel * level;
    }

    public static int GetRecoveryHp(this SkillBuff skillBuff, int level)
    {
        return skillBuff.baseRecoveryHp + (int)(skillBuff.recoveryHpIncreaseEachLevel * level);
    }

    public static int GetRecoveryMp(this SkillBuff skillBuff, int level)
    {
        return skillBuff.baseRecoveryMp + (int)(skillBuff.recoveryMpIncreaseEachLevel * level);
    }

    public static CharacterStats GetStats(this SkillBuff skillBuff, int level)
    {
        return skillBuff.baseStats + skillBuff.statsIncreaseEachLevel * level;
    }
    #endregion
}
