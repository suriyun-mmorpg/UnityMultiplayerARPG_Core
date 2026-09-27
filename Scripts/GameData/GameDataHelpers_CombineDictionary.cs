using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static partial class GameDataHelpers
    {
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
            DamageElementMinMaxFloatAmounts amounts = default;
            amounts.Combine(resultDictionary);
            amounts.Combine(combineDictionary, rate);
            amounts.CopyTo(resultDictionary);
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
            DamageElementMinMaxFloatAmounts amounts = default;
            amounts.Combine(resultDictionary);
            DamageElementMinMaxFloatAmounts rates = default;
            rates.Combine(multiplyDictionary);
            amounts.MultiplyRates(rates);
            amounts.CopyTo(resultDictionary);
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
            DamageElementFloatAmounts amounts = default;
            amounts.Combine(resultDictionary);
            amounts.Combine(combineDictionary);
            amounts.CopyTo(resultDictionary);
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
            AttributeAmounts amounts = default;
            amounts.Combine(resultDictionary);
            amounts.Combine(combineDictionary);
            amounts.CopyTo(resultDictionary);
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
            AttributeAmounts amounts = default;
            amounts.Combine(resultDictionary);
            AttributeAmounts rates = default;
            rates.Combine(multiplyDictionary);
            amounts.MultiplyValues(rates);
            amounts.CopyTo(resultDictionary);
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
            DamageElementFloatAmounts amounts = default;
            amounts.Combine(resultDictionary);
            amounts.Combine(combineDictionary);
            amounts.CopyTo(resultDictionary);
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
            DamageElementFloatAmounts amounts = default;
            amounts.Combine(resultDictionary);
            amounts.Combine(combineDictionary);
            amounts.CopyTo(resultDictionary);
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
            DamageElementFloatAmounts amounts = default;
            amounts.Combine(resultDictionary);
            DamageElementFloatAmounts rates = default;
            rates.Combine(multiplyDictionary);
            amounts.MultiplyValues(rates);
            amounts.CopyTo(resultDictionary);
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
        /// <param name="combineDictionary"></param>
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
        /// <param name="combineDictionary"></param>
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
    }
}
