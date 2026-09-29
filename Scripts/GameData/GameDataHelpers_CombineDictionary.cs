using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static partial class GameDataHelpers
    {
        #region Combine Dictionary with Dictionary functions
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
