using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class GameDataHelpers
    {
        #region Combine Dictionary with KeyValuePair functions
        /// <summary>
        /// Combine damage amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, MinMaxFloat> CombineDamages(Dictionary<DamageElement, MinMaxFloat> targetDictionary, KeyValuePair<DamageElement, MinMaxFloat> newEntry)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, MinMaxFloat>();
            DamageElement damageElement = newEntry.Key;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            MinMaxFloat value = newEntry.Value;
            if (!targetDictionary.ContainsKey(damageElement))
                targetDictionary[damageElement] = value;
            else
                targetDictionary[damageElement] += value;
            return targetDictionary;
        }

        /// <summary>
        /// Combine damage infliction amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineDamageInflictions(Dictionary<DamageElement, float> targetDictionary, KeyValuePair<DamageElement, float> newEntry)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            DamageElement damageElement = newEntry.Key;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            float value = newEntry.Value;
            if (!targetDictionary.ContainsKey(damageElement))
                targetDictionary[damageElement] = value;
            else
                targetDictionary[damageElement] += value;
            return targetDictionary;
        }

        /// <summary>
        /// Combine attribute amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<Attribute, float> CombineAttributes(Dictionary<Attribute, float> targetDictionary, KeyValuePair<Attribute, float> newEntry)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Attribute, float>();
            if (newEntry.Key != null)
            {
                if (!targetDictionary.ContainsKey(newEntry.Key))
                    targetDictionary[newEntry.Key] = newEntry.Value;
                else
                    targetDictionary[newEntry.Key] += newEntry.Value;
            }
            return targetDictionary;
        }

        /// <summary>
        /// Multiply attribute amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="multiplyEntry"></param>
        /// <returns></returns>
        public static Dictionary<Attribute, float> MultiplyAttributes(Dictionary<Attribute, float> targetDictionary, KeyValuePair<Attribute, float> multiplyEntry)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Attribute, float>();
            if (multiplyEntry.Key != null)
            {
                if (targetDictionary.ContainsKey(multiplyEntry.Key))
                    targetDictionary[multiplyEntry.Key] *= multiplyEntry.Value;
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine resistance amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineResistances(Dictionary<DamageElement, float> targetDictionary, KeyValuePair<DamageElement, float> newEntry)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            if (newEntry.Key != null)
            {
                if (!targetDictionary.ContainsKey(newEntry.Key))
                    targetDictionary[newEntry.Key] = newEntry.Value;
                else
                    targetDictionary[newEntry.Key] += newEntry.Value;
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine armor amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineArmors(Dictionary<DamageElement, float> targetDictionary, KeyValuePair<DamageElement, float> newEntry)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            if (newEntry.Key != null)
            {
                if (!targetDictionary.ContainsKey(newEntry.Key))
                    targetDictionary[newEntry.Key] = newEntry.Value;
                else
                    targetDictionary[newEntry.Key] += newEntry.Value;
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine skill levels dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<BaseSkill, short> CombineSkills(Dictionary<BaseSkill, short> targetDictionary, KeyValuePair<BaseSkill, short> newEntry)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<BaseSkill, short>();
            if (newEntry.Key != null)
            {
                if (!targetDictionary.ContainsKey(newEntry.Key))
                    targetDictionary[newEntry.Key] = newEntry.Value;
                else
                    targetDictionary[newEntry.Key] += newEntry.Value;
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine item amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<Item, short> CombineItems(Dictionary<Item, short> targetDictionary, KeyValuePair<Item, short> newEntry)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Item, short>();
            if (newEntry.Key != null)
            {
                if (!targetDictionary.ContainsKey(newEntry.Key))
                    targetDictionary[newEntry.Key] = newEntry.Value;
                else
                    targetDictionary[newEntry.Key] += newEntry.Value;
            }
            return targetDictionary;
        }
        #endregion

        #region Combine Dictionary with Dictionary functions
        /// <summary>
        /// Combine damage amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, MinMaxFloat> CombineDamages(Dictionary<DamageElement, MinMaxFloat> targetDictionary, Dictionary<DamageElement, MinMaxFloat> combineDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, MinMaxFloat>();
            if (combineDictionary != null && combineDictionary.Count > 0)
            {
                foreach (KeyValuePair<DamageElement, MinMaxFloat> entry in combineDictionary)
                {
                    CombineDamages(targetDictionary, entry);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine damage infliction amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineDamageInflictions(Dictionary<DamageElement, float> targetDictionary, Dictionary<DamageElement, float> combineDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            if (combineDictionary != null && combineDictionary.Count > 0)
            {
                foreach (KeyValuePair<DamageElement, float> entry in combineDictionary)
                {
                    CombineDamageInflictions(targetDictionary, entry);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine attribute amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static Dictionary<Attribute, float> CombineAttributes(Dictionary<Attribute, float> targetDictionary, Dictionary<Attribute, float> combineDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Attribute, float>();
            if (combineDictionary != null && combineDictionary.Count > 0)
            {
                foreach (KeyValuePair<Attribute, float> entry in combineDictionary)
                {
                    CombineAttributes(targetDictionary, entry);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Multiply attribute amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="multiplyDictionary"></param>
        /// <returns></returns>
        public static Dictionary<Attribute, float> MultiplyAttributes(Dictionary<Attribute, float> targetDictionary, Dictionary<Attribute, float> multiplyDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Attribute, float>();
            if (multiplyDictionary != null && multiplyDictionary.Count > 0)
            {
                // Remove attributes that are not multiplying
                List<Attribute> availableAttributes = new List<Attribute>(targetDictionary.Keys);
                foreach (Attribute attribute in availableAttributes)
                {
                    if (!multiplyDictionary.ContainsKey(attribute))
                        targetDictionary.Remove(attribute);
                }
                foreach (KeyValuePair<Attribute, float> entry in multiplyDictionary)
                {
                    MultiplyAttributes(targetDictionary, entry);
                }
            }
            else
            {
                targetDictionary.Clear();
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine resistance amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineResistances(Dictionary<DamageElement, float> targetDictionary, Dictionary<DamageElement, float> combineDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            if (combineDictionary != null && combineDictionary.Count > 0)
            {
                foreach (KeyValuePair<DamageElement, float> entry in combineDictionary)
                {
                    CombineResistances(targetDictionary, entry);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine defend amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineArmors(Dictionary<DamageElement, float> targetDictionary, Dictionary<DamageElement, float> combineDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            if (combineDictionary != null && combineDictionary.Count > 0)
            {
                foreach (KeyValuePair<DamageElement, float> entry in combineDictionary)
                {
                    CombineArmors(targetDictionary, entry);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine skill levels dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static Dictionary<BaseSkill, short> CombineSkills(Dictionary<BaseSkill, short> targetDictionary, Dictionary<BaseSkill, short> combineDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<BaseSkill, short>();
            if (combineDictionary != null && combineDictionary.Count > 0)
            {
                foreach (KeyValuePair<BaseSkill, short> entry in combineDictionary)
                {
                    CombineSkills(targetDictionary, entry);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine item amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<Item, short> CombineItems(Dictionary<Item, short> targetDictionary, Dictionary<Item, short> combineDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Item, short>();
            if (combineDictionary != null && combineDictionary.Count > 0)
            {
                foreach (KeyValuePair<Item, short> entry in combineDictionary)
                {
                    CombineItems(targetDictionary, entry);
                }
            }
            return targetDictionary;
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
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
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
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            return new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, (source.amount.GetAmount(level) * rate) + effectiveness);
        }

        /// <summary>
        /// Make damage infliction - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, float> MakeDamageInfliction(DamageInflictionAmount source)
        {
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
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
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            return new KeyValuePair<DamageElement, float>(damageElement, source.rate.GetAmount(level));
        }

        /// <summary>
        /// Make attribute - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<Attribute, float> MakeAttribute(AttributeAmount source, float rate)
        {
            if (source.attribute == null)
                return new KeyValuePair<Attribute, float>();
            return new KeyValuePair<Attribute, float>(source.attribute, source.amount * rate);
        }

        /// <summary>
        /// Make attribute - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<Attribute, float> MakeAttribute(AttributeIncremental source, short level, float rate)
        {
            if (source.attribute == null)
                return new KeyValuePair<Attribute, float>();
            return new KeyValuePair<Attribute, float>(source.attribute, source.amount.GetAmount(level) * rate);
        }

        /// <summary>
        /// Make resistance - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, float> MakeResistance(ResistanceAmount source, float rate)
        {
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            return new KeyValuePair<DamageElement, float>(damageElement, source.amount * rate);
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
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            return new KeyValuePair<DamageElement, float>(damageElement, source.amount.GetAmount(level) * rate);
        }

        /// <summary>
        /// Make armor - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, float> MakeArmor(ArmorAmount source, float rate)
        {
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            return new KeyValuePair<DamageElement, float>(damageElement, source.amount * rate);
        }

        /// <summary>
        /// Make armor - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, float> MakeArmor(ArmorIncremental source, short level, float rate)
        {
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            return new KeyValuePair<DamageElement, float>(damageElement, source.amount.GetAmount(level) * rate);
        }

        /// <summary>
        /// Make skill - level key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static KeyValuePair<BaseSkill, short> MakeSkill(SkillLevel source)
        {
            if (source.skill == null)
                return new KeyValuePair<BaseSkill, short>();
            return new KeyValuePair<BaseSkill, short>(source.skill, source.level);
        }

        /// <summary>
        /// Make skill - level key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static KeyValuePair<BaseSkill, short> MakeSkill(MonsterSkill source)
        {
            if (source.skill == null)
                return new KeyValuePair<BaseSkill, short>();
            return new KeyValuePair<BaseSkill, short>(source.skill, source.level);
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

        #region Combine Dictionary functions
        /// <summary>
        /// Combine damage effectiveness attribute amounts dictionary
        /// </summary>
        /// <param name="sourceEffectivesses"></param>
        /// <param name="targetDictionary"></param>
        /// <returns></returns>
        public static Dictionary<Attribute, float> CombineDamageEffectivenessAttributes(DamageEffectivenessAttribute[] sourceEffectivesses, Dictionary<Attribute, float> targetDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Attribute, float>();
            if (sourceEffectivesses != null)
            {
                foreach (DamageEffectivenessAttribute sourceEffectivess in sourceEffectivesses)
                {
                    if (sourceEffectivess.attribute == null)
                        continue;
                    if (!targetDictionary.ContainsKey(sourceEffectivess.attribute))
                        targetDictionary[sourceEffectivess.attribute] = sourceEffectivess.effectiveness;
                    else
                        targetDictionary[sourceEffectivess.attribute] += sourceEffectivess.effectiveness;
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine damage amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, MinMaxFloat> CombineDamages(DamageAmount[] sourceAmounts, Dictionary<DamageElement, MinMaxFloat> targetDictionary, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, MinMaxFloat>();
            if (sourceAmounts != null)
            {
                KeyValuePair<DamageElement, MinMaxFloat> pair;
                foreach (DamageAmount sourceAmount in sourceAmounts)
                {
                    pair = MakeDamage(sourceAmount, rate, 0f);
                    targetDictionary = CombineDamages(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine damage amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, MinMaxFloat> CombineDamages(DamageIncremental[] sourceIncrementals, Dictionary<DamageElement, MinMaxFloat> targetDictionary, short level, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, MinMaxFloat>();
            if (sourceIncrementals != null)
            {
                KeyValuePair<DamageElement, MinMaxFloat> pair;
                foreach (DamageIncremental sourceIncremental in sourceIncrementals)
                {
                    pair = MakeDamage(sourceIncremental, level, rate, 0f);
                    targetDictionary = CombineDamages(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine damage infliction amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineDamageInflictions(DamageInflictionIncremental[] sourceIncrementals, Dictionary<DamageElement, float> targetDictionary, short level)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            if (sourceIncrementals != null)
            {
                KeyValuePair<DamageElement, float> pair;
                foreach (DamageInflictionIncremental sourceIncremental in sourceIncrementals)
                {
                    pair = MakeDamageInfliction(sourceIncremental, level);
                    targetDictionary = CombineDamageInflictions(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine attribute amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<Attribute, float> CombineAttributes(AttributeAmount[] sourceAmounts, Dictionary<Attribute, float> targetDictionary, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Attribute, float>();
            if (sourceAmounts != null)
            {
                KeyValuePair<Attribute, float> pair;
                foreach (AttributeAmount sourceAmount in sourceAmounts)
                {
                    pair = MakeAttribute(sourceAmount, rate);
                    targetDictionary = CombineAttributes(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine attribute amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<Attribute, float> CombineAttributes(AttributeIncremental[] sourceIncrementals, Dictionary<Attribute, float> targetDictionary, short level, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Attribute, float>();
            if (sourceIncrementals != null)
            {
                KeyValuePair<Attribute, float> pair;
                foreach (AttributeIncremental sourceIncremental in sourceIncrementals)
                {
                    pair = MakeAttribute(sourceIncremental, level, rate);
                    targetDictionary = CombineAttributes(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine resistance amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineResistances(ResistanceAmount[] sourceAmounts, Dictionary<DamageElement, float> targetDictionary, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            if (sourceAmounts != null)
            {
                KeyValuePair<DamageElement, float> pair;
                foreach (ResistanceAmount sourceAmount in sourceAmounts)
                {
                    pair = MakeResistance(sourceAmount, rate);
                    targetDictionary = CombineResistances(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine resistance amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineResistances(ResistanceIncremental[] sourceIncrementals, Dictionary<DamageElement, float> targetDictionary, short level, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            if (sourceIncrementals != null)
            {
                KeyValuePair<DamageElement, float> pair;
                foreach (ResistanceIncremental sourceIncremental in sourceIncrementals)
                {
                    pair = MakeResistance(sourceIncremental, level, rate);
                    targetDictionary = CombineResistances(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }


        /// <summary>
        /// Combine armor amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineArmors(ArmorAmount[] sourceAmounts, Dictionary<DamageElement, float> targetDictionary, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            if (sourceAmounts != null)
            {
                KeyValuePair<DamageElement, float> pair;
                foreach (ArmorAmount sourceAmount in sourceAmounts)
                {
                    pair = MakeArmor(sourceAmount, rate);
                    targetDictionary = CombineArmors(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine armor amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineArmors(ArmorIncremental[] sourceIncrementals, Dictionary<DamageElement, float> targetDictionary, short level, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<DamageElement, float>();
            if (sourceIncrementals != null)
            {
                KeyValuePair<DamageElement, float> pair;
                foreach (ArmorIncremental sourceIncremental in sourceIncrementals)
                {
                    pair = MakeArmor(sourceIncremental, level, rate);
                    targetDictionary = CombineArmors(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine skill levels dictionary
        /// </summary>
        /// <param name="sourceLevels"></param>
        /// <param name="targetDictionary"></param>
        /// <returns></returns>
        public static Dictionary<BaseSkill, short> CombineSkills(SkillLevel[] sourceLevels, Dictionary<BaseSkill, short> targetDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<BaseSkill, short>();
            if (sourceLevels != null)
            {
                KeyValuePair<BaseSkill, short> pair;
                foreach (SkillLevel sourceLevel in sourceLevels)
                {
                    pair = MakeSkill(sourceLevel);
                    targetDictionary = CombineSkills(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine monster skills dictionary
        /// </summary>
        /// <param name="sourceMonsterSkills"></param>
        /// <param name="targetDictionary"></param>
        /// <returns></returns>
        public static Dictionary<BaseSkill, short> CombineSkills(MonsterSkill[] sourceMonsterSkills, Dictionary<BaseSkill, short> targetDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<BaseSkill, short>();
            if (sourceMonsterSkills != null)
            {
                KeyValuePair<BaseSkill, short> pair;
                foreach (MonsterSkill sourceMonsterSkill in sourceMonsterSkills)
                {
                    pair = MakeSkill(sourceMonsterSkill);
                    targetDictionary = CombineSkills(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine item amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="targetDictionary"></param>
        /// <returns></returns>
        public static Dictionary<Item, short> CombineItems(ItemAmount[] sourceAmounts, Dictionary<Item, short> targetDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Item, short>();
            if (sourceAmounts != null)
            {
                KeyValuePair<Item, short> pair;
                foreach (ItemAmount sourceAmount in sourceAmounts)
                {
                    pair = MakeItem(sourceAmount);
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
                Dictionary<Attribute, float> characterAttributes = character.GetAttributes();
                foreach (Attribute attribute in characterAttributes.Keys)
                {
                    if (attribute != null && effectivenessAttributes.ContainsKey(attribute))
                        damageEffectiveness += effectivenessAttributes[attribute] * characterAttributes[attribute];
                }
            }
            return damageEffectiveness;
        }

        public static CharacterStats GetStatsFromAttributes(Dictionary<Attribute, float> attributeAmounts)
        {
            CharacterStats stats = new CharacterStats();
            if (attributeAmounts != null)
            {
                foreach (Attribute attribute in attributeAmounts.Keys)
                {
                    if (attribute == null) continue;
                    stats += attribute.statsIncreaseEachLevel * attributeAmounts[attribute];
                }
            }
            return stats;
        }

        public static MinMaxFloat GetSumDamages(Dictionary<DamageElement, MinMaxFloat> damages)
        {
            MinMaxFloat totalDamageAmount = new MinMaxFloat();
            totalDamageAmount.min = 0;
            totalDamageAmount.max = 0;
            if (damages == null || damages.Count == 0)
                return totalDamageAmount;
            foreach (MinMaxFloat damageAmount in damages.Values)
            {
                totalDamageAmount += damageAmount;
            }
            return totalDamageAmount;
        }
        #endregion
    }
}
