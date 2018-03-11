using System.Collections;
using System.Collections.Generic;

public static class GameDataHelpers
{
    public static float CalculateEffectivenessDamage(Dictionary<Attribute, float> effectivenessAttributes, ICharacterData character)
    {
        var damageEffectiveness = 0f;
        var characterAttributes = character.Attributes;
        foreach (var characterAttribute in characterAttributes)
        {
            var attribute = characterAttribute.GetAttribute();
            if (effectivenessAttributes.ContainsKey(attribute))
                damageEffectiveness += effectivenessAttributes[attribute] * characterAttribute.amount;
        }
        return damageEffectiveness;
    }

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

    public static Dictionary<Attribute, float> MakeDamageEffectivenessAttributesDictionary(DamageEffectivenessAttribute[] sourceEffectivesses, Dictionary<Attribute, float> targetDictionary)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Attribute, float>();
        foreach (var sourceEffectivess in sourceEffectivesses)
        {
            var key = sourceEffectivess.attribute;
            if (!targetDictionary.ContainsKey(key))
                targetDictionary[key] = sourceEffectivess.effectiveness;
            else
                targetDictionary[key] += sourceEffectivess.effectiveness;
        }
        return targetDictionary;
    }

    public static Dictionary<DamageElement, DamageAmount> CombineDamageAttributesDictionary(Dictionary<DamageElement, DamageAmount> sourceDictionary, KeyValuePair<DamageElement, DamageAmount> newEntry)
    {
        var key = newEntry.Key;
        var value = newEntry.Value;
        if (!sourceDictionary.ContainsKey(key))
            sourceDictionary[key] = value;
        else
            sourceDictionary[key] += value;
        return sourceDictionary;
    }

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

    public static Dictionary<DamageElement, DamageAmount> MakeDamageAttributesDictionary(DamageAttribute[] sourceAttributes, Dictionary<DamageElement, DamageAmount> targetDictionary, int level)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<DamageElement, DamageAmount>();
        var gameInstance = GameInstance.Singleton;
        foreach (var sourceAttribute in sourceAttributes)
        {
            var key = sourceAttribute.damageElement;
            if (key == null)
                key = gameInstance.DefaultDamageElement;
            if (!targetDictionary.ContainsKey(key))
                targetDictionary[key] = sourceAttribute.baseDamageAmount + sourceAttribute.damageAmountIncreaseEachLevel * level;
            else
                targetDictionary[key] += sourceAttribute.baseDamageAmount + sourceAttribute.damageAmountIncreaseEachLevel * level;
        }
        return targetDictionary;
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

    public static Dictionary<Attribute, int> CombineAttributeAmountsDictionary(Dictionary<Attribute, int> sourceDictionary, KeyValuePair<Attribute, int> newEntry)
    {
        var key = newEntry.Key;
        var value = newEntry.Value;
        if (!sourceDictionary.ContainsKey(key))
            sourceDictionary[key] = value;
        else
            sourceDictionary[key] += value;
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

    public static Dictionary<Attribute, int> MakeAttributeAmountsDictionary(AttributeAmount[] sourceAmounts, Dictionary<Attribute, int> targetDictionary)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Attribute, int>();
        foreach (var sourceAmount in sourceAmounts)
        {
            var key = sourceAmount.attribute;
            if (key == null)
                continue;
            if (!targetDictionary.ContainsKey(key))
                targetDictionary[key] = sourceAmount.amount;
            else
                targetDictionary[key] += sourceAmount.amount;
        }
        return targetDictionary;
    }

    public static Dictionary<Attribute, int> MakeAttributeIncrementalsDictionary(AttributeIncremental[] sourceIncrementals, Dictionary<Attribute, int> targetDictionary, int level)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Attribute, int>();
        foreach (var sourceIncremental in sourceIncrementals)
        {
            var key = sourceIncremental.attribute;
            if (key == null)
                continue;
            if (!targetDictionary.ContainsKey(key))
                targetDictionary[key] = (int)(sourceIncremental.baseAmount + sourceIncremental.amountIncreaseEachLevel * level);
            else
                targetDictionary[key] += (int)(sourceIncremental.baseAmount + sourceIncremental.amountIncreaseEachLevel * level);
        }
        return targetDictionary;
    }

    public static Dictionary<Resistance, float> CombineResistanceAmountsDictionary(Dictionary<Resistance, float> sourceDictionary, KeyValuePair<Resistance, float> newEntry)
    {
        var key = newEntry.Key;
        var value = newEntry.Value;
        if (!sourceDictionary.ContainsKey(key))
            sourceDictionary[key] = value;
        else
            sourceDictionary[key] += value;
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

    public static Dictionary<Resistance, float> MakeResistanceAmountsDictionary(ResistanceAmount[] sourceAmounts, Dictionary<Resistance, float> targetDictionary)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Resistance, float>();
        foreach (var sourceAmount in sourceAmounts)
        {
            var key = sourceAmount.resistance;
            if (key == null)
                continue;
            if (!targetDictionary.ContainsKey(key))
                targetDictionary[key] = sourceAmount.amount;
            else
                targetDictionary[key] += sourceAmount.amount;
        }
        return targetDictionary;
    }

    public static Dictionary<Resistance, float> MakeResistanceIncrementalsDictionary(ResistanceIncremental[] sourceIncrementals, Dictionary<Resistance, float> targetDictionary, int level)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Resistance, float>();
        foreach (var sourceIncremental in sourceIncrementals)
        {
            var key = sourceIncremental.resistance;
            if (key == null)
                continue;
            if (!targetDictionary.ContainsKey(key))
                targetDictionary[key] = sourceIncremental.baseAmount + sourceIncremental.amountIncreaseEachLevel * level;
            else
                targetDictionary[key] += sourceIncremental.baseAmount + sourceIncremental.amountIncreaseEachLevel * level;
        }
        return targetDictionary;
    }

    public static Dictionary<Skill, int> CombineSkillLevelsDictionary(Dictionary<Skill, int> sourceDictionary, KeyValuePair<Skill, int> newEntry)
    {
        var key = newEntry.Key;
        var value = newEntry.Value;
        if (!sourceDictionary.ContainsKey(key))
            sourceDictionary[key] = value;
        else
            sourceDictionary[key] += value;
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

    public static Dictionary<Skill, int> MakeSkillLevelsDictionary(SkillLevel[] sourceLevels, Dictionary<Skill, int> targetDictionary)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Skill, int>();
        foreach (var sourceLevel in sourceLevels)
        {
            var key = sourceLevel.skill;
            if (key == null)
                continue;
            if (!targetDictionary.ContainsKey(key))
                targetDictionary[key] = sourceLevel.level;
            else
                targetDictionary[key] += sourceLevel.level;
        }
        return targetDictionary;
    }

    public static CharacterStats GetStatsByAttributeAmountPairs(Dictionary<Attribute, int> attributeAmountPairs)
    {
        var stats = new CharacterStats();
        foreach (var attributeAmountPair in attributeAmountPairs)
        {
            var attribute = attributeAmountPair.Key;
            var level = attributeAmountPair.Value;
            stats += attribute.statsIncreaseEachLevel * level;
        }
        return stats;
    }
}
