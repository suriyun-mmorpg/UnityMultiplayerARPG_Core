using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class GameDataHelpers
    {
        #region Combine Dictionary with KeyValuePair functions
        public static Dictionary<DamageElement, MinMaxFloat> CombineDamageAmountsDictionary(Dictionary<DamageElement, MinMaxFloat> sourceDictionary, KeyValuePair<DamageElement, MinMaxFloat> newEntry)
        {
            GameInstance gameInstance = GameInstance.Singleton;
            DamageElement damageElement = newEntry.Key;
            if (damageElement == null)
                damageElement = gameInstance.DefaultDamageElement;
            MinMaxFloat value = newEntry.Value;
            if (!sourceDictionary.ContainsKey(damageElement))
                sourceDictionary[damageElement] = value;
            else
                sourceDictionary[damageElement] += value;
            return sourceDictionary;
        }

        public static Dictionary<DamageElement, float> CombineDamageInflictionAmountsDictionary(Dictionary<DamageElement, float> sourceDictionary, KeyValuePair<DamageElement, float> newEntry)
        {
            GameInstance gameInstance = GameInstance.Singleton;
            DamageElement damageElement = newEntry.Key;
            if (damageElement == null)
                damageElement = gameInstance.DefaultDamageElement;
            float value = newEntry.Value;
            if (!sourceDictionary.ContainsKey(damageElement))
                sourceDictionary[damageElement] = value;
            else
                sourceDictionary[damageElement] += value;
            return sourceDictionary;
        }

        public static Dictionary<Attribute, short> CombineAttributeAmountsDictionary(Dictionary<Attribute, short> sourceDictionary, KeyValuePair<Attribute, short> newEntry)
        {
            Attribute attribute = newEntry.Key;
            if (attribute != null)
            {
                short value = newEntry.Value;
                if (!sourceDictionary.ContainsKey(attribute))
                    sourceDictionary[attribute] = value;
                else
                    sourceDictionary[attribute] += value;
            }
            return sourceDictionary;
        }

        public static Dictionary<DamageElement, float> CombineResistanceAmountsDictionary(Dictionary<DamageElement, float> sourceDictionary, KeyValuePair<DamageElement, float> newEntry)
        {
            DamageElement damageElement = newEntry.Key;
            if (damageElement != null)
            {
                float value = newEntry.Value;
                if (!sourceDictionary.ContainsKey(damageElement))
                    sourceDictionary[damageElement] = value;
                else
                    sourceDictionary[damageElement] += value;
            }
            return sourceDictionary;
        }

        public static Dictionary<Skill, short> CombineSkillLevelsDictionary(Dictionary<Skill, short> sourceDictionary, KeyValuePair<Skill, short> newEntry)
        {
            Skill skill = newEntry.Key;
            if (skill != null)
            {
                short value = newEntry.Value;
                if (!sourceDictionary.ContainsKey(skill))
                    sourceDictionary[skill] = value;
                else
                    sourceDictionary[skill] += value;
            }
            return sourceDictionary;
        }

        public static Dictionary<Item, short> CombineItemAmountsDictionary(Dictionary<Item, short> sourceDictionary, KeyValuePair<Item, short> newEntry)
        {
            Item item = newEntry.Key;
            if (item != null)
            {
                short value = newEntry.Value;
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
                foreach (KeyValuePair<DamageElement, MinMaxFloat> entry in combineDictionary)
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
                foreach (KeyValuePair<DamageElement, float> entry in combineDictionary)
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
                foreach (KeyValuePair<Attribute, short> entry in combineDictionary)
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
                foreach (KeyValuePair<DamageElement, float> entry in combineDictionary)
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
                foreach (KeyValuePair<Skill, short> entry in combineDictionary)
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
            GameInstance gameInstance = GameInstance.Singleton;
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = gameInstance.DefaultDamageElement;
            return new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, (source.amount.GetAmount(level) * rate) + effectiveness);
        }

        public static Dictionary<DamageElement, MinMaxFloat> MakeDamageAmountWithInflictions(DamageIncremental source, short level, float rate, float effectiveness, Dictionary<DamageElement, float> damageInflictionAmounts)
        {
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            GameInstance gameInstance = GameInstance.Singleton;
            MinMaxFloat baseDamage = (source.amount.GetAmount(level) * rate) + effectiveness;
            if (damageInflictionAmounts != null && damageInflictionAmounts.Count > 0)
            {
                foreach (KeyValuePair<DamageElement, float> damageInflictionAmount in damageInflictionAmounts)
                {
                    DamageElement damageElement = damageInflictionAmount.Key;
                    result = CombineDamageAmountsDictionary(result, new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, baseDamage * damageInflictionAmount.Value));
                }
            }
            else
            {
                DamageElement damageElement = source.damageElement;
                if (damageElement == null)
                    damageElement = gameInstance.DefaultDamageElement;
                result = CombineDamageAmountsDictionary(result, new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, baseDamage));
            }
            return result;
        }

        public static KeyValuePair<DamageElement, float> MakeDamageInflictionPair(DamageInflictionAmount source)
        {
            GameInstance gameInstance = GameInstance.Singleton;
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = gameInstance.DefaultDamageElement;
            return new KeyValuePair<DamageElement, float>(damageElement, source.rate);
        }

        public static KeyValuePair<DamageElement, float> MakeDamageInflictionPair(DamageInflictionIncremental source, short level)
        {
            GameInstance gameInstance = GameInstance.Singleton;
            DamageElement damageElement = source.damageElement;
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
                foreach (DamageEffectivenessAttribute sourceEffectivess in sourceEffectivesses)
                {
                    Attribute key = sourceEffectivess.attribute;
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
                GameInstance gameInstance = GameInstance.Singleton;
                foreach (DamageIncremental sourceIncremental in sourceIncrementals)
                {
                    KeyValuePair<DamageElement, MinMaxFloat> pair = MakeDamageAmountPair(sourceIncremental, level, rate, 0f);
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
                GameInstance gameInstance = GameInstance.Singleton;
                foreach (DamageInflictionIncremental sourceIncremental in sourceIncrementals)
                {
                    KeyValuePair<DamageElement, float> pair = MakeDamageInflictionPair(sourceIncremental, level);
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
                foreach (AttributeAmount sourceAmount in sourceAmounts)
                {
                    KeyValuePair<Attribute, short> pair = MakeAttributeAmountPair(sourceAmount, rate);
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
                foreach (AttributeIncremental sourceIncremental in sourceIncrementals)
                {
                    KeyValuePair<Attribute, short> pair = MakeAttributeAmountPair(sourceIncremental, level, rate);
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
                foreach (ResistanceAmount sourceAmount in sourceAmounts)
                {
                    KeyValuePair<DamageElement, float> pair = MakeResistanceAmountPair(sourceAmount, rate);
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
                foreach (ResistanceIncremental sourceIncremental in sourceIncrementals)
                {
                    KeyValuePair<DamageElement, float> pair = MakeResistanceAmountPair(sourceIncremental, level, rate);
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
                foreach (SkillLevel sourceLevel in sourceLevels)
                {
                    KeyValuePair<Skill, short> pair = MakeSkillLevelPair(sourceLevel);
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
                foreach (ItemAmount sourceAmount in sourceAmounts)
                {
                    KeyValuePair<Item, short> pair = MakeItemAmountPair(sourceAmount);
                    targetDictionary = CombineItemAmountsDictionary(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }
        #endregion

        #region Calculate functions
        public static float GetEffectivenessDamage(Dictionary<Attribute, float> effectivenessAttributes, ICharacterData character)
        {
            float damageEffectiveness = 0f;
            if (effectivenessAttributes != null && character != null)
            {
                Dictionary<Attribute, short> characterAttributes = character.GetAttributes();
                foreach (KeyValuePair<Attribute, short> characterAttribute in characterAttributes)
                {
                    Attribute attribute = characterAttribute.Key;
                    if (attribute != null && effectivenessAttributes.ContainsKey(attribute))
                        damageEffectiveness += effectivenessAttributes[attribute] * characterAttribute.Value;
                }
            }
            return damageEffectiveness;
        }

        public static CharacterStats GetStatsFromAttributes(Dictionary<Attribute, short> attributeAmounts)
        {
            CharacterStats stats = new CharacterStats();
            if (attributeAmounts != null)
            {
                foreach (KeyValuePair<Attribute, short> attributeAmount in attributeAmounts)
                {
                    Attribute attribute = attributeAmount.Key;
                    short level = attributeAmount.Value;
                    stats += attribute.statsIncreaseEachLevel * level;
                }
            }
            return stats;
        }

        public static MinMaxFloat GetSumDamages(Dictionary<DamageElement, MinMaxFloat> damages)
        {
            MinMaxFloat damageAmount = new MinMaxFloat();
            damageAmount.min = 0;
            damageAmount.max = 0;
            if (damages == null || damages.Count == 0)
                return damageAmount;
            foreach (KeyValuePair<DamageElement, MinMaxFloat> damage in damages)
            {
                damageAmount += damage.Value;
            }
            return damageAmount;
        }
        #endregion
    }
}
