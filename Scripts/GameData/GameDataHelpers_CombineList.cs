using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static partial class GameDataHelpers
    {
        #region Combine Dictionary with List functions
        /// <summary>
        /// Combine damage amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineDamages(List<DamageAmount> sourceAmounts, Dictionary<DamageElement, MinMaxFloat> resultDictionary, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceAmounts == null)
                return;
            DamageElementMinMaxFloatAmounts amounts = default;
            amounts.Combine(resultDictionary);
            foreach (DamageAmount sourceAmount in sourceAmounts)
            {
                DamageElement element = sourceAmount.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceAmount.damageElement;
                amounts.Add(RuntimeGameDataSlots.GetSlot(element), sourceAmount.amount * rate);
            }
            amounts.CopyTo(resultDictionary);
        }

        /// <summary>
        /// Combine damage amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineDamages(List<DamageIncremental> sourceIncrementals, Dictionary<DamageElement, MinMaxFloat> resultDictionary, int level, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceIncrementals == null)
                return;
            DamageElementMinMaxFloatAmounts amounts = default;
            amounts.Combine(resultDictionary);
            foreach (DamageIncremental sourceIncremental in sourceIncrementals)
            {
                DamageElement element = sourceIncremental.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceIncremental.damageElement;
                amounts.Add(RuntimeGameDataSlots.GetSlot(element), sourceIncremental.amount.GetAmount(level) * rate);
            }
            amounts.CopyTo(resultDictionary);
        }

        /// <summary>
        /// Combine damage infliction amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static void CombineDamageInflictions(List<DamageInflictionIncremental> sourceIncrementals, Dictionary<DamageElement, float> resultDictionary, int level)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceIncrementals == null)
                return;
            DamageElementFloatAmounts amounts = default;
            amounts.Combine(resultDictionary);
            foreach (DamageInflictionIncremental sourceIncremental in sourceIncrementals)
            {
                DamageElement element = sourceIncremental.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceIncremental.damageElement;
                amounts.Add(RuntimeGameDataSlots.GetSlot(element), sourceIncremental.rate.GetAmount(level));
            }
            amounts.CopyTo(resultDictionary);
        }

        /// <summary>
        /// Combine attribute amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineAttributes(List<AttributeAmount> sourceAmounts, Dictionary<Attribute, float> resultDictionary, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceAmounts == null)
                return;
            AttributeAmounts amounts = default;
            amounts.Combine(resultDictionary);
            foreach (AttributeAmount sourceAmount in sourceAmounts)
            {
                if (sourceAmount.attribute != null)
                    amounts.Add(RuntimeGameDataSlots.GetSlot(sourceAmount.attribute), sourceAmount.amount * rate);
            }
            amounts.CopyTo(resultDictionary);
        }

        /// <summary>
        /// Combine attribute amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineAttributes(List<AttributeIncremental> sourceIncrementals, Dictionary<Attribute, float> resultDictionary, int level, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceIncrementals == null)
                return;
            AttributeAmounts amounts = default;
            amounts.Combine(resultDictionary);
            foreach (AttributeIncremental sourceIncremental in sourceIncrementals)
            {
                if (sourceIncremental.attribute != null)
                    amounts.Add(RuntimeGameDataSlots.GetSlot(sourceIncremental.attribute), sourceIncremental.amount.GetAmount(level) * rate);
            }
            amounts.CopyTo(resultDictionary);
        }

        /// <summary>
        /// Combine currency amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineCurrencies(List<CurrencyAmount> sourceAmounts, Dictionary<Currency, int> resultDictionary, float rate)
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
        public static void CombineResistances(List<ResistanceAmount> sourceAmounts, Dictionary<DamageElement, float> resultDictionary, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceAmounts == null)
                return;
            DamageElementFloatAmounts amounts = default;
            amounts.Combine(resultDictionary);
            foreach (ResistanceAmount sourceAmount in sourceAmounts)
            {
                DamageElement element = sourceAmount.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceAmount.damageElement;
                amounts.Add(RuntimeGameDataSlots.GetSlot(element), sourceAmount.amount * rate);
            }
            amounts.CopyTo(resultDictionary);
        }

        /// <summary>
        /// Combine resistance amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineResistances(List<ResistanceIncremental> sourceIncrementals, Dictionary<DamageElement, float> resultDictionary, int level, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceIncrementals == null)
                return;
            DamageElementFloatAmounts amounts = default;
            amounts.Combine(resultDictionary);
            foreach (ResistanceIncremental sourceIncremental in sourceIncrementals)
            {
                DamageElement element = sourceIncremental.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceIncremental.damageElement;
                amounts.Add(RuntimeGameDataSlots.GetSlot(element), sourceIncremental.amount.GetAmount(level) * rate);
            }
            amounts.CopyTo(resultDictionary);
        }

        /// <summary>
        /// Combine armor amounts dictionary
        /// </summary>
        /// <param name="sourceAmounts"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineArmors(List<ArmorAmount> sourceAmounts, Dictionary<DamageElement, float> resultDictionary, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceAmounts == null)
                return;
            DamageElementFloatAmounts amounts = default;
            amounts.Combine(resultDictionary);
            foreach (ArmorAmount sourceAmount in sourceAmounts)
            {
                DamageElement element = sourceAmount.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceAmount.damageElement;
                amounts.Add(RuntimeGameDataSlots.GetSlot(element), sourceAmount.amount * rate);
            }
            amounts.CopyTo(resultDictionary);
        }

        /// <summary>
        /// Combine armor amounts dictionary
        /// </summary>
        /// <param name="sourceIncrementals"></param>
        /// <param name="resultDictionary"></param>
        /// <param name="level"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public static void CombineArmors(List<ArmorIncremental> sourceIncrementals, Dictionary<DamageElement, float> resultDictionary, int level, float rate)
        {
            if (resultDictionary == null)
            {
                Debug.LogError("Collecton is null");
                return;
            }
            if (sourceIncrementals == null)
                return;
            DamageElementFloatAmounts amounts = default;
            amounts.Combine(resultDictionary);
            foreach (ArmorIncremental sourceIncremental in sourceIncrementals)
            {
                DamageElement element = sourceIncremental.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : sourceIncremental.damageElement;
                amounts.Add(RuntimeGameDataSlots.GetSlot(element), sourceIncremental.amount.GetAmount(level) * rate);
            }
            amounts.CopyTo(resultDictionary);
        }

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
