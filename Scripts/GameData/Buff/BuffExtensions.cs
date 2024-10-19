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

        public static Dictionary<Attribute, float> GetIncreaseAttributes(this Buff buff, int level, Dictionary<Attribute, float> result = null)
        {
            return GameDataHelpers.CombineAttributes(buff.increaseAttributes, result, level, 1f);
        }

        public static Dictionary<Attribute, float> GetIncreaseAttributesRate(this Buff buff, int level, Dictionary<Attribute, float> result = null)
        {
            return GameDataHelpers.CombineAttributes(buff.increaseAttributesRate, result, level, 1f);
        }

        public static Dictionary<DamageElement, float> GetIncreaseResistances(this Buff buff, int level, Dictionary<DamageElement, float> result = null)
        {
            return GameDataHelpers.CombineResistances(buff.increaseResistances, result, level, 1f);
        }

        public static Dictionary<DamageElement, float> GetIncreaseArmors(this Buff buff, int level, Dictionary<DamageElement, float> result = null)
        {
            return GameDataHelpers.CombineArmors(buff.increaseArmors, result, level, 1f);
        }

        public static Dictionary<DamageElement, float> GetIncreaseArmorsRate(this Buff buff, int level, Dictionary<DamageElement, float> result = null)
        {
            return GameDataHelpers.CombineArmors(buff.increaseArmorsRate, result, level, 1f);
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this Buff buff, int level, Dictionary<DamageElement, MinMaxFloat> result = null)
        {
            return GameDataHelpers.CombineDamages(buff.increaseDamages, result, level, 1f);
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamagesRate(this Buff buff, int level, Dictionary<DamageElement, MinMaxFloat> result = null)
        {
            return GameDataHelpers.CombineDamages(buff.increaseDamagesRate, result, level, 1f);
        }

        public static Dictionary<BaseSkill, int> GetIncreaseSkills(this Buff buff, int level, Dictionary<BaseSkill, int> result = null)
        {
            return GameDataHelpers.CombineSkills(buff.increaseSkills, result, level, 1f);
        }

        public static Dictionary<BaseSkill, int> GetOverrideSkills(this Buff buff, int level, Dictionary<BaseSkill, int> result = null)
        {
            return GameDataHelpers.CombineSkills(buff.overrideSkills, result, level, 1f);
        }

        public static Dictionary<StatusEffect, float> GetIncreaseStatusEffectResistances(this Buff buff, int level, Dictionary<StatusEffect, float> result = null)
        {
            return GameDataHelpers.CombineStatusEffectResistances(buff.increaseStatusEffectResistances, result, level, 1f);
        }

        public static Dictionary<BuffRemoval, float> GetBuffRemovals(this Buff buff, int level, Dictionary<BuffRemoval, float> result = null)
        {
            return GameDataHelpers.CombineBuffRemovals(buff.buffRemovals, result, level, 1f);
        }

        public static Dictionary<DamageElement, MinMaxFloat> GetDamageOverTimes(this Buff buff, int level, Dictionary<DamageElement, MinMaxFloat> result = null)
        {
            return GameDataHelpers.CombineDamages(buff.damageOverTimes, result, level, 1f);
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
