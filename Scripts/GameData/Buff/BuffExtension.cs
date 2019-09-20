using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class BuffExtension
    {
        #region Buff Extension
        public static float GetDuration(this Buff buff, short level)
        {
            return buff.duration.GetAmount(level);
        }

        public static int GetRecoveryHp(this Buff buff, short level)
        {
            return buff.recoveryHp.GetAmount(level);
        }

        public static int GetRecoveryMp(this Buff buff, short level)
        {
            return buff.recoveryMp.GetAmount(level);
        }

        public static int GetRecoveryStamina(this Buff buff, short level)
        {
            return buff.recoveryStamina.GetAmount(level);
        }

        public static int GetRecoveryFood(this Buff buff, short level)
        {
            return buff.recoveryFood.GetAmount(level);
        }

        public static int GetRecoveryWater(this Buff buff, short level)
        {
            return buff.recoveryWater.GetAmount(level);
        }

        public static CharacterStats GetIncreaseStats(this Buff buff, short level)
        {
            return buff.increaseStats.GetCharacterStats(level);
        }

        public static CharacterStats GetIncreaseStatsRate(this Buff buff, short level)
        {
            return buff.increaseStatsRate.GetCharacterStats(level);
        }

        public static Dictionary<Attribute, float> GetIncreaseAttributes(this Buff buff, short level)
        {
            return GameDataHelpers.CombineAttributes(buff.increaseAttributes, new Dictionary<Attribute, float>(), level, 1f);
        }

        public static Dictionary<Attribute, float> GetIncreaseAttributesRate(this Buff buff, short level)
        {
            return GameDataHelpers.CombineAttributes(buff.increaseAttributesRate, new Dictionary<Attribute, float>(), level, 1f);
        }

        public static Dictionary<DamageElement, float> GetIncreaseResistances(this Buff buff, short level)
        {
            return GameDataHelpers.CombineResistances(buff.increaseResistances, new Dictionary<DamageElement, float>(), level, 1f);
        }

        public static Dictionary<DamageElement, float> GetIncreaseArmors(this Buff buff, short level)
        {
            return GameDataHelpers.CombineArmors(buff.increaseArmors, new Dictionary<DamageElement, float>(), level, 1f);
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this Buff buff, short level)
        {
            return GameDataHelpers.CombineDamages(buff.increaseDamages, new Dictionary<DamageElement, MinMaxFloat>(), level, 1f);
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetDamageOverTimes(this Buff buff, short level)
        {
            return GameDataHelpers.CombineDamages(buff.damageOverTimes, new Dictionary<DamageElement, MinMaxFloat>(), level, 1f);
        }
        #endregion
    }
}
