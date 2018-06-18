using System.Collections;
using System.Collections.Generic;

public static class GameDataHelpers
{
    #region Combine Dictionary with KeyValuePair functions
    public static Dictionary<DamageElement, MinMaxFloat> CombineDamageAmountsDictionary(Dictionary<DamageElement, MinMaxFloat> sourceDictionary, KeyValuePair<DamageElement, MinMaxFloat> newEntry)
    {
        var gameInstance = GameInstance.Singleton;
        var damageElement = newEntry.Key;
        if (damageElement == null)
            damageElement = gameInstance.DefaultDamageElement;
        var value = newEntry.Value;
        if (!sourceDictionary.ContainsKey(damageElement))
            sourceDictionary[damageElement] = value;
        else
            sourceDictionary[damageElement] += value;
        return sourceDictionary;
    }

    public static Dictionary<DamageElement, float> CombineDamageInflictionAmountsDictionary(Dictionary<DamageElement, float> sourceDictionary, KeyValuePair<DamageElement, float> newEntry)
    {
        var gameInstance = GameInstance.Singleton;
        var damageElement = newEntry.Key;
        if (damageElement == null)
            damageElement = gameInstance.DefaultDamageElement;
        var value = newEntry.Value;
        if (!sourceDictionary.ContainsKey(damageElement))
            sourceDictionary[damageElement] = value;
        else
            sourceDictionary[damageElement] += value;
        return sourceDictionary;
    }

    public static Dictionary<Attribute, short> CombineAttributeAmountsDictionary(Dictionary<Attribute, short> sourceDictionary, KeyValuePair<Attribute, short> newEntry)
    {
        var attribute = newEntry.Key;
        if (attribute != null)
        {
            var value = newEntry.Value;
            if (!sourceDictionary.ContainsKey(attribute))
                sourceDictionary[attribute] = value;
            else
                sourceDictionary[attribute] += value;
        }
        return sourceDictionary;
    }

    public static Dictionary<DamageElement, float> CombineResistanceAmountsDictionary(Dictionary<DamageElement, float> sourceDictionary, KeyValuePair<DamageElement, float> newEntry)
    {
        var damageElement = newEntry.Key;
        if (damageElement != null)
        {
            var value = newEntry.Value;
            if (!sourceDictionary.ContainsKey(damageElement))
                sourceDictionary[damageElement] = value;
            else
                sourceDictionary[damageElement] += value;
        }
        return sourceDictionary;
    }

    public static Dictionary<Skill, short> CombineSkillLevelsDictionary(Dictionary<Skill, short> sourceDictionary, KeyValuePair<Skill, short> newEntry)
    {
        var skill = newEntry.Key;
        if (skill != null)
        {
            var value = newEntry.Value;
            if (!sourceDictionary.ContainsKey(skill))
                sourceDictionary[skill] = value;
            else
                sourceDictionary[skill] += value;
        }
        return sourceDictionary;
    }

    public static Dictionary<Item, short> CombineItemAmountsDictionary(Dictionary<Item, short> sourceDictionary, KeyValuePair<Item, short> newEntry)
    {
        var item = newEntry.Key;
        if (item != null)
        {
            var value = newEntry.Value;
            if (!sourceDictionary.ContainsKey(item))
                sourceDictionary[item] = value;
            else
                sourceDictionary[item] += value;
        }
        return sourceDictionary;
    }
    #endregion

    #region Combine Dictionary with Dictionary functions
    public static Dictionary<DamageElement, MinMaxFloat> CombineDamageAmountsDictionary(Dictionary<DamageElement, MinMaxFloat> sourceDictionary, Dictionary<DamageElement, MinMaxFloat> combineDictionary)
    {
        if (combineDictionary != null)
        {
            foreach (var entry in combineDictionary)
            {
                CombineDamageAmountsDictionary(sourceDictionary, entry);
            }
        }
        return sourceDictionary;
    }

    public static Dictionary<DamageElement, float> CombineDamageInflictionAmountsDictionary(Dictionary<DamageElement, float> sourceDictionary, Dictionary<DamageElement, float> combineDictionary)
    {
        if (combineDictionary != null)
        {
            foreach (var entry in combineDictionary)
            {
                CombineDamageInflictionAmountsDictionary(sourceDictionary, entry);
            }
        }
        return sourceDictionary;
    }

    public static Dictionary<Attribute, short> CombineAttributeAmountsDictionary(Dictionary<Attribute, short> sourceDictionary, Dictionary<Attribute, short> combineDictionary)
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

    public static Dictionary<DamageElement, float> CombineResistanceAmountsDictionary(Dictionary<DamageElement, float> sourceDictionary, Dictionary<DamageElement, float> combineDictionary)
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

    public static Dictionary<Skill, short> CombineSkillLevelsDictionary(Dictionary<Skill, short> sourceDictionary, Dictionary<Skill, short> combineDictionary)
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
    public static KeyValuePair<DamageElement, MinMaxFloat> MakeDamageAmountPair(DamageIncremental source, short level, float rate, float effectiveness)
    {
        var gameInstance = GameInstance.Singleton;
        var damageElement = source.damageElement;
        if (damageElement == null)
            damageElement = gameInstance.DefaultDamageElement;
        return new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, (source.amount.GetAmount(level) * rate) + effectiveness);
    }

    public static Dictionary<DamageElement, MinMaxFloat> MakeDamageAmountWithInflictions(DamageIncremental source, short level, float rate, float effectiveness, Dictionary<DamageElement, float> damageInflictionAmounts)
    {
        var result = new Dictionary<DamageElement, MinMaxFloat>();
        var gameInstance = GameInstance.Singleton;
        var baseDamage = (source.amount.GetAmount(level) * rate) + effectiveness;
        if (damageInflictionAmounts != null && damageInflictionAmounts.Count > 0)
        {
            foreach (var damageInflictionAmount in damageInflictionAmounts)
            {
                var damageElement = damageInflictionAmount.Key;
                result = CombineDamageAmountsDictionary(result, new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, baseDamage * damageInflictionAmount.Value));
            }
        }
        else
        {
            var damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = gameInstance.DefaultDamageElement;
            result = CombineDamageAmountsDictionary(result, new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, baseDamage));
        }
        return result;
    }

    public static KeyValuePair<DamageElement, float> MakeDamageInflictionPair(DamageInflictionAmount source)
    {
        var gameInstance = GameInstance.Singleton;
        var damageElement = source.damageElement;
        if (damageElement == null)
            damageElement = gameInstance.DefaultDamageElement;
        return new KeyValuePair<DamageElement, float>(damageElement, source.rate);
    }

    public static KeyValuePair<DamageElement, float> MakeDamageInflictionPair(DamageInflictionIncremental source, short level)
    {
        var gameInstance = GameInstance.Singleton;
        var damageElement = source.damageElement;
        if (damageElement == null)
            damageElement = gameInstance.DefaultDamageElement;
        return new KeyValuePair<DamageElement, float>(damageElement, source.rate.GetAmount(level));
    }

    public static KeyValuePair<Attribute, short> MakeAttributeAmountPair(AttributeAmount source, float rate)
    {
        if (source.attribute == null)
            return new KeyValuePair<Attribute, short>();
        return new KeyValuePair<Attribute, short>(source.attribute, (short)(source.amount * rate));
    }

    public static KeyValuePair<Attribute, short> MakeAttributeAmountPair(AttributeIncremental source, short level, float rate)
    {
        if (source.attribute == null)
            return new KeyValuePair<Attribute, short>();
        return new KeyValuePair<Attribute, short>(source.attribute, (short)(source.amount.GetAmount(level) * rate));
    }

    public static KeyValuePair<DamageElement, float> MakeResistanceAmountPair(ResistanceAmount source, float rate)
    {
        if (source.damageElement == null)
            return new KeyValuePair<DamageElement, float>();
        return new KeyValuePair<DamageElement, float>(source.damageElement, source.amount * rate);
    }

    public static KeyValuePair<DamageElement, float> MakeResistanceAmountPair(ResistanceIncremental source, short level, float rate)
    {
        if (source.damageElement == null)
            return new KeyValuePair<DamageElement, float>();
        return new KeyValuePair<DamageElement, float>(source.damageElement, source.amount.GetAmount(level) * rate);
    }

    public static KeyValuePair<Skill, short> MakeSkillLevelPair(SkillLevel source)
    {
        if (source.skill == null)
            return new KeyValuePair<Skill, short>();
        return new KeyValuePair<Skill, short>(source.skill, source.level);
    }

    public static KeyValuePair<Item, short> MakeItemAmountPair(ItemAmount source)
    {
        if (source.item == null)
            return new KeyValuePair<Item, short>();
        return new KeyValuePair<Item, short>(source.item, source.amount);
    }
    #endregion

    #region Make Dictionary functions
    public static Dictionary<Attribute, float> MakeDamageEffectivenessAttributesDictionary(DamageEffectivenessAttribute[] sourceEffectivesses, Dictionary<Attribute, float> targetDictionary)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Attribute, float>();
        if (sourceEffectivesses != null)
        {
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
        }
        return targetDictionary;
    }

    public static Dictionary<DamageElement, MinMaxFloat> MakeDamageAmountsDictionary(DamageIncremental[] sourceIncrementals, Dictionary<DamageElement, MinMaxFloat> targetDictionary, short level, float rate)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<DamageElement, MinMaxFloat>();
        if (sourceIncrementals != null)
        {
            var gameInstance = GameInstance.Singleton;
            foreach (var sourceIncremental in sourceIncrementals)
            {
                var pair = MakeDamageAmountPair(sourceIncremental, level, rate, 0f);
                targetDictionary = CombineDamageAmountsDictionary(targetDictionary, pair);
            }
        }
        return targetDictionary;
    }

    public static Dictionary<DamageElement, float> MakeDamageInflictionAmountsDictionary(DamageInflictionIncremental[] sourceIncrementals, Dictionary<DamageElement, float> targetDictionary, short level)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<DamageElement, float>();
        if (sourceIncrementals != null)
        {
            var gameInstance = GameInstance.Singleton;
            foreach (var sourceIncremental in sourceIncrementals)
            {
                var pair = MakeDamageInflictionPair(sourceIncremental, level);
                targetDictionary = CombineDamageInflictionAmountsDictionary(targetDictionary, pair);
            }
        }
        return targetDictionary;
    }

    public static Dictionary<Attribute, short> MakeAttributeAmountsDictionary(AttributeAmount[] sourceAmounts, Dictionary<Attribute, short> targetDictionary, float rate)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Attribute, short>();
        if (sourceAmounts != null)
        {
            foreach (var sourceAmount in sourceAmounts)
            {
                var pair = MakeAttributeAmountPair(sourceAmount, rate);
                targetDictionary = CombineAttributeAmountsDictionary(targetDictionary, pair);
            }
        }
        return targetDictionary;
    }

    public static Dictionary<Attribute, short> MakeAttributeAmountsDictionary(AttributeIncremental[] sourceIncrementals, Dictionary<Attribute, short> targetDictionary, short level, float rate)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Attribute, short>();
        if (sourceIncrementals != null)
        {
            foreach (var sourceIncremental in sourceIncrementals)
            {
                var pair = MakeAttributeAmountPair(sourceIncremental, level, rate);
                targetDictionary = CombineAttributeAmountsDictionary(targetDictionary, pair);
            }
        }
        return targetDictionary;
    }

    public static Dictionary<DamageElement, float> MakeResistanceAmountsDictionary(ResistanceAmount[] sourceAmounts, Dictionary<DamageElement, float> targetDictionary, float rate)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<DamageElement, float>();
        if (sourceAmounts != null)
        {
            foreach (var sourceAmount in sourceAmounts)
            {
                var pair = MakeResistanceAmountPair(sourceAmount, rate);
                targetDictionary = CombineResistanceAmountsDictionary(targetDictionary, pair);
            }
        }
        return targetDictionary;
    }

    public static Dictionary<DamageElement, float> MakeResistanceAmountsDictionary(ResistanceIncremental[] sourceIncrementals, Dictionary<DamageElement, float> targetDictionary, short level, float rate)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<DamageElement, float>();
        if (sourceIncrementals != null)
        {
            foreach (var sourceIncremental in sourceIncrementals)
            {
                var pair = MakeResistanceAmountPair(sourceIncremental, level, rate);
                targetDictionary = CombineResistanceAmountsDictionary(targetDictionary, pair);
            }
        }
        return targetDictionary;
    }

    public static Dictionary<Skill, short> MakeSkillLevelsDictionary(SkillLevel[] sourceLevels, Dictionary<Skill, short> targetDictionary)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Skill, short>();
        if (sourceLevels != null)
        {
            foreach (var sourceLevel in sourceLevels)
            {
                var pair = MakeSkillLevelPair(sourceLevel);
                targetDictionary = CombineSkillLevelsDictionary(targetDictionary, pair);
            }
        }
        return targetDictionary;
    }

    public static Dictionary<Item, short> MakeItemAmountsDictionary(ItemAmount[] sourceAmounts, Dictionary<Item, short> targetDictionary)
    {
        if (targetDictionary == null)
            targetDictionary = new Dictionary<Item, short>();
        if (sourceAmounts != null)
        {
            foreach (var sourceAmount in sourceAmounts)
            {
                var pair = MakeItemAmountPair(sourceAmount);
                targetDictionary = CombineItemAmountsDictionary(targetDictionary, pair);
            }
        }
        return targetDictionary;
    }
    #endregion

    public static float CalculateEffectivenessDamage(Dictionary<Attribute, float> effectivenessAttributes, ICharacterData character)
    {
        var damageEffectiveness = 0f;
        if (effectivenessAttributes != null && character != null)
        {
            var characterAttributes = character.GetAttributes();
            foreach (var characterAttribute in characterAttributes)
            {
                var attribute = characterAttribute.Key;
                if (attribute != null && effectivenessAttributes.ContainsKey(attribute))
                    damageEffectiveness += effectivenessAttributes[attribute] * characterAttribute.Value;
            }
        }
        return damageEffectiveness;
    }

    public static CharacterStats CaculateStats(Dictionary<Attribute, short> attributeAmounts)
    {
        var stats = new CharacterStats();
        if (attributeAmounts != null)
        {
            foreach (var attributeAmount in attributeAmounts)
            {
                var attribute = attributeAmount.Key;
                short level = attributeAmount.Value;
                stats += attribute.statsIncreaseEachLevel * level;
            }
        }
        return stats;
    }

    public static MinMaxFloat GetSumDamages(Dictionary<DamageElement, MinMaxFloat> damages)
    {
        var damageAmount = new MinMaxFloat();
        damageAmount.min = 0;
        damageAmount.max = 0;
        if (damages == null || damages.Count == 0)
            return damageAmount;
        foreach (var damage in damages)
        {
            damageAmount += damage.Value;
        }
        return damageAmount;
    }
}
