using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static partial class GameDataHelpers
    {
        #region Make KeyValuePair functions
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

    }
}
