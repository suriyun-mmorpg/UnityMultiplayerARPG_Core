using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static partial class GameDataHelpers
    {
        public static void CombineDamages(List<DamageAmount> sourceAmounts, ref DamageElementMinMaxFloatAmounts result, float rate)
        {
            if (sourceAmounts == null)
                return;
            foreach (DamageAmount sourceAmount in sourceAmounts)
            {
                DamageElement element = sourceAmount.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceAmount.damageElement;
                result.Add(RuntimeGameDataSlots.GetSlot(element), sourceAmount.amount * rate);
            }
        }

        public static void CombineDamages(List<DamageIncremental> sourceIncrementals, ref DamageElementMinMaxFloatAmounts result, int level, float rate)
        {
            if (sourceIncrementals == null)
                return;
            foreach (DamageIncremental sourceIncremental in sourceIncrementals)
            {
                DamageElement element = sourceIncremental.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceIncremental.damageElement;
                result.Add(RuntimeGameDataSlots.GetSlot(element), sourceIncremental.amount.GetAmount(level) * rate);
            }
        }

        public static void CombineDamageInflictions(List<DamageInflictionIncremental> sourceIncrementals, ref DamageElementFloatAmounts result, int level)
        {
            if (sourceIncrementals == null)
                return;
            foreach (DamageInflictionIncremental sourceIncremental in sourceIncrementals)
            {
                DamageElement element = sourceIncremental.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceIncremental.damageElement;
                result.Add(RuntimeGameDataSlots.GetSlot(element), sourceIncremental.rate.GetAmount(level));
            }
        }

        public static void CombineAttributes(List<AttributeAmount> sourceAmounts, ref AttributeAmounts result, float rate)
        {
            if (sourceAmounts == null)
                return;
            foreach (AttributeAmount sourceAmount in sourceAmounts)
            {
                Attribute attribute = sourceAmount.attribute;
                if (attribute != null)
                    result.Add(RuntimeGameDataSlots.GetSlot(attribute), sourceAmount.amount * rate);
            }
        }

        public static void CombineAttributes(List<AttributeIncremental> sourceIncrementals, ref AttributeAmounts result, int level, float rate)
        {
            if (sourceIncrementals == null)
                return;
            foreach (AttributeIncremental sourceIncremental in sourceIncrementals)
            {
                Attribute attribute = sourceIncremental.attribute;
                if (attribute != null)
                    result.Add(RuntimeGameDataSlots.GetSlot(attribute), sourceIncremental.amount.GetAmount(level) * rate);
            }
        }

        public static void CombineResistances(List<ResistanceAmount> sourceAmounts, ref DamageElementFloatAmounts result, float rate)
        {
            if (sourceAmounts == null)
                return;
            foreach (ResistanceAmount sourceAmount in sourceAmounts)
            {
                DamageElement element = sourceAmount.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceAmount.damageElement;
                result.Add(RuntimeGameDataSlots.GetSlot(element), sourceAmount.amount * rate);
            }
        }

        public static void CombineResistances(List<ResistanceIncremental> sourceIncrementals, ref DamageElementFloatAmounts result, int level, float rate)
        {
            if (sourceIncrementals == null)
                return;
            foreach (ResistanceIncremental sourceIncremental in sourceIncrementals)
            {
                DamageElement element = sourceIncremental.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceIncremental.damageElement;
                result.Add(RuntimeGameDataSlots.GetSlot(element), sourceIncremental.amount.GetAmount(level) * rate);
            }
        }

        public static void CombineArmors(List<ArmorAmount> sourceAmounts, ref DamageElementFloatAmounts result, float rate)
        {
            if (sourceAmounts == null)
                return;
            foreach (ArmorAmount sourceAmount in sourceAmounts)
            {
                DamageElement element = sourceAmount.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceAmount.damageElement;
                result.Add(RuntimeGameDataSlots.GetSlot(element), sourceAmount.amount * rate);
            }
        }

        public static void CombineArmors(List<ArmorIncremental> sourceIncrementals, ref DamageElementFloatAmounts result, int level, float rate)
        {
            if (sourceIncrementals == null)
                return;
            foreach (ArmorIncremental sourceIncremental in sourceIncrementals)
            {
                DamageElement element = sourceIncremental.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceIncremental.damageElement;
                result.Add(RuntimeGameDataSlots.GetSlot(element), sourceIncremental.amount.GetAmount(level) * rate);
            }
        }
        public static void CombineCurrencies(List<CurrencyAmount> sourceAmounts, ref CurrencyAmounts result, float rate)
        {
            if (sourceAmounts == null)
                return;
            foreach (CurrencyAmount sourceAmount in sourceAmounts)
            {
                if (sourceAmount.currency != null)
                    result.Add(RuntimeGameDataSlots.GetSlot(sourceAmount.currency), Mathf.CeilToInt(sourceAmount.amount * rate));
            }
        }

        #region Combine Dictionary with List functions
        /// <summary>
        /// Combine skill levels dictionary
        /// </summary>
        /// <param name="sourceLevels"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineSkills(List<SkillLevel> sourceLevels, Dictionary<BaseSkill, int> resultDictionary, float rate)
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
        public static void CombineSkills(List<SkillIncremental> sourceIncrementals, Dictionary<BaseSkill, int> resultDictionary, int level, float rate)
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
        public static void CombineSkills(List<PlayerSkill> sourcePlayerSkills, Dictionary<BaseSkill, int> resultDictionary, int characterLevel)
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
        public static void CombineSkills(List<MonsterSkill> sourceMonsterSkills, Dictionary<BaseSkill, int> resultDictionary, int characterLevel)
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
        public static void CombineStatusEffectResistances(List<StatusEffectResistanceAmount> sourceAmounts, Dictionary<StatusEffect, float> resultDictionary, float rate)
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
        public static void CombineStatusEffectResistances(List<StatusEffectResistanceIncremental> sourceIncrementals, Dictionary<StatusEffect, float> resultDictionary, int level, float rate)
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
        public static void CombineBuffRemovals(List<BuffRemoval> sourceAmounts, Dictionary<BuffRemoval, float> resultDictionary, int level, float rate)
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
        public static void CombineItems(List<ItemAmount> sourceAmounts, Dictionary<BaseItem, int> resultDictionary)
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
        public static void CombineAmmoTypes(List<AmmoTypeAmount> sourceAmounts, Dictionary<AmmoType, int> resultDictionary)
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
    }
}
