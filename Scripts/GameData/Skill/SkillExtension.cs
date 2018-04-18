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
    public static KeyValuePair<DamageElement, MinMaxFloat> GetDamageAttribute(this Skill skill, int level, float effectiveness, float inflictRate)
    {
        if (!skill.IsAttack() || skill.skillAttackType != SkillAttackType.PureSkillDamage)
            return new KeyValuePair<DamageElement, MinMaxFloat>();
        level = skill.GetAdjustedLevel(level);
        return GameDataHelpers.MakeDamageAttributePair(skill.damageAttribute, level, effectiveness, inflictRate);
    }

    public static float GetDamageEffectiveness(this Skill skill, ICharacterData character)
    {
        if (skill == null)
            return 1f;
        return GameDataHelpers.CalculateEffectivenessDamage(skill.CacheEffectivenessAttributes, character);
    }

    public static float GetInflictRate(this Skill skill, int level)
    {
        if (!skill.IsAttack() || skill.skillAttackType != SkillAttackType.WeaponDamageInflict)
            return 1f;
        level = skill.GetAdjustedLevel(level);
        return skill.inflictRate.GetAmount(level);
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetAdditionalDamageAttributes(this Skill skill, int level)
    {
        if (!skill.IsAttack() || skill.skillAttackType != SkillAttackType.PureSkillDamage)
            return new Dictionary<DamageElement, MinMaxFloat>();
        level = skill.GetAdjustedLevel(level);
        return GameDataHelpers.MakeDamageAttributesDictionary(skill.additionalDamageAttributes, new Dictionary<DamageElement, MinMaxFloat>(), level);
    }
    #endregion
}
