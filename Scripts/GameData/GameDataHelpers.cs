using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class GameDataHelpers
    {
        #region Combine Dictionary with KeyValuePair functions
        /// <summary>
        /// Combine damage amounts dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, MinMaxFloat> CombineDamages(Dictionary<DamageElement, MinMaxFloat> sourceDictionary, KeyValuePair<DamageElement, MinMaxFloat> newEntry)
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

        /// <summary>
        /// Combine damage infliction amounts dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineDamageInflictions(Dictionary<DamageElement, float> sourceDictionary, KeyValuePair<DamageElement, float> newEntry)
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

        /// <summary>
        /// Combine attribute amounts dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<Attribute, short> CombineAttributes(Dictionary<Attribute, short> sourceDictionary, KeyValuePair<Attribute, short> newEntry)
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

        /// <summary>
        /// Combine resistance amounts dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineResistances(Dictionary<DamageElement, float> sourceDictionary, KeyValuePair<DamageElement, float> newEntry)
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

        /// <summary>
        /// Combine skill levels dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<Skill, short> CombineSkills(Dictionary<Skill, short> sourceDictionary, KeyValuePair<Skill, short> newEntry)
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

        /// <summary>
        /// Combine item amounts dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<Item, short> CombineItems(Dictionary<Item, short> sourceDictionary, KeyValuePair<Item, short> newEntry)
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
        /// <summary>
        /// Combine damage amounts dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, MinMaxFloat> CombineDamages(Dictionary<DamageElement, MinMaxFloat> sourceDictionary, Dictionary<DamageElement, MinMaxFloat> combineDictionary)
        {
            if (combineDictionary != null)
            {
                foreach (KeyValuePair<DamageElement, MinMaxFloat> entry in combineDictionary)
                {
                    CombineDamages(sourceDictionary, entry);
                }
            }
            return sourceDictionary;
        }

        /// <summary>
        /// Combine damage infliction amounts dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineDamageInflictions(Dictionary<DamageElement, float> sourceDictionary, Dictionary<DamageElement, float> combineDictionary)
        {
            if (combineDictionary != null)
            {
                foreach (KeyValuePair<DamageElement, float> entry in combineDictionary)
                {
                    CombineDamageInflictions(sourceDictionary, entry);
                }
            }
            return sourceDictionary;
        }

        /// <summary>
        /// Combine attribute amounts dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static Dictionary<Attribute, short> CombineAttributes(Dictionary<Attribute, short> sourceDictionary, Dictionary<Attribute, short> combineDictionary)
        {
            if (combineDictionary != null)
            {
                foreach (KeyValuePair<Attribute, short> entry in combineDictionary)
                {
                    CombineAttributes(sourceDictionary, entry);
                }
            }
            return sourceDictionary;
        }

        /// <summary>
        /// Combine resistance amounts dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineResistances(Dictionary<DamageElement, float> sourceDictionary, Dictionary<DamageElement, float> combineDictionary)
        {
            if (combineDictionary != null)
            {
                foreach (KeyValuePair<DamageElement, float> entry in combineDictionary)
                {
                    CombineResistances(sourceDictionary, entry);
                }
            }
            return sourceDictionary;
        }

        /// <summary>
        /// Combine skill levels dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static Dictionary<Skill, short> CombineSkills(Dictionary<Skill, short> sourceDictionary, Dictionary<Skill, short> combineDictionary)
        {
            if (combineDictionary != null)
            {
                foreach (KeyValuePair<Skill, short> entry in combineDictionary)
                {
                    CombineSkills(sourceDictionary, entry);
                }
            }
            return sourceDictionary;
        }

        /// <summary>
        /// Combine item amounts dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<Item, short> CombineItems(Dictionary<Item, short> sourceDictionary, Dictionary<Item, short> combineDictionary)
        {
            if (combineDictionary != null)
            {
                foreach (KeyValuePair<Item, short> entry in combineDictionary)
                {
                    CombineItems(sourceDictionary, entry);
                }
            }
            return sourceDictionary;
        }
        #endregion

        #region Make KeyValuePair functions
        /// <summary>
        /// Make damage - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rate"></param>
        /// <param name="effectiveness"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, MinMaxFloat> MakeDamage(DamageAmount source, float rate, float effectiveness)
        {
            GameInstance gameInstance = GameInstance.Singleton;
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = gameInstance.DefaultDamageElement;
            return new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, (source.amount * rate) + effectiveness);
        }

        /// <summary>
        /// Make damage amount
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <param name="effectiveness"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, MinMaxFloat> MakeDamage(DamageIncremental source, short level, float rate, float effectiveness)
        {
            GameInstance gameInstance = GameInstance.Singleton;
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = gameInstance.DefaultDamageElement;
            return new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, (source.amount.GetAmount(level) * rate) + effectiveness);
        }

        /// <summary>
        /// Make damage - amount key-value pair which calculates with damage inflictions
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <param name="effectiveness"></param>
        /// <param name="damageInflictions"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, MinMaxFloat> MakeDamageWithInflictions(DamageIncremental source, short level, float rate, float effectiveness, Dictionary<DamageElement, float> damageInflictions)
        {
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            GameInstance gameInstance = GameInstance.Singleton;
            MinMaxFloat baseDamage = (source.amount.GetAmount(level) * rate) + effectiveness;
            if (damageInflictions != null && damageInflictions.Count > 0)
            {
                foreach (KeyValuePair<DamageElement, float> damageInflictionAmount in damageInflictions)
                {
                    DamageElement damageElement = damageInflictionAmount.Key;
                    result = CombineDamages(result, new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, baseDamage * damageInflictionAmount.Value));
                }
            }
            else
            {
                DamageElement damageElement = source.damageElement;
                if (damageElement == null)
                    damageElement = gameInstance.DefaultDamageElement;
                result = CombineDamages(result, new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, baseDamage));
            }
            return result;
        }

        /// <summary>
        /// Make damage infliction - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, float> MakeDamageInfliction(DamageInflictionAmount source)
        {
            GameInstance gameInstance = GameInstance.Singleton;
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = gameInstance.DefaultDamageElement;
            return new KeyValuePair<DamageElement, float>(damageElement, source.rate);
        }

        /// <summary>
        /// Make damage infliction - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, float> MakeDamageInfliction(DamageInflictionIncremental source, short level)
        {
            GameInstance gameInstance = GameInstance.Singleton;
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = gameInstance.DefaultDamageElement;
            return new KeyValuePair<DamageElement, float>(damageElement, source.rate.GetAmount(level));
        }

        /// <summary>
        /// Make attribute - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<Attribute, short> MakeAttribute(AttributeAmount source, float rate)
        {
            if (source.attribute == null)
                return new KeyValuePair<Attribute, short>();
            return new KeyValuePair<Attribute, short>(source.attribute, (short)(source.amount * rate));
        }

        /// <summary>
        /// Make attribute - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<Attribute, short> MakeAttribute(AttributeIncremental source, short level, float rate)
        {
            if (source.attribute == null)
                return new KeyValuePair<Attribute, short>();
            return new KeyValuePair<Attribute, short>(source.attribute, (short)(source.amount.GetAmount(level) * rate));
        }

        /// <summary>
        /// Make resistance - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, float> MakeResistance(ResistanceAmount source, float rate)
        {
            if (source.damageElement == null)
                return new KeyValuePair<DamageElement, float>();
            return new KeyValuePair<DamageElement, float>(source.damageElement, source.amount * rate);
        }

        /// <summary>
        /// Make resistance - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, float> MakeResistance(ResistanceIncremental source, short level, float rate)
        {
            if (source.damageElement == null)
                return new KeyValuePair<DamageElement, float>();
            return new KeyValuePair<DamageElement, float>(source.damageElement, source.amount.GetAmount(level) * rate);
        }

        /// <summary>
        /// Make skill - level key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static KeyValuePair<Skill, short> MakeSkill(SkillLevel source)
        {
            if (source.skill == null)
                return new KeyValuePair<Skill, short>();
            return new KeyValuePair<Skill, short>(source.skill, source.level);
        }

        /// <summary>
        /// Make item - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static KeyValuePair<Item, short> MakeItem(ItemAmount source)
        {
            if (source.item == null)
                return new KeyValuePair<Item, short>();
            return new KeyValuePair<Item, short>(source.item, source.amount);
        }
        #endregion

        #region Make Dictionary functions
        /// <summary>
        /// Make damage effectiveness attribute amounts dictionary
        /// </summary>
        /// <param name="sourceEffectivesses"></param>
        /// <param name="targetDictionary"></param>
        /// <returns></returns>
        public static Dictionary<Attribute, float> MakeDamageEffectivenessAttributes(DamageEffectivenessAttribute[] sourceEffectivesses, Dictionary<Attribute, float> targetDictionary)
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

        /// <summary>
        /// Make damage amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, MinMaxFloat> MakeDamages(DamageAmount[] sourceAmounts, Dictionary<DamageElement, MinMaxFloat> targetDictionary, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, MinMaxFloat>();
            if (sourceAmounts != null)
            {
                GameInstance gameInstance = GameInstance.Singleton;
                foreach (DamageAmount sourceAmount in sourceAmounts)
                {
                    KeyValuePair<DamageElement, MinMaxFloat> pair = MakeDamage(sourceAmount, rate, 0f);
                    targetDictionary = CombineDamages(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Make damage amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, MinMaxFloat> MakeDamages(DamageIncremental[] sourceIncrementals, Dictionary<DamageElement, MinMaxFloat> targetDictionary, short level, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, MinMaxFloat>();
            if (sourceIncrementals != null)
            {
                GameInstance gameInstance = GameInstance.Singleton;
                foreach (DamageIncremental sourceIncremental in sourceIncrementals)
                {
                    KeyValuePair<DamageElement, MinMaxFloat> pair = MakeDamage(sourceIncremental, level, rate, 0f);
                    targetDictionary = CombineDamages(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Make damage infliction amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> MakeDamageInflictions(DamageInflictionIncremental[] sourceIncrementals, Dictionary<DamageElement, float> targetDictionary, short level)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            if (sourceIncrementals != null)
            {
                GameInstance gameInstance = GameInstance.Singleton;
                foreach (DamageInflictionIncremental sourceIncremental in sourceIncrementals)
                {
                    KeyValuePair<DamageElement, float> pair = MakeDamageInfliction(sourceIncremental, level);
                    targetDictionary = CombineDamageInflictions(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Make attribute amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<Attribute, short> MakeAttributes(AttributeAmount[] sourceAmounts, Dictionary<Attribute, short> targetDictionary, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Attribute, short>();
            if (sourceAmounts != null)
            {
                foreach (AttributeAmount sourceAmount in sourceAmounts)
                {
                    KeyValuePair<Attribute, short> pair = MakeAttribute(sourceAmount, rate);
                    targetDictionary = CombineAttributes(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Make attribute amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<Attribute, short> MakeAttributes(AttributeIncremental[] sourceIncrementals, Dictionary<Attribute, short> targetDictionary, short level, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Attribute, short>();
            if (sourceIncrementals != null)
            {
                foreach (AttributeIncremental sourceIncremental in sourceIncrementals)
                {
                    KeyValuePair<Attribute, short> pair = MakeAttribute(sourceIncremental, level, rate);
                    targetDictionary = CombineAttributes(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Make resistance amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> MakeResistances(ResistanceAmount[] sourceAmounts, Dictionary<DamageElement, float> targetDictionary, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            if (sourceAmounts != null)
            {
                foreach (ResistanceAmount sourceAmount in sourceAmounts)
                {
                    KeyValuePair<DamageElement, float> pair = MakeResistance(sourceAmount, rate);
                    targetDictionary = CombineResistances(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Make resistance amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> MakeResistances(ResistanceIncremental[] sourceIncrementals, Dictionary<DamageElement, float> targetDictionary, short level, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            if (sourceIncrementals != null)
            {
                foreach (ResistanceIncremental sourceIncremental in sourceIncrementals)
                {
                    KeyValuePair<DamageElement, float> pair = MakeResistance(sourceIncremental, level, rate);
                    targetDictionary = CombineResistances(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Make skill levels dictionary
        /// </summary>
        /// <param name="sourceLevels"></param>
        /// <param name="targetDictionary"></param>
        /// <returns></returns>
        public static Dictionary<Skill, short> MakeSkills(SkillLevel[] sourceLevels, Dictionary<Skill, short> targetDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Skill, short>();
            if (sourceLevels != null)
            {
                foreach (SkillLevel sourceLevel in sourceLevels)
                {
                    KeyValuePair<Skill, short> pair = MakeSkill(sourceLevel);
                    targetDictionary = CombineSkills(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Make item amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="targetDictionary"></param>
        /// <returns></returns>
        public static Dictionary<Item, short> MakeItems(ItemAmount[] sourceAmounts, Dictionary<Item, short> targetDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Item, short>();
            if (sourceAmounts != null)
            {
                foreach (ItemAmount sourceAmount in sourceAmounts)
                {
                    KeyValuePair<Item, short> pair = MakeItem(sourceAmount);
                    targetDictionary = CombineItems(targetDictionary, pair);
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
