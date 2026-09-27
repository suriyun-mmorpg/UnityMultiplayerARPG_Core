using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class BuffExtensions
    {
        #region Buff Extension
        public static float GetDuration(this Buff buff, int level)
        {
            return buff.duration.GetAmount(level);
        }

        public static int GetRecoveryHp(this Buff buff, int level)
        {
            return buff.recoveryHp.GetAmount(level);
        }

        public static int GetRecoveryMp(this Buff buff, int level)
        {
            return buff.recoveryMp.GetAmount(level);
        }

        public static int GetRecoveryStamina(this Buff buff, int level)
        {
            return buff.recoveryStamina.GetAmount(level);
        }

        public static int GetRecoveryFood(this Buff buff, int level)
        {
            return buff.recoveryFood.GetAmount(level);
        }

        public static int GetRecoveryWater(this Buff buff, int level)
        {
            return buff.recoveryWater.GetAmount(level);
        }

        public static CharacterStats GetIncreaseStats(this Buff buff, int level)
        {
            return buff.increaseStats.GetCharacterStats(level);
        }

        public static CharacterStats GetIncreaseStatsRate(this Buff buff, int level)
        {
            return buff.increaseStatsRate.GetCharacterStats(level);
        }

        public static void GetIncreaseAttributes(this Buff buff, int level, Dictionary<Attribute, float> result)
        {
            AttributeAmounts amounts = default;
            GameDataHelpers.CombineAttributes(buff.increaseAttributes, ref amounts, level, 1f);
            amounts.CopyTo(result);
        }

        public static void GetIncreaseAttributesRate(this Buff buff, int level, Dictionary<Attribute, float> result)
        {
            AttributeAmounts amounts = default;
            GameDataHelpers.CombineAttributes(buff.increaseAttributesRate, ref amounts, level, 1f);
            amounts.CopyTo(result);
        }

        public static void GetIncreaseResistances(this Buff buff, int level, Dictionary<DamageElement, float> result)
        {
            DamageElementFloatAmounts amounts = default;
            GameDataHelpers.CombineResistances(buff.increaseResistances, ref amounts, level, 1f);
            amounts.CopyTo(result);
        }

        public static void GetIncreaseArmors(this Buff buff, int level, Dictionary<DamageElement, float> result)
        {
            DamageElementFloatAmounts amounts = default;
            GameDataHelpers.CombineArmors(buff.increaseArmors, ref amounts, level, 1f);
            amounts.CopyTo(result);
        }

        public static void GetIncreaseArmorsRate(this Buff buff, int level, Dictionary<DamageElement, float> result)
        {
            DamageElementFloatAmounts amounts = default;
            GameDataHelpers.CombineArmors(buff.increaseArmorsRate, ref amounts, level, 1f);
            amounts.CopyTo(result);
        }

        public static void GetIncreaseDamages(this Buff buff, int level, Dictionary<DamageElement, MinMaxFloat> result)
        {
            DamageElementMinMaxFloatAmounts amounts = default;
            GameDataHelpers.CombineDamages(buff.increaseDamages, ref amounts, level, 1f);
            amounts.CopyToDictionary(result);
        }

        public static void GetIncreaseDamagesRate(this Buff buff, int level, Dictionary<DamageElement, MinMaxFloat> result)
        {
            DamageElementMinMaxFloatAmounts amounts = default;
            GameDataHelpers.CombineDamages(buff.increaseDamagesRate, ref amounts, level, 1f);
            amounts.CopyToDictionary(result);
        }

        public static void GetIncreaseSkills(this Buff buff, int level, Dictionary<BaseSkill, int> result)
        {
            result.Clear();
            GameDataHelpers.CombineSkills(buff.increaseSkills, result, level, 1f);
        }

        public static void GetOverrideSkills(this Buff buff, int level, Dictionary<BaseSkill, int> result)
        {
            result.Clear();
            GameDataHelpers.CombineSkills(buff.overrideSkills, result, level, 1f);
        }

        public static void GetIncreaseStatusEffectResistances(this Buff buff, int level, Dictionary<StatusEffect, float> result)
        {
            result.Clear();
            GameDataHelpers.CombineStatusEffectResistances(buff.increaseStatusEffectResistances, result, level, 1f);
        }

        public static void GetBuffRemovals(this Buff buff, int level, Dictionary<BuffRemoval, float> result)
        {
            result.Clear();
            GameDataHelpers.CombineBuffRemovals(buff.buffRemovals, result, level, 1f);
        }

        public static void GetDamageOverTimes(this Buff buff, int level, Dictionary<DamageElement, MinMaxFloat> result)
        {
            DamageElementMinMaxFloatAmounts amounts = default;
            GameDataHelpers.CombineDamages(buff.damageOverTimes, ref amounts, level, 1f);
            amounts.CopyToDictionary(result);
        }

        public static float GetRemoveBuffWhenAttackChance(this Buff buff, int level)
        {
            return buff.removeBuffWhenAttackChance.GetAmount(level);
        }

        public static float GetRemoveBuffWhenAttackedChance(this Buff buff, int level)
        {
            return buff.removeBuffWhenAttackedChance.GetAmount(level);
        }

        public static float GetRemoveBuffWhenUseSkillChance(this Buff buff, int level)
        {
            return buff.removeBuffWhenUseSkillChance.GetAmount(level);
        }

        public static float GetRemoveBuffWhenUseItemChance(this Buff buff, int level)
        {
            return buff.removeBuffWhenUseItemChance.GetAmount(level);
        }

        public static float GetRemoveBuffWhenPickupItemChance(this Buff buff, int level)
        {
            return buff.removeBuffWhenPickupItemChance.GetAmount(level);
        }

        public static int GetMaxStack(this Buff buff, int level)
        {
            return buff.maxStack.GetAmount(level);
        }

        public static void ApplySelfStatusEffectsWhenAttacking(this Buff buff, int level, EntityInfo applier, BaseCharacterEntity target)
        {
            if (level <= 0 || target == null)
                return;
            buff.selfStatusEffectsWhenAttacking.ApplyStatusEffect(level, applier, CharacterItem.Empty, target);
        }

        public static void ApplyEnemyStatusEffectsWhenAttacking(this Buff buff, int level, EntityInfo applier, BaseCharacterEntity target)
        {
            if (level <= 0 || target == null)
                return;
            buff.enemyStatusEffectsWhenAttacking.ApplyStatusEffect(level, applier, CharacterItem.Empty, target);
        }

        public static void ApplySelfStatusEffectsWhenAttacked(this Buff buff, int level, EntityInfo applier, BaseCharacterEntity target)
        {
            if (level <= 0 || target == null)
                return;
            buff.selfStatusEffectsWhenAttacked.ApplyStatusEffect(level, applier, CharacterItem.Empty, target);
        }

        public static void ApplyEnemyStatusEffectsWhenAttacked(this Buff buff, int level, EntityInfo applier, BaseCharacterEntity target)
        {
            if (level <= 0 || target == null)
                return;
            buff.enemyStatusEffectsWhenAttacked.ApplyStatusEffect(level, applier, CharacterItem.Empty, target);
        }
        #endregion
    }
}
