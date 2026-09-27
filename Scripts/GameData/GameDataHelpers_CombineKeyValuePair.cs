using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static partial class GameDataHelpers
    {
        #region Combine Dictionary with KeyValuePair functions
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
    }
}
