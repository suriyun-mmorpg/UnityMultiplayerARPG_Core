using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class BuffExtensions
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

        public static int GetMaxStack(this Buff buff, short level)
        {
            return buff.maxStack.GetAmount(level);
        }

        public static void ApplySelfStatusEffectsWhenAttacking(this Buff buff, short level, EntityInfo applier, BaseCharacterEntity target)
        {
            if (level <= 0 || target == null)
                return;
            buff.selfStatusEffectsWhenAttacking.ApplyStatusEffect(level, applier, null, target);
        }

        public static void ApplyEnemyStatusEffectsWhenAttacking(this Buff buff, short level, EntityInfo applier, BaseCharacterEntity target)
        {
            if (level <= 0 || target == null)
                return;
            buff.enemyStatusEffectsWhenAttacking.ApplyStatusEffect(level, applier, null, target);
        }

        public static void ApplySelfStatusEffectsWhenAttacked(this Buff buff, short level, EntityInfo applier, BaseCharacterEntity target)
        {
            if (level <= 0 || target == null)
                return;
            buff.selfStatusEffectsWhenAttacked.ApplyStatusEffect(level, applier, null, target);
        }

        public static void ApplyEnemyStatusEffectsWhenAttacked(this Buff buff, short level, EntityInfo applier, BaseCharacterEntity target)
        {
            if (level <= 0 || target == null)
                return;
            buff.enemyStatusEffectsWhenAttacked.ApplyStatusEffect(level, applier, null, target);
        }
        #endregion
    }
}
