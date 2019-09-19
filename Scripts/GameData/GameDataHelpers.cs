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
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<DamageElement, MinMaxFloat>();
            DamageElement damageElement = newEntry.Key;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
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
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<DamageElement, float>();
            DamageElement damageElement = newEntry.Key;
            if (damageElement == null)
                damageElement = GameInstance.Singleton.DefaultDamageElement;
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
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<Attribute, short>();
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
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<DamageElement, float>();
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
        /// Combine armor amounts dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineArmors(Dictionary<DamageElement, float> sourceDictionary, KeyValuePair<DamageElement, float> newEntry)
        {
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<DamageElement, float>();
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
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<Skill, short>();
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
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<Item, short>();
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
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<DamageElement, MinMaxFloat>();
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
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<DamageElement, float>();
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
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<Attribute, short>();
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
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<DamageElement, float>();
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
        /// Combine defend amounts dictionary
        /// </summary>
        /// <param name="sourceDictionary"></param>
        /// <param name="combineDictionary"></param>
        /// <returns></returns>
        public static Dictionary<DamageElement, float> CombineArmors(Dictionary<DamageElement, float> sourceDictionary, Dictionary<DamageElement, float> combineDictionary)
        {
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<DamageElement, float>();
            if (combineDictionary != null)
            {
                foreach (KeyValuePair<DamageElement, float> entry in combineDictionary)
                {
                    CombineArmors(sourceDictionary, entry);
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
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<Skill, short>();
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
            if (sourceDictionary == null)
                sourceDictionary = new Dictionary<Item, short>();
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
            MinMaxFloat baseDamage = (source.amount.GetAmount(level) * rate) + effectiveness;
            if (damageInflictions != null && damageInflictions.Count > 0)
            {
                foreach (DamageElement element in damageInflictions.Keys)
                {
                    if (element == null) continue;
                    result = CombineDamages(result, new KeyValuePair<DamageElement, MinMaxFloat>(element, baseDamage * damageInflictions[element]));
                }
            }
            else
            {
                if (source.damageElement == null)
                    source.damageElement = GameInstance.Singleton.DefaultDamageElement;
                result = CombineDamages(result, new KeyValuePair<DamageElement, MinMaxFloat>(source.damageElement, baseDamage));
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
        /// Make armor - amount key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static KeyValuePair<DamageElement, float> MakeArmor(ArmorAmount source, float rate)
        {
            if (source.damageElement == null)
                return new KeyValuePair<DamageElement, float>();
            return new KeyValuePair<DamageElement, float>(source.damageElement, source.amount * rate);
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
        /// Make skill - level key-value pair
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static KeyValuePair<Skill, short> MakeSkill(MonsterSkill source)
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
        public static Dictionary<Attribute, short> CombineAttributes(AttributeAmount[] sourceAmounts, Dictionary<Attribute, short> targetDictionary, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Attribute, short>();
            if (sourceAmounts != null)
            {
                KeyValuePair<Attribute, short> pair;
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
        public static Dictionary<Attribute, short> CombineAttributes(AttributeIncremental[] sourceIncrementals, Dictionary<Attribute, short> targetDictionary, short level, float rate)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Attribute, short>();
            if (sourceIncrementals != null)
            {
                KeyValuePair<Attribute, short> pair;
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
        public static Dictionary<Skill, short> CombineSkills(SkillLevel[] sourceLevels, Dictionary<Skill, short> targetDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Skill, short>();
            if (sourceLevels != null)
            {
                KeyValuePair<Skill, short> pair;
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
        public static Dictionary<Skill, short> CombineSkills(MonsterSkill[] sourceMonsterSkills, Dictionary<Skill, short> targetDictionary)
        {
            if (targetDictionary == null)
                targetDictionary = new Dictionary<Skill, short>();
            if (sourceMonsterSkills != null)
            {
                KeyValuePair<Skill, short> pair;
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
                Dictionary<Attribute, short> characterAttributes = character.GetAttributes();
                foreach (Attribute attribute in characterAttributes.Keys)
                {
                    if (attribute != null && effectivenessAttributes.ContainsKey(attribute))
                        damageEffectiveness += effectivenessAttributes[attribute] * characterAttributes[attribute];
                }
            }
            return damageEffectiveness;
        }

        public static CharacterStats GetStatsFromAttributes(Dictionary<Attribute, short> attributeAmounts)
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
