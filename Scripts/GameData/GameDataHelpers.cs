using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MultiplayerARPG
{
    public static class GameDataHelpers
    {
        #region Combine Dictionary with KeyValuePair functions
        /// <summary>
        /// Combine damage amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static void CombineDamages(Dictionary<DamageElement, MinMaxFloat> resultDictionary, KeyValuePair<DamageElement, MinMaxFloat> newEntry, float rate = 1f)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            DamageElement damageElement = newEntry.Key;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            if (!resultDictionary.ContainsKey(damageElement))
                resultDictionary[damageElement] = newEntry.Value * rate;
            else
                resultDictionary[damageElement] += newEntry.Value * rate;
            return;
        }

        /// <summary>
        /// Multiply damage amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="multiplyEntry"></param>
        /// <returns></returns>
        public static void MultiplyDamages(Dictionary<DamageElement, MinMaxFloat> resultDictionary, KeyValuePair<DamageElement, MinMaxFloat> multiplyEntry)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (multiplyEntry.Key != null && resultDictionary.ContainsKey(multiplyEntry.Key))
                resultDictionary[multiplyEntry.Key] *= multiplyEntry.Value;
            return;
        }

        /// <summary>
        /// Combine damage infliction amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static void CombineDamageInflictions(Dictionary<DamageElement, float> resultDictionary, KeyValuePair<DamageElement, float> newEntry)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            DamageElement damageElement = newEntry.Key;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            if (!resultDictionary.ContainsKey(damageElement))
                resultDictionary[damageElement] = newEntry.Value;
            else
                resultDictionary[damageElement] += newEntry.Value;
            return;
        }

        /// <summary>
        /// Combine attribute amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static void CombineAttributes(Dictionary<Attribute, float> resultDictionary, KeyValuePair<Attribute, float> newEntry)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (newEntry.Key == null)
                return;
            if (!resultDictionary.ContainsKey(newEntry.Key))
                resultDictionary[newEntry.Key] = newEntry.Value;
            else
                resultDictionary[newEntry.Key] += newEntry.Value;
            return;
        }

        /// <summary>
        /// Multiply attribute amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="multiplyEntry"></param>
        /// <returns></returns>
        public static void MultiplyAttributes(Dictionary<Attribute, float> resultDictionary, KeyValuePair<Attribute, float> multiplyEntry)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (multiplyEntry.Key != null && resultDictionary.ContainsKey(multiplyEntry.Key))
                resultDictionary[multiplyEntry.Key] *= multiplyEntry.Value;
            return;
        }

        /// <summary>
        /// Combine currency amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static void CombineCurrencies(Dictionary<Currency, int> resultDictionary, KeyValuePair<Currency, int> newEntry)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (newEntry.Key == null)
                return;
            if (!resultDictionary.ContainsKey(newEntry.Key))
                resultDictionary[newEntry.Key] = newEntry.Value;
            else
                resultDictionary[newEntry.Key] += newEntry.Value;
            return;
        }

        /// <summary>
        /// Combine resistance amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static void CombineResistances(Dictionary<DamageElement, float> resultDictionary, KeyValuePair<DamageElement, float> newEntry)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (newEntry.Key == null)
                return;
            if (!resultDictionary.ContainsKey(newEntry.Key))
                resultDictionary[newEntry.Key] = newEntry.Value;
            else
                resultDictionary[newEntry.Key] += newEntry.Value;
            return;
        }

        /// <summary>
        /// Combine armor amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static void CombineArmors(Dictionary<DamageElement, float> resultDictionary, KeyValuePair<DamageElement, float> newEntry)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (newEntry.Key == null)
                return;
            if (!resultDictionary.ContainsKey(newEntry.Key))
                resultDictionary[newEntry.Key] = newEntry.Value;
            else
                resultDictionary[newEntry.Key] += newEntry.Value;
            return;
        }

        /// <summary>
        /// Multiply armors amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="multiplyEntry"></param>
        /// <returns></returns>
        public static void MultiplyArmors(Dictionary<DamageElement, float> resultDictionary, KeyValuePair<DamageElement, float> multiplyEntry)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (multiplyEntry.Key != null && resultDictionary.ContainsKey(multiplyEntry.Key))
                resultDictionary[multiplyEntry.Key] *= multiplyEntry.Value;
            return;
        }

        /// <summary>
        /// Combine skill levels dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static void CombineSkills(Dictionary<BaseSkill, int> resultDictionary, KeyValuePair<BaseSkill, int> newEntry)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (newEntry.Key == null)
                return;
            if (!resultDictionary.ContainsKey(newEntry.Key))
                resultDictionary[newEntry.Key] = newEntry.Value;
            else
                resultDictionary[newEntry.Key] += newEntry.Value;
            return;
        }

        /// <summary>
        /// Combine status effect resistance amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static void CombineStatusEffectResistances(Dictionary<StatusEffect, float> resultDictionary, KeyValuePair<StatusEffect, float> newEntry)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (newEntry.Key == null)
                return;
            if (!resultDictionary.ContainsKey(newEntry.Key))
                resultDictionary[newEntry.Key] = newEntry.Value;
            else
                resultDictionary[newEntry.Key] += newEntry.Value;
            return;
        }

        /// <summary>
        /// Combine buff removals dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static void CombineBuffRemovals(Dictionary<BuffRemoval, float> resultDictionary, KeyValuePair<BuffRemoval, float> newEntry)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (newEntry.Key == null)
                return;
            if (!resultDictionary.ContainsKey(newEntry.Key))
                resultDictionary[newEntry.Key] = newEntry.Value;
            else
                resultDictionary[newEntry.Key] += newEntry.Value;
            return;
        }

        /// <summary>
        /// Combine item amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static void CombineItems(Dictionary<BaseItem, int> resultDictionary, KeyValuePair<BaseItem, int> newEntry)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (newEntry.Key == null)
                return;
            if (!resultDictionary.ContainsKey(newEntry.Key))
                resultDictionary[newEntry.Key] = newEntry.Value;
            else
                resultDictionary[newEntry.Key] += newEntry.Value;
            return;
        }

        /// <summary>
        /// Combine ammo type amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static void CombineAmmoTypes(Dictionary<AmmoType, int> resultDictionary, KeyValuePair<AmmoType, int> newEntry)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (newEntry.Key == null)
                return;
            if (!resultDictionary.ContainsKey(newEntry.Key))
                resultDictionary[newEntry.Key] = newEntry.Value;
            else
                resultDictionary[newEntry.Key] += newEntry.Value;
            return;
        }
        #endregion

        #region Combine Dictionary with Dictionary functions
        /// <summary>
        /// Combine damage amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineDamages(Dictionary<DamageElement, MinMaxFloat> resultDictionary, Dictionary<DamageElement, MinMaxFloat> combineDictionary, float rate = 1f)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (combineDictionary == null || combineDictionary.Count <= 0)
                return;
            foreach (KeyValuePair<DamageElement, MinMaxFloat> entry in combineDictionary)
            {
                CombineDamages(resultDictionary, entry, rate);
            }
            return;
        }

        /// <summary>
        /// Multiply damage amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="multiplyDictionary"></param>
        /// <returns></returns>
        public static void MultiplyDamages(Dictionary<DamageElement, MinMaxFloat> resultDictionary, Dictionary<DamageElement, MinMaxFloat> multiplyDictionary)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (multiplyDictionary != null && multiplyDictionary.Count > 0)
            {
                // Remove attributes that are not multiplying
                using (CollectionPool<List<DamageElement>, DamageElement>.Get(out List<DamageElement> availableDamages))
                {
                    availableDamages.AddRange(resultDictionary.Keys);
                    foreach (DamageElement damage in availableDamages)
                    {
                        if (!multiplyDictionary.ContainsKey(damage))
                            resultDictionary.Remove(damage);
                    }
                }
                foreach (KeyValuePair<DamageElement, MinMaxFloat> entry in multiplyDictionary)
                {
                    MultiplyDamages(resultDictionary, entry);
                }
            }
            else
            {
                resultDictionary.Clear();
            }
            return;
        }

        /// <summary>
        /// Combine damage infliction amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static void CombineDamageInflictions(Dictionary<DamageElement, float> resultDictionary, Dictionary<DamageElement, float> combineDictionary)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (combineDictionary == null || combineDictionary.Count <= 0)
                return;
            foreach (KeyValuePair<DamageElement, float> entry in combineDictionary)
            {
                CombineDamageInflictions(resultDictionary, entry);
            }
            return;
        }

        /// <summary>
        /// Combine attribute amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static void CombineAttributes(Dictionary<Attribute, float> resultDictionary, Dictionary<Attribute, float> combineDictionary)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (combineDictionary == null || combineDictionary.Count <= 0)
                return;
            foreach (KeyValuePair<Attribute, float> entry in combineDictionary)
            {
                CombineAttributes(resultDictionary, entry);
            }
            return;
        }

        /// <summary>
        /// Multiply attribute amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="multiplyDictionary"></param>
        /// <returns></returns>
        public static void MultiplyAttributes(Dictionary<Attribute, float> resultDictionary, Dictionary<Attribute, float> multiplyDictionary)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (multiplyDictionary != null && multiplyDictionary.Count > 0)
            {
                // Remove attributes that are not multiplying
                using (CollectionPool<List<Attribute>, Attribute>.Get(out List<Attribute> availableAttributes))
                {
                    availableAttributes.AddRange(resultDictionary.Keys);
                    foreach (Attribute attribute in availableAttributes)
                    {
                        if (!multiplyDictionary.ContainsKey(attribute))
                            resultDictionary.Remove(attribute);
                    }
                }
                foreach (KeyValuePair<Attribute, float> entry in multiplyDictionary)
                {
                    MultiplyAttributes(resultDictionary, entry);
                }
            }
            else
            {
                resultDictionary.Clear();
            }
            return;
        }

        /// <summary>
        /// Combine resistance amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static void CombineResistances(Dictionary<DamageElement, float> resultDictionary, Dictionary<DamageElement, float> combineDictionary)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (combineDictionary == null || combineDictionary.Count <= 0)
                return;
            foreach (KeyValuePair<DamageElement, float> entry in combineDictionary)
            {
                CombineResistances(resultDictionary, entry);
            }
            return;
        }

        /// <summary>
        /// Combine defend amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static void CombineArmors(Dictionary<DamageElement, float> resultDictionary, Dictionary<DamageElement, float> combineDictionary)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (combineDictionary == null || combineDictionary.Count <= 0)
                return;
            foreach (KeyValuePair<DamageElement, float> entry in combineDictionary)
            {
                CombineArmors(resultDictionary, entry);
            }
            return;
        }

        /// <summary>
        /// Multiply armors amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="multiplyDictionary"></param>
        /// <returns></returns>
        public static void MultiplyArmors(Dictionary<DamageElement, float> resultDictionary, Dictionary<DamageElement, float> multiplyDictionary)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (multiplyDictionary != null && multiplyDictionary.Count > 0)
            {
                // Remove attributes that are not multiplying
                using (CollectionPool<List<DamageElement>, DamageElement>.Get(out List<DamageElement> availableArmors))
                {
                    availableArmors.AddRange(resultDictionary.Keys);
                    foreach (DamageElement armor in availableArmors)
                    {
                        if (!multiplyDictionary.ContainsKey(armor))
                            resultDictionary.Remove(armor);
                    }
                }
                foreach (KeyValuePair<DamageElement, float> entry in multiplyDictionary)
                {
                    MultiplyArmors(resultDictionary, entry);
                }
            }
            else
            {
                resultDictionary.Clear();
            }
            return;
        }

        /// <summary>
        /// Combine skill levels dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static void CombineSkills(Dictionary<BaseSkill, int> resultDictionary, Dictionary<BaseSkill, int> combineDictionary)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (combineDictionary == null || combineDictionary.Count <= 0)
                return;
            foreach (KeyValuePair<BaseSkill, int> entry in combineDictionary)
            {
                CombineSkills(resultDictionary, entry);
            }
            return;
        }

        /// <summary>
        /// Combine status effect resistance amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static void CombineStatusEffectResistances(Dictionary<StatusEffect, float> resultDictionary, Dictionary<StatusEffect, float> combineDictionary)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (combineDictionary == null || combineDictionary.Count <= 0)
                return;
            foreach (KeyValuePair<StatusEffect, float> entry in combineDictionary)
            {
                CombineStatusEffectResistances(resultDictionary, entry);
            }
            return;
        }

        /// <summary>
        /// Combine item amounts dictionary
        /// </summary>
        /// <param name="resultDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static void CombineItems(Dictionary<BaseItem, int> resultDictionary, Dictionary<BaseItem, int> combineDictionary)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (combineDictionary == null || combineDictionary.Count <= 0)
                return;
            foreach (KeyValuePair<BaseItem, int> entry in combineDictionary)
            {
                CombineItems(resultDictionary, entry);
            }
            return;
        }
        #endregion

        #region Make KeyValuePair functions
        /// <summary>
        /// Make damage - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, MinMaxFloat> ToKeyValuePair(this DamageAmount source, float rate)
        {
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            return new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, source.amount * rate);
        }

        /// <summary>
        /// Make damage amount
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, MinMaxFloat> ToKeyValuePair(this DamageIncremental source, int level, float rate)
        {
            DamageElement damageElement = source.damageElement;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            return new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, source.amount.GetAmount(level) * rate);
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
        public static KeyValuePair<DamageElement, float> ToKeyValuePair(this DamageInflictionIncremental source, int level)
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
        public static KeyValuePair<Attribute, float> ToKeyValuePair(this AttributeIncremental source, int level, float rate)
        {
            if (source.attribute == null)
                return new KeyValuePair<Attribute, float>();
            return new KeyValuePair<Attribute, float>(source.attribute, source.amount.GetAmount(level) * rate);
        }

        /// <summary>
        /// Make currency - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<Currency, int> ToKeyValuePair(this CurrencyAmount source, float rate)
        {
            return new KeyValuePair<Currency, int>(source.currency, Mathf.CeilToInt(source.amount * rate));
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
        public static KeyValuePair<DamageElement, float> ToKeyValuePair(this ResistanceIncremental source, int level, float rate)
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
        public static KeyValuePair<DamageElement, float> ToKeyValuePair(this ArmorIncremental source, int level, float rate)
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
        public static KeyValuePair<BaseSkill, int> ToKeyValuePair(this SkillLevel source, float rate)
        {
            if (source.skill == null)
                return new KeyValuePair<BaseSkill, int>();
            return new KeyValuePair<BaseSkill, int>(source.skill, Mathf.CeilToInt(source.level * rate));
        }

        /// <summary>
        /// Make skill - level key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static KeyValuePair<BaseSkill, int> ToKeyValuePair(this SkillIncremental source, int level, float rate)
        {
            if (source.skill == null)
                return new KeyValuePair<BaseSkill, int>();
            return new KeyValuePair<BaseSkill, int>(source.skill, Mathf.CeilToInt(source.level.GetAmount(level) * rate));
        }

        /// <summary>
        /// Make skill - level key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="characterLevel"></param>
        /// <returns></returns>
        public static KeyValuePair<BaseSkill, int> ToKeyValuePair(this PlayerSkill source, int characterLevel)
        {
            if (source.skill == null)
                return new KeyValuePair<BaseSkill, int>();
            return new KeyValuePair<BaseSkill, int>(source.skill, source.skillLevel.GetAmount(characterLevel));
        }

        /// <summary>
        /// Make skill - level key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="characterLevel"></param>
        /// <returns></returns>
        public static KeyValuePair<BaseSkill, int> ToKeyValuePair(this MonsterSkill source, int characterLevel)
        {
            if (source.skill == null)
                return new KeyValuePair<BaseSkill, int>();
            return new KeyValuePair<BaseSkill, int>(source.skill, source.skillLevel.GetAmount(characterLevel));
        }

        /// <summary>
        /// Make status effect resistance - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<StatusEffect, float> ToKeyValuePair(this StatusEffectResistanceAmount source, float rate)
        {
            if (source.statusEffect == null)
                return new KeyValuePair<StatusEffect, float>();
            return new KeyValuePair<StatusEffect, float>(source.statusEffect, source.amount * rate);
        }

        /// <summary>
        /// Make status effect resistance - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<StatusEffect, float> ToKeyValuePair(this StatusEffectResistanceIncremental source, int level, float rate)
        {
            if (source.statusEffect == null)
                return new KeyValuePair<StatusEffect, float>();
            return new KeyValuePair<StatusEffect, float>(source.statusEffect, source.amount.GetAmount(level) * rate);
        }

        /// <summary>
        /// Make buff removal - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<BuffRemoval, float> ToKeyValuePair(this BuffRemoval source, int level, float rate)
        {
            if (source == null)
                return new KeyValuePair<BuffRemoval, float>();
            return new KeyValuePair<BuffRemoval, float>(source, source.removalChance.GetAmount(level) * rate);
        }

        /// <summary>
        /// Make item - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static KeyValuePair<BaseItem, int> ToKeyValuePair(this ItemAmount source)
        {
            if (source.item == null)
                return new KeyValuePair<BaseItem, int>();
            return new KeyValuePair<BaseItem, int>(source.item, source.amount);
        }

        /// <summary>
        /// Make ammo type - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static KeyValuePair<AmmoType, int> ToKeyValuePair(this AmmoTypeAmount source)
        {
            if (source.ammoType == null)
                return new KeyValuePair<AmmoType, int>();
            return new KeyValuePair<AmmoType, int>(source.ammoType, source.amount);
        }
        #endregion

        #region Combine Dictionary functions
        /// <summary>
        /// Combine damage effectiveness attribute amounts dictionary
        /// </summary>
        /// <param name="sourceEffectivesses"></param>
        /// <param name="resultDictionary"></param>
        /// <returns></returns>
        public static void CombineDamageEffectivenessAttributes(IEnumerable<DamageEffectivenessAttribute> sourceEffectivesses, Dictionary<Attribute, float> resultDictionary)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceEffectivesses == null)
                return;
            foreach (DamageEffectivenessAttribute sourceEffectivess in sourceEffectivesses)
            {
                if (sourceEffectivess.attribute == null)
                    continue;
                if (!resultDictionary.ContainsKey(sourceEffectivess.attribute))
                    resultDictionary[sourceEffectivess.attribute] = sourceEffectivess.effectiveness;
                else
                    resultDictionary[sourceEffectivess.attribute] += sourceEffectivess.effectiveness;
            }
            return;
        }

        /// <summary>
        /// Combine damage amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineDamages(IEnumerable<DamageAmount> sourceAmounts, Dictionary<DamageElement, MinMaxFloat> resultDictionary, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceAmounts == null)
                return;
            KeyValuePair<DamageElement, MinMaxFloat> pair;
            foreach (DamageAmount sourceAmount in sourceAmounts)
            {
                pair = ToKeyValuePair(sourceAmount, rate);
                CombineDamages(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine damage amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineDamages(IEnumerable<DamageIncremental> sourceIncrementals, Dictionary<DamageElement, MinMaxFloat> resultDictionary, int level, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceIncrementals == null)
                return;
            KeyValuePair<DamageElement, MinMaxFloat> pair;
            foreach (DamageIncremental sourceIncremental in sourceIncrementals)
            {
                pair = ToKeyValuePair(sourceIncremental, level, rate);
                CombineDamages(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine damage infliction amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static void CombineDamageInflictions(IEnumerable<DamageInflictionIncremental> sourceIncrementals, Dictionary<DamageElement, float> resultDictionary, int level)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceIncrementals == null)
                return;
            KeyValuePair<DamageElement, float> pair;
            foreach (DamageInflictionIncremental sourceIncremental in sourceIncrementals)
            {
                pair = ToKeyValuePair(sourceIncremental, level);
                CombineDamageInflictions(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine attribute amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineAttributes(IEnumerable<AttributeAmount> sourceAmounts, Dictionary<Attribute, float> resultDictionary, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceAmounts == null)
                return;
            KeyValuePair<Attribute, float> pair;
            foreach (AttributeAmount sourceAmount in sourceAmounts)
            {
                pair = ToKeyValuePair(sourceAmount, rate);
                CombineAttributes(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine attribute amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineAttributes(IEnumerable<AttributeIncremental> sourceIncrementals, Dictionary<Attribute, float> resultDictionary, int level, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceIncrementals == null)
                return;
            KeyValuePair<Attribute, float> pair;
            foreach (AttributeIncremental sourceIncremental in sourceIncrementals)
            {
                pair = ToKeyValuePair(sourceIncremental, level, rate);
                CombineAttributes(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine currency amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineCurrencies(IEnumerable<CurrencyAmount> sourceAmounts, Dictionary<Currency, int> resultDictionary, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceAmounts == null)
                return;
            KeyValuePair<Currency, int> pair;
            foreach (CurrencyAmount sourceAmount in sourceAmounts)
            {
                pair = ToKeyValuePair(sourceAmount, rate);
                CombineCurrencies(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine resistance amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineResistances(IEnumerable<ResistanceAmount> sourceAmounts, Dictionary<DamageElement, float> resultDictionary, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceAmounts == null)
                return;
            KeyValuePair<DamageElement, float> pair;
            foreach (ResistanceAmount sourceAmount in sourceAmounts)
            {
                pair = ToKeyValuePair(sourceAmount, rate);
                CombineResistances(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine resistance amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineResistances(IEnumerable<ResistanceIncremental> sourceIncrementals, Dictionary<DamageElement, float> resultDictionary, int level, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceIncrementals == null)
                return;
            KeyValuePair<DamageElement, float> pair;
            foreach (ResistanceIncremental sourceIncremental in sourceIncrementals)
            {
                pair = ToKeyValuePair(sourceIncremental, level, rate);
                CombineResistances(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine armor amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineArmors(IEnumerable<ArmorAmount> sourceAmounts, Dictionary<DamageElement, float> resultDictionary, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceAmounts == null)
                return;
            KeyValuePair<DamageElement, float> pair;
            foreach (ArmorAmount sourceAmount in sourceAmounts)
            {
                pair = ToKeyValuePair(sourceAmount, rate);
                CombineArmors(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine armor amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineArmors(IEnumerable<ArmorIncremental> sourceIncrementals, Dictionary<DamageElement, float> resultDictionary, int level, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceIncrementals == null)
                return;
            KeyValuePair<DamageElement, float> pair;
            foreach (ArmorIncremental sourceIncremental in sourceIncrementals)
            {
                pair = ToKeyValuePair(sourceIncremental, level, rate);
                CombineArmors(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine skill levels dictionary
        /// </summary>
        /// <param name="sourceLevels"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineSkills(IEnumerable<SkillLevel> sourceLevels, Dictionary<BaseSkill, int> resultDictionary, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceLevels == null)
                return;
            KeyValuePair<BaseSkill, int> pair;
            foreach (SkillLevel sourceLevel in sourceLevels)
            {
                pair = ToKeyValuePair(sourceLevel, rate);
                CombineSkills(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine skill level incrementals dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineSkills(IEnumerable<SkillIncremental> sourceIncrementals, Dictionary<BaseSkill, int> resultDictionary, int level, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceIncrementals == null)
                return;
            KeyValuePair<BaseSkill, int> pair;
            foreach (SkillIncremental sourceIncremental in sourceIncrementals)
            {
                pair = ToKeyValuePair(sourceIncremental, level, rate);
                CombineSkills(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine player skills dictionary
        /// </summary>
        /// <param name="sourcePlayerSkills"></param>
        /// <param name="resultDictionary"></param>
        /// <returns></returns>
        public static void CombineSkills(IEnumerable<PlayerSkill> sourcePlayerSkills, Dictionary<BaseSkill, int> resultDictionary, int characterLevel)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourcePlayerSkills == null)
                return;
            KeyValuePair<BaseSkill, int> pair;
            foreach (PlayerSkill sourcePlayerSkill in sourcePlayerSkills)
            {
                pair = ToKeyValuePair(sourcePlayerSkill, characterLevel);
                CombineSkills(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine monster skills dictionary
        /// </summary>
        /// <param name="sourceMonsterSkills"></param>
        /// <param name="resultDictionary"></param>
        /// <returns></returns>
        public static void CombineSkills(IEnumerable<MonsterSkill> sourceMonsterSkills, Dictionary<BaseSkill, int> resultDictionary, int characterLevel)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceMonsterSkills == null)
                return;
            KeyValuePair<BaseSkill, int> pair;
            foreach (MonsterSkill sourceMonsterSkill in sourceMonsterSkills)
            {
                pair = ToKeyValuePair(sourceMonsterSkill, characterLevel);
                CombineSkills(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine status effect resistance amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineStatusEffectResistances(IEnumerable<StatusEffectResistanceAmount> sourceAmounts, Dictionary<StatusEffect, float> resultDictionary, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceAmounts == null)
                return;
            KeyValuePair<StatusEffect, float> pair;
            foreach (StatusEffectResistanceAmount sourceAmount in sourceAmounts)
            {
                pair = ToKeyValuePair(sourceAmount, rate);
                CombineStatusEffectResistances(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine status effect resistance amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineStatusEffectResistances(IEnumerable<StatusEffectResistanceIncremental> sourceIncrementals, Dictionary<StatusEffect, float> resultDictionary, int level, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceIncrementals == null)
                return;
            KeyValuePair<StatusEffect, float> pair;
            foreach (StatusEffectResistanceIncremental sourceIncremental in sourceIncrementals)
            {
                pair = ToKeyValuePair(sourceIncremental, level, rate);
                CombineStatusEffectResistances(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine buff removals dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineBuffRemovals(IEnumerable<BuffRemoval> sourceAmounts, Dictionary<BuffRemoval, float> resultDictionary, int level, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceAmounts == null)
                return;
            KeyValuePair<BuffRemoval, float> pair;
            foreach (BuffRemoval sourceAmount in sourceAmounts)
            {
                pair = ToKeyValuePair(sourceAmount, level, rate);
                CombineBuffRemovals(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine item amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="resultDictionary"></param>
        /// <returns></returns>
        public static void CombineItems(IEnumerable<ItemAmount> sourceAmounts, Dictionary<BaseItem, int> resultDictionary)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceAmounts == null)
                return;
            KeyValuePair<BaseItem, int> pair;
            foreach (ItemAmount sourceAmount in sourceAmounts)
            {
                pair = ToKeyValuePair(sourceAmount);
                CombineItems(resultDictionary, pair);
            }
            return;
        }

        /// <summary>
        /// Combine ammo type amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="resultDictionary"></param>
        /// <returns></returns>
        public static void CombineAmmoTypes(IEnumerable<AmmoTypeAmount> sourceAmounts, Dictionary<AmmoType, int> resultDictionary)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceAmounts == null)
                return;
            KeyValuePair<AmmoType, int> pair;
            foreach (AmmoTypeAmount sourceAmount in sourceAmounts)
            {
                pair = ToKeyValuePair(sourceAmount);
                CombineAmmoTypes(resultDictionary, pair);
            }
            return;
        }
        #endregion

        #region Calculate functions
        public static float GetEffectivenessDamage(Dictionary<Attribute, float> effectivenessAttributes, Dictionary<Attribute, float> characterAttributes)
        {
            float damageEffectiveness = 0f;
            if (effectivenessAttributes == null || characterAttributes == null)
                return damageEffectiveness;
            foreach (Attribute attribute in characterAttributes.Keys)
            {
                if (attribute != null && effectivenessAttributes.ContainsKey(attribute))
                    damageEffectiveness += effectivenessAttributes[attribute] * characterAttributes[attribute];
            }
            return damageEffectiveness;
        }

        public static KeyValuePair<DamageElement, MinMaxFloat> GetDamageWithEffectiveness(Dictionary<Attribute, float> effectivenessAttributes, Dictionary<Attribute, float> characterAttributes, KeyValuePair<DamageElement, MinMaxFloat> pureDamage)
        {
            float damageEffectiveness = GetEffectivenessDamage(effectivenessAttributes, characterAttributes);
            DamageElement damageElement = pureDamage.Key;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
            return new KeyValuePair<DamageElement, MinMaxFloat>(damageElement, pureDamage.Value + damageEffectiveness);
        }

        public static MinMaxFloat GetSumDamages(Dictionary<DamageElement, MinMaxFloat> damages)
        {
            MinMaxFloat totalDamageAmount = new MinMaxFloat()
            {
                min = 0,
                max = 0,
            };
            if (damages == null || damages.Count <= 0)
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