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
        /// Combine currency amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<Currency, int> CombineCurrencies(Dictionary<Currency, int> targetDictionary, KeyValuePair<Currency, int> newEntry)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Currency, int>();
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
                if (targetDictionary[newEntry.Key] > newEntry.Key.MaxResistanceAmount)
                    targetDictionary[newEntry.Key] = newEntry.Key.MaxResistanceAmount;
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
        public static Dictionary<BaseItem, short> CombineItems(Dictionary<BaseItem, short> targetDictionary, KeyValuePair<BaseItem, short> newEntry)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<BaseItem, short>();
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
        /// Combine ammo type amounts dictionary
        /// </summary>
        /// <param name="targetDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<AmmoType, short> CombineAmmoTypes(Dictionary<AmmoType, short> targetDictionary, KeyValuePair<AmmoType, short> newEntry)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<AmmoType, short>();
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
        public static Dictionary<BaseItem, short> CombineItems(Dictionary<BaseItem, short> targetDictionary, Dictionary<BaseItem, short> combineDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<BaseItem, short>();
            if (combineDictionary != null && combineDictionary.Count > 0)
            {
                foreach (KeyValuePair<BaseItem, short> entry in combineDictionary)
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
        public static KeyValuePair<DamageElement, MinMaxFloat> ToKeyValuePair(this DamageAmount source, float rate, float effectiveness)
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
        public static KeyValuePair<DamageElement, MinMaxFloat> ToKeyValuePair(this DamageIncremental source, short level, float rate, float effectiveness)
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
        public static KeyValuePair<DamageElement, float> ToKeyValuePair(this DamageInflictionAmount source)
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
        public static KeyValuePair<DamageElement, float> ToKeyValuePair(this DamageInflictionIncremental source, short level)
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
        public static KeyValuePair<Attribute, float> ToKeyValuePair(this AttributeAmount source, float rate)
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
        public static KeyValuePair<Attribute, float> ToKeyValuePair(this AttributeIncremental source, short level, float rate)
        {
            if (source.attribute == null)
                return new KeyValuePair<Attribute, float>();
            return new KeyValuePair<Attribute, float>(source.attribute, source.amount.GetAmount(level) * rate);
        }

        /// <summary>
        /// Make currency - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static KeyValuePair<Currency, int> ToKeyValuePair(this CurrencyAmount source)
        {
            return new KeyValuePair<Currency, int>(source.currency, source.amount);
        }

        /// <summary>
        /// Make resistance - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, float> ToKeyValuePair(this ResistanceAmount source, float rate)
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
        public static KeyValuePair<DamageElement, float> ToKeyValuePair(this ResistanceIncremental source, short level, float rate)
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
        public static KeyValuePair<DamageElement, float> ToKeyValuePair(this ArmorAmount source, float rate)
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
        public static KeyValuePair<DamageElement, float> ToKeyValuePair(this ArmorIncremental source, short level, float rate)
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
        public static KeyValuePair<BaseSkill, short> ToKeyValuePair(this SkillLevel source)
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
        public static KeyValuePair<BaseSkill, short> ToKeyValuePair(this MonsterSkill source)
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
        public static KeyValuePair<BaseItem, short> ToKeyValuePair(this ItemAmount source)
        {
            if (source.item == null)
                return new KeyValuePair<BaseItem, short>();
            return new KeyValuePair<BaseItem, short>(source.item, source.amount);
        }

        /// <summary>
        /// Make ammo type - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static KeyValuePair<AmmoType, short> ToKeyValuePair(this AmmoTypeAmount source)
        {
            if (source.ammoType == null)
                return new KeyValuePair<AmmoType, short>();
            return new KeyValuePair<AmmoType, short>(source.ammoType, source.amount);
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
            if (sourceEffectivesses != null && sourceEffectivesses.Length > 0)
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
            if (sourceAmounts != null && sourceAmounts.Length > 0)
            {
                KeyValuePair<DamageElement, MinMaxFloat> pair;
                foreach (DamageAmount sourceAmount in sourceAmounts)
                {
                    pair = ToKeyValuePair(sourceAmount, rate, 0f);
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
            if (sourceIncrementals != null && sourceIncrementals.Length > 0)
            {
                KeyValuePair<DamageElement, MinMaxFloat> pair;
                foreach (DamageIncremental sourceIncremental in sourceIncrementals)
                {
                    pair = ToKeyValuePair(sourceIncremental, level, rate, 0f);
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
            if (sourceIncrementals != null && sourceIncrementals.Length > 0)
            {
                KeyValuePair<DamageElement, float> pair;
                foreach (DamageInflictionIncremental sourceIncremental in sourceIncrementals)
                {
                    pair = ToKeyValuePair(sourceIncremental, level);
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
            if (sourceAmounts != null && sourceAmounts.Length > 0)
            {
                KeyValuePair<Attribute, float> pair;
                foreach (AttributeAmount sourceAmount in sourceAmounts)
                {
                    pair = ToKeyValuePair(sourceAmount, rate);
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
            if (sourceIncrementals != null && sourceIncrementals.Length > 0)
            {
                KeyValuePair<Attribute, float> pair;
                foreach (AttributeIncremental sourceIncremental in sourceIncrementals)
                {
                    pair = ToKeyValuePair(sourceIncremental, level, rate);
                    targetDictionary = CombineAttributes(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine currency amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="targetDictionary"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static Dictionary<Currency, int> CombineCurrencies(CurrencyAmount[] sourceAmounts, Dictionary<Currency, int> targetDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Currency, int>();
            if (sourceAmounts != null && sourceAmounts.Length > 0)
            {
                KeyValuePair<Currency, int> pair;
                foreach (CurrencyAmount sourceAmount in sourceAmounts)
                {
                    pair = ToKeyValuePair(sourceAmount);
                    targetDictionary = CombineCurrencies(targetDictionary, pair);
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
            if (sourceAmounts != null && sourceAmounts.Length > 0)
            {
                KeyValuePair<DamageElement, float> pair;
                foreach (ResistanceAmount sourceAmount in sourceAmounts)
                {
                    pair = ToKeyValuePair(sourceAmount, rate);
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
            if (sourceIncrementals != null && sourceIncrementals.Length > 0)
            {
                KeyValuePair<DamageElement, float> pair;
                foreach (ResistanceIncremental sourceIncremental in sourceIncrementals)
                {
                    pair = ToKeyValuePair(sourceIncremental, level, rate);
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
            if (sourceAmounts != null && sourceAmounts.Length > 0)
            {
                KeyValuePair<DamageElement, float> pair;
                foreach (ArmorAmount sourceAmount in sourceAmounts)
                {
                    pair = ToKeyValuePair(sourceAmount, rate);
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
            if (sourceIncrementals != null && sourceIncrementals.Length > 0)
            {
                KeyValuePair<DamageElement, float> pair;
                foreach (ArmorIncremental sourceIncremental in sourceIncrementals)
                {
                    pair = ToKeyValuePair(sourceIncremental, level, rate);
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
            if (sourceLevels != null && sourceLevels.Length > 0)
            {
                KeyValuePair<BaseSkill, short> pair;
                foreach (SkillLevel sourceLevel in sourceLevels)
                {
                    pair = ToKeyValuePair(sourceLevel);
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
            if (sourceMonsterSkills != null && sourceMonsterSkills.Length > 0)
            {
                KeyValuePair<BaseSkill, short> pair;
                foreach (MonsterSkill sourceMonsterSkill in sourceMonsterSkills)
                {
                    pair = ToKeyValuePair(sourceMonsterSkill);
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
        public static Dictionary<BaseItem, short> CombineItems(ItemAmount[] sourceAmounts, Dictionary<BaseItem, short> targetDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<BaseItem, short>();
            if (sourceAmounts != null && sourceAmounts.Length > 0)
            {
                KeyValuePair<BaseItem, short> pair;
                foreach (ItemAmount sourceAmount in sourceAmounts)
                {
                    pair = ToKeyValuePair(sourceAmount);
                    targetDictionary = CombineItems(targetDictionary, pair);
                }
            }
            return targetDictionary;
        }

        /// <summary>
        /// Combine ammo type amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="targetDictionary"></param>
        /// <returns></returns>
        public static Dictionary<AmmoType, short> CombineAmmoTypes(AmmoTypeAmount[] sourceAmounts, Dictionary<AmmoType, short> targetDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<AmmoType, short>();
            if (sourceAmounts != null && sourceAmounts.Length > 0)
            {
                KeyValuePair<AmmoType, short> pair;
                foreach (AmmoTypeAmount sourceAmount in sourceAmounts)
                {
                    pair = ToKeyValuePair(sourceAmount);
                    targetDictionary = CombineAmmoTypes(targetDictionary, pair);
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
                Dictionary<Attribute, float> characterAttributes = character.GetCaches().Attributes;
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
            if (attributeAmounts != null && attributeAmounts.Count > 0)
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
