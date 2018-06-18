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
        return skill.skillType == SkillType.Passive || skill.skillBuffType != SkillBuffType.None;
    }

    public static bool IsDebuff(this Skill skill)
    {
        if (skill == null)
            return false;
        return skill.IsAttack() && skill.isDebuff;
    }

    public static short GetRequireCharacterLevel(this Skill skill, short level)
    {
        if (skill == null)
            return 0;
        return skill.requirement.characterLevel.GetAmount((short)(level + 1));
    }

    public static int GetMaxLevel(this Skill skill)
    {
        if (skill == null)
            return 0;
        return skill.maxLevel;
    }

    public static bool CanLevelUp(this Skill skill, IPlayerCharacterData character, short level)
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

    public static short GetAdjustedLevel(this Skill skill, short level)
    {
        if (skill == null)
            return 0;
        if (level > skill.maxLevel)
            level = skill.maxLevel;
        return level;
    }

    public static int GetConsumeMp(this Skill skill, short level)
    {
        if (skill == null)
            return 0;
        level = skill.GetAdjustedLevel(level);
        return skill.consumeMp.GetAmount(level);
    }

    public static float GetCoolDownDuration(this Skill skill, short level)
    {
        if (skill == null)
            return 0f;
        level = skill.GetAdjustedLevel(level);
        var duration = skill.coolDownDuration.GetAmount(level);
        if (duration < 0f)
            duration = 0f;
        return duration;
    }

    public static bool CanCraft(this Skill skill, ICharacterData character)
    {
        if (skill.skillType != SkillType.CraftItem || skill.craftingItem == null)
            return false;
        var craftRequirements = skill.craftRequirements;
        if (craftRequirements == null || craftRequirements.Length == 0)
            return true;
        foreach (var craftRequirement in craftRequirements)
        {
            if (craftRequirement.item != null && character.CountNonEquipItems(craftRequirement.item.HashId) < craftRequirement.amount)
                return false;
        }
        return true;
    }
    #endregion

    #region Attack
    public static KeyValuePair<DamageElement, MinMaxFloat> GetDamageAmount(this Skill skill, short level, ICharacterData character)
    {
        if (!skill.IsAttack() || skill.skillAttackType != SkillAttackType.Normal)
            return new KeyValuePair<DamageElement, MinMaxFloat>();
        return GameDataHelpers.MakeDamageAmountPair(skill.damageAmount, level, 1f, skill.GetEffectivenessDamage(character));
    }

    public static float GetEffectivenessDamage(this Skill skill, ICharacterData character)
    {
        if (skill == null)
            return 1f;
        return GameDataHelpers.CalculateEffectivenessDamage(skill.CacheEffectivenessAttributes, character);
    }

    public static Dictionary<DamageElement, float> GetWeaponDamageInflictions(this Skill skill, short level)
    {
        if (!skill.IsAttack())
            return new Dictionary<DamageElement, float>();
        level = skill.GetAdjustedLevel(level);
        return GameDataHelpers.MakeDamageInflictionAmountsDictionary(skill.weaponDamageInflictions, new Dictionary<DamageElement, float>(), level);
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetAdditionalDamageAmounts(this Skill skill, short level)
    {
        if (!skill.IsAttack())
            return new Dictionary<DamageElement, MinMaxFloat>();
        level = skill.GetAdjustedLevel(level);
        return GameDataHelpers.MakeDamageAmountsDictionary(skill.additionalDamageAmounts, new Dictionary<DamageElement, MinMaxFloat>(), level, 1f);
    }
    #endregion
}
