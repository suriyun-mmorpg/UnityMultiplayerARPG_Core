using System.Collections;
using System.Collections.Generic;

public static class GameDataHelpers
{
    #region Combine Dictionary with KeyValuePair functions
    public static Dictionary<DamageElement, DamageAmount> CombineDamageAttributesDictionary(Dictionary<DamageElement, DamageAmount> sourceDictionary, KeyValuePair<DamageElement, DamageAmount> newEntry)
    {
        var key = newEntry.Key;
        if (key != null)
        {
            var value = newEntry.Value;
            if (!sourceDictionary.ContainsKey(key))
                sourceDictionary[key] = value;
            else
                sourceDictionary[key] += value;
        }
        return sourceDictionary;
    }

    public static Dictionary<Attribute, int> CombineAttributeAmountsDictionary(Dictionary<Attribute, int> sourceDictionary, KeyValuePair<Attribute, int> newEntry)
    {
        var key = newEntry.Key;
        if (key != null)
        {
            var value = newEntry.Value;
            if (!sourceDictionary.ContainsKey(key))
                sourceDictionary[key] = value;
            else
                sourceDictionary[key] += value;
        }
        return sourceDictionary;
    }

    public static Dictionary<Resistance, float> CombineResistanceAmountsDictionary(Dictionary<Resistance, float> sourceDictionary, KeyValuePair<Resistance, float> newEntry)
    {
        var key = newEntry.Key;
        if (key != null)
        {
            var value = newEntry.Value;
            if (!sourceDictionary.ContainsKey(key))
                sourceDictionary[key] = value;
            else
                sourceDictionary[key] += value;
        }
        return sourceDictionary;
    }

    public static Dictionary<Skill, int> CombineSkillLevelsDictionary(Dictionary<Skill, int> sourceDictionary, KeyValuePair<Skill, int> newEntry)
    {
        var key = newEntry.Key;
        if (key != null)
        {
            var value = newEntry.Value;
            if (!sourceDictionary.ContainsKey(key))
                sourceDictionary[key] = value;
            else
                sourceDictionary[key] += value;
        }
        return sourceDictionary;
    }
    #endregion

    #region Combine Dictionary with Dictionary functions
    public static Dictionary<DamageElement, DamageAmount> CombineDamageAttributesDictionary(Dictionary<DamageElement, DamageAmount> sourceDictionary, Dictionary<DamageElement, DamageAmount> combineDictionary)
    {
        if (combineDictionary != null)
        {
            foreach (var entry in combineDictionary)
            {
                CombineDamageAttributesDictionary(sourceDictionary, entry);
            }
        }
        return sourceDictionary;
    }

    public static Dictionary<Attribute, int> CombineAttributeAmountsDictionary(Dictionary<Attribute, int> sourceDictionary, Dictionary<Attribute, int> combineDictionary)
    {
        if (combineDictionary != null)
        {
            foreach (var entry in combineDictionary)
            {
                CombineAttributeAmountsDictionary(sourceDictionary, entry);
            }
        }
        return sourceDictionary;
    }

    public static Dictionary<Resistance, float> CombineResistanceAmountsDictionary(Dictionary<Resistance, float> sourceDictionary, Dictionary<Resistance, float> combineDictionary)
    {
        if (combineDictionary != null)
        {
            foreach (var entry in combineDictionary)
            {
                CombineResistanceAmountsDictionary(sourceDictionary, entry);
            }
        }
        return sourceDictionary;
    }

    public static Dictionary<Skill, int> CombineSkillLevelsDictionary(Dictionary<Skill, int> sourceDictionary, Dictionary<Skill, int> combineDictionary)
    {
        if (combineDictionary != null)
        {
            foreach (var entry in combineDictionary)
            {
                CombineSkillLevelsDictionary(sourceDictionary, entry);
            }
        }
        return sourceDictionary;
    }
    #endregion

    #region Make KeyValuePair functions
    public static KeyValuePair<DamageElement, DamageAmount> MakeDamageAttributePair(DamageAttribute source, int level, float effectiveness, float inflictRate)
    {
        var gameInstance = GameInstance.Singleton;
        var element = source.damageElement;
        if (element == null)
            element = gameInstance.DefaultDamageElement;
        return new KeyValuePair<DamageElement, DamageAmount>(element,
            (source.baseDamageAmount +
            (source.damageAmountIncreaseEachLevel * level) +
            effectiveness) * inflictRate);
    }

    public static KeyValuePair<Attribute, int> MakeAttributeAmountPair(AttributeAmount sourceAmount)
    {
        if (sourceAmount.attribute == null)
            return new KeyValuePair<Attribute, int>();
        return new KeyValuePair<Attribute, int>(sourceAmount.attribute, sourceAmount.amount);
    }

    public static KeyValuePair<Attribute, int> MakeAttributeAmountPair(AttributeIncremental sourceIncremental, int level)
    {
        if (sourceIncremental.attribute == null)
            return new KeyValuePair<Attribute, int>();
        return new KeyValuePair<Attribute, int>(sourceIncremental.attribute, sourceIncremental.GetAmount(level));
    }

    public static KeyValuePair<Resistance, float> MakeResistanceAmountPair(ResistanceAmount sourceAmount)
    {
        if (sourceAmount.resistance == null)
            return new KeyValuePair<Resistance, float>();
        return new KeyValuePair<Resistance, float>(sourceAmount.resistance, sourceAmount.amount);
    }

    public static KeyValuePair<Resistance, float> MakeResistanceAmountPair(ResistanceIncremental sourceIncremental, int level)
    {
        if (sourceIncremental.resistance == null)
            return new KeyValuePair<Resistance, float>();
        return new KeyValuePair<Resistance, float>(sourceIncremental.resistance, sourceIncremental.GetAmount(level));
    }

    public static KeyValuePair<Skill, int> MakeSkillLevelPair(SkillLevel sourceLevel)
    {
        if (sourceLevel.skill == null)
            return new KeyValuePair<Skill, int>();
        return new KeyValuePair<Skill, int>(sourceLevel.skill, sourceLevel.level);
    }
    #endregion

    #region Make Dictionary functions
    public static Dictionary<Attribute, float> MakeDamageEffectivenessAttributesDictionary(DamageEffectivenessAttribute[] sourceEffectivesses, Dictionary<Attribute, float> targetDictionary)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Attribute, float>();
        foreach (var sourceEffectivess in sourceEffectivesses)
        {
            var key = sourceEffectivess.attribute;
            if (key == null)
                continue;
            if (!targetDictionary.ContainsKey(key))
                targetDictionary[key] = sourceEffectivess.effectiveness;
            else
                targetDictionary[key] += sourceEffectivess.effectiveness;
        }
        return targetDictionary;
    }

    public static Dictionary<DamageElement, DamageAmount> MakeDamageAttributesDictionary(DamageAttribute[] sourceAttributes, Dictionary<DamageElement, DamageAmount> targetDictionary, int level)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<DamageElement, DamageAmount>();
        var gameInstance = GameInstance.Singleton;
        foreach (var sourceAttribute in sourceAttributes)
        {
            var pair = MakeDamageAttributePair(sourceAttribute, level, 0f, 1f);
            targetDictionary = CombineDamageAttributesDictionary(targetDictionary, pair);
        }
        return targetDictionary;
    }

    public static Dictionary<Attribute, int> MakeAttributeAmountsDictionary(AttributeAmount[] sourceAmounts, Dictionary<Attribute, int> targetDictionary)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Attribute, int>();
        foreach (var sourceAmount in sourceAmounts)
        {
            var pair = MakeAttributeAmountPair(sourceAmount);
            targetDictionary = CombineAttributeAmountsDictionary(targetDictionary, pair);
        }
        return targetDictionary;
    }

    public static Dictionary<Attribute, int> MakeAttributeAmountsDictionary(AttributeIncremental[] sourceIncrementals, Dictionary<Attribute, int> targetDictionary, int level)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Attribute, int>();
        foreach (var sourceIncremental in sourceIncrementals)
        {
            var pair = MakeAttributeAmountPair(sourceIncremental, level);
            targetDictionary = CombineAttributeAmountsDictionary(targetDictionary, pair);
        }
        return targetDictionary;
    }

    public static Dictionary<Resistance, float> MakeResistanceAmountsDictionary(ResistanceAmount[] sourceAmounts, Dictionary<Resistance, float> targetDictionary)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Resistance, float>();
        foreach (var sourceAmount in sourceAmounts)
        {
            var pair = MakeResistanceAmountPair(sourceAmount);
            targetDictionary = CombineResistanceAmountsDictionary(targetDictionary, pair);
        }
        return targetDictionary;
    }

    public static Dictionary<Resistance, float> MakeResistanceAmountsDictionary(ResistanceIncremental[] sourceIncrementals, Dictionary<Resistance, float> targetDictionary, int level)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Resistance, float>();
        foreach (var sourceIncremental in sourceIncrementals)
        {
            var pair = MakeResistanceAmountPair(sourceIncremental, level);
            targetDictionary = CombineResistanceAmountsDictionary(targetDictionary, pair);
        }
        return targetDictionary;
    }
    
    public static Dictionary<Skill, int> MakeSkillLevelsDictionary(SkillLevel[] sourceLevels, Dictionary<Skill, int> targetDictionary)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Skill, int>();
        foreach (var sourceLevel in sourceLevels)
        {
            var pair = MakeSkillLevelPair(sourceLevel);
            targetDictionary = CombineSkillLevelsDictionary(targetDictionary, pair);
        }
        return targetDictionary;
    }
    #endregion

    public static float CalculateEffectivenessDamage(Dictionary<Attribute, float> effectivenessAttributes, ICharacterData character)
    {
        var damageEffectiveness = 0f;
        var characterAttributes = character.GetAttributesWithBuffs();
        foreach (var characterAttribute in characterAttributes)
        {
            var attribute = characterAttribute.Key;
            if (attribute != null && effectivenessAttributes.ContainsKey(attribute))
                damageEffectiveness += effectivenessAttributes[attribute] * characterAttribute.Value;
        }
        return damageEffectiveness;
    }

    public static CharacterStats CaculateStats(Dictionary<Attribute, int> attributeAmountsDictionary)
    {
        var stats = new CharacterStats();
        foreach (var attributeAmountPair in attributeAmountsDictionary)
        {
            var attribute = attributeAmountPair.Key;
            var level = attributeAmountPair.Value;
            stats += attribute.statsIncreaseEachLevel * level;
        }
        return stats;
    }

    public static DamageAmount GetSumDamages(Dictionary<DamageElement, DamageAmount> damages)
    {
        var damageAmount = new DamageAmount();
        damageAmount.minDamage = 0;
        damageAmount.maxDamage = 0;
        if (damages == null || damages.Count == 0)
            return damageAmount;
        foreach (var damage in damages)
        {
            damageAmount += damage.Value;
        }
        return damageAmount;
    }
}
