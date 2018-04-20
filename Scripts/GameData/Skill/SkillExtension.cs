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
        return skill.requirement.characterLevel.GetAmount(level + 1);
    }

    public static int GetMaxLevel(this Skill skill)
    {
        if (skill == null)
            return 0;
        return skill.maxLevel;
    }

    public static bool CanLevelUp(this Skill skill, IPlayerCharacterData character, int level)
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
        var requireSkillLevels = skill.CacheRequireSkillLevels;
        foreach (var requireSkillLevel in requireSkillLevels)
        {
            if (!skillLevelsDict.ContainsKey(requireSkillLevel.Key) || 
                skillLevelsDict[requireSkillLevel.Key] < requireSkillLevel.Value)
            {
                isPass = false;
                break;
            }
        }

        return character.SkillPoint > 0 && level < skill.maxLevel && character.Level >= skill.GetRequireCharacterLevel(level) && isPass;
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
        return skill.consumeMp.GetAmount(level);
    }

    public static float GetCoolDownDuration(this Skill skill, int level)
    {
        if (skill == null)
            return 0f;
        level = skill.GetAdjustedLevel(level);
        var duration = skill.coolDownDuration.GetAmount(level);
        if (duration < 0f)
            duration = 0f;
        return duration;
    }
    #endregion

    #region Attack
    public static KeyValuePair<DamageElement, MinMaxFloat> GetDamageAmount(this Skill skill, int level, ICharacterData character)
    {
        if (!skill.IsAttack() || skill.skillAttackType != SkillAttackType.Normal)
            return new KeyValuePair<DamageElement, MinMaxFloat>();
        return GameDataHelpers.MakeDamageAmountPair(skill.damageAmount, level, skill.GetEffectivenessDamage(character));
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetDamageAmountWithInflictions(this Skill skill, int level, ICharacterData character)
    {
        if (!skill.IsAttack() || skill.skillAttackType != SkillAttackType.Normal)
            return new Dictionary<DamageElement, MinMaxFloat>();
        level = skill.GetAdjustedLevel(level);
        return GameDataHelpers.MakeDamageAmountWithInflictions(skill.damageAmount, level, skill.GetEffectivenessDamage(character), skill.GetDamageInflictions(level));
    }

    public static float GetEffectivenessDamage(this Skill skill, ICharacterData character)
    {
        if (skill == null)
            return 1f;
        return GameDataHelpers.CalculateEffectivenessDamage(skill.CacheEffectivenessAttributes, character);
    }

    public static Dictionary<DamageElement, float> GetDamageInflictions(this Skill skill, int level)
    {
        if (!skill.IsAttack())
            return new Dictionary<DamageElement, float>();
        level = skill.GetAdjustedLevel(level);
        return GameDataHelpers.MakeDamageInflictionAmountsDictionary(skill.damageInflictions, new Dictionary<DamageElement, float>(), level);
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetAdditionalDamageAmounts(this Skill skill, int level)
    {
        if (!skill.IsAttack())
            return new Dictionary<DamageElement, MinMaxFloat>();
        level = skill.GetAdjustedLevel(level);
        return GameDataHelpers.MakeDamageAmountsDictionary(skill.additionalDamageAmounts, new Dictionary<DamageElement, MinMaxFloat>(), level);
    }
    #endregion
}
