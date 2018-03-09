using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDataHelpers
{
    public static KeyValuePair<DamageElement, DamageAmount> MakeDamageAttributeCache(DamageAttribute source, int level)
    {
        var gameInstance = GameInstance.Singleton;
        var element = source.damageElement;
        if (element == null)
            element = gameInstance.DefaultDamageElement;
        return new KeyValuePair<DamageElement, DamageAmount>(element, source.baseDamageAmount + source.damageAmountIncreaseEachLevel * level);
    }

    public static void MakeDamageAttributesCache(DamageAttribute[] sourceAttributes, Dictionary<DamageElement, DamageAmount> cacheAttributes, int level)
    {
        cacheAttributes.Clear();
        var gameInstance = GameInstance.Singleton;
        foreach (var sourceAttribute in sourceAttributes)
        {
            var element = sourceAttribute.damageElement;
            if (element == null)
                element = gameInstance.DefaultDamageElement;
            if (!cacheAttributes.ContainsKey(element))
                cacheAttributes[element] = sourceAttribute.baseDamageAmount + sourceAttribute.damageAmountIncreaseEachLevel * level;
            else
                cacheAttributes[element] += sourceAttribute.baseDamageAmount + sourceAttribute.damageAmountIncreaseEachLevel * level;
        }
    }

    public static void MakeAttributeIncrementalCache(CharacterAttributeIncremental[] sourceIncrementals, Dictionary<CharacterAttribute, int> cacheIncremental, int level)
    {
        cacheIncremental.Clear();
        foreach (var sourceIncremental in sourceIncrementals)
        {
            var attribute = sourceIncremental.attribute;
            if (attribute == null)
                continue;
            if (!cacheIncremental.ContainsKey(attribute))
                cacheIncremental[attribute] = (int)(sourceIncremental.baseAmount + sourceIncremental.amountIncreaseEachLevel * level);
            else
                cacheIncremental[attribute] += (int)(sourceIncremental.baseAmount + sourceIncremental.amountIncreaseEachLevel * level);
        }
    }

    public static void MakeResistanceIncrementalCache(CharacterResistanceIncremental[] sourceIncrementals, Dictionary<CharacterResistance, float> cacheIncremental, int level)
    {
        cacheIncremental.Clear();
        foreach (var sourceIncremental in sourceIncrementals)
        {
            var resistance = sourceIncremental.resistance;
            if (resistance == null)
                continue;
            if (!cacheIncremental.ContainsKey(resistance))
                cacheIncremental[resistance] = sourceIncremental.baseAmount + sourceIncremental.amountIncreaseEachLevel * level;
            else
                cacheIncremental[resistance] += sourceIncremental.baseAmount + sourceIncremental.amountIncreaseEachLevel * level;
        }
    }

    public static CharacterStats GetStatsByAttributeAmountPairs(Dictionary<CharacterAttribute, int> attributeAmountPairs)
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
