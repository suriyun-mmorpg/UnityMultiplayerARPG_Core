using System.Collections;
using System.Collections.Generic;

public static class GameDataHelpers
{
    public static KeyValuePair<DamageElement, DamageAmount> MakeDamageAttributePair(DamageAttribute source, int level)
    {
        var gameInstance = GameInstance.Singleton;
        var element = source.damageElement;
        if (element == null)
            element = gameInstance.DefaultDamageElement;
        return new KeyValuePair<DamageElement, DamageAmount>(element, source.baseDamageAmount + source.damageAmountIncreaseEachLevel * level);
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

    public static Dictionary<Attribute, int> MakeAttributeAmountDictionary(AttributeAmount[] sourceAmounts, Dictionary<Attribute, int> targetDictionary)
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

    public static Dictionary<Attribute, int> MakeAttributeIncrementalDictionary(AttributeIncremental[] sourceIncrementals, Dictionary<Attribute, int> targetDictionary, int level)
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

    public static Dictionary<Resistance, float> MakeResistanceAmountDictionary(ResistanceAmount[] sourceAmounts, Dictionary<Resistance, float> targetDictionary)
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

    public static Dictionary<Resistance, float> MakeResistanceIncrementalDictionary(ResistanceIncremental[] sourceIncrementals, Dictionary<Resistance, float> targetDictionary, int level)
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

    public static Dictionary<Skill, int> MakeSkillLevelDictionary(SkillLevel[] sourceLevels, Dictionary<Skill, int> targetDictionary)
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
