using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class CalculatedBuff
    {
        private Buff buff;
        private int level;
        private float cacheDuration;
        private int cacheRecoveryHp;
        private int cacheRecoveryMp;
        private int cacheRecoveryStamina;
        private int cacheRecoveryFood;
        private int cacheRecoveryWater;
        private CharacterStats cacheIncreaseStats;
        private CharacterStats cacheIncreaseStatsRate;
        private Dictionary<Attribute, float> cacheIncreaseAttributes = new Dictionary<Attribute, float>();
        private Dictionary<Attribute, float> cacheIncreaseAttributesRate = new Dictionary<Attribute, float>();
        private Dictionary<DamageElement, float> cacheIncreaseResistances = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, float> cacheIncreaseArmors = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, MinMaxFloat> cacheIncreaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
        private Dictionary<DamageElement, MinMaxFloat> cacheDamageOverTimes = new Dictionary<DamageElement, MinMaxFloat>();
        private float cacheRemoveBuffWhenAttackChance;
        private float cacheRemoveBuffWhenAttackedChance;
        private float cacheRemoveBuffWhenUseSkillChance;
        private float cacheRemoveBuffWhenUseItemChance;
        private float cacheRemoveBuffWhenPickupItemChance;
        private int cacheMaxStack;

        public void Build(Buff buff, int level)
        {
            this.buff = buff;
            this.level = level;

            cacheIncreaseAttributes.Clear();
            cacheIncreaseAttributesRate.Clear();
            cacheIncreaseResistances.Clear();
            cacheIncreaseArmors.Clear();
            cacheIncreaseDamages.Clear();
            cacheDamageOverTimes.Clear();

            cacheDuration = buff.GetDuration(level);
            cacheRecoveryHp = buff.GetRecoveryHp(level);
            cacheRecoveryMp = buff.GetRecoveryMp(level);
            cacheRecoveryStamina = buff.GetRecoveryStamina(level);
            cacheRecoveryFood = buff.GetRecoveryFood(level);
            cacheRecoveryWater = buff.GetRecoveryWater(level);
            cacheIncreaseStats = buff.GetIncreaseStats(level);
            cacheIncreaseStatsRate = buff.GetIncreaseStatsRate(level);
            buff.GetIncreaseAttributes(level, cacheIncreaseAttributes);
            buff.GetIncreaseAttributesRate(level, cacheIncreaseAttributesRate);
            buff.GetIncreaseResistances(level, cacheIncreaseResistances);
            buff.GetIncreaseArmors(level, cacheIncreaseArmors);
            buff.GetIncreaseDamages(level, cacheIncreaseDamages);
            buff.GetDamageOverTimes(level, cacheDamageOverTimes);
            cacheRemoveBuffWhenAttackChance = buff.GetRemoveBuffWhenAttackChance(level);
            cacheRemoveBuffWhenAttackedChance = buff.GetRemoveBuffWhenAttackedChance(level);
            cacheRemoveBuffWhenUseSkillChance = buff.GetRemoveBuffWhenUseSkillChance(level);
            cacheRemoveBuffWhenUseItemChance = buff.GetRemoveBuffWhenUseItemChance(level);
            cacheRemoveBuffWhenPickupItemChance = buff.GetRemoveBuffWhenPickupItemChance(level);
            cacheMaxStack = buff.GetMaxStack(level);
        }

        public Buff GetBuff()
        {
            return buff;
        }

        public int GetLevel()
        {
            return level;
        }

        public float GetDuration()
        {
            return cacheDuration;
        }

        public int GetRecoveryHp()
        {
            return cacheRecoveryHp;
        }

        public int GetRecoveryMp()
        {
            return cacheRecoveryMp;
        }

        public int GetRecoveryStamina()
        {
            return cacheRecoveryStamina;
        }

        public int GetRecoveryFood()
        {
            return cacheRecoveryFood;
        }

        public int GetRecoveryWater()
        {
            return cacheRecoveryWater;
        }

        public CharacterStats GetIncreaseStats()
        {
            return cacheIncreaseStats;
        }

        public CharacterStats GetIncreaseStatsRate()
        {
            return cacheIncreaseStatsRate;
        }

        public Dictionary<Attribute, float> GetIncreaseAttributes()
        {
            return cacheIncreaseAttributes;
        }

        public Dictionary<Attribute, float> GetIncreaseAttributesRate()
        {
            return cacheIncreaseAttributesRate;
        }

        public Dictionary<DamageElement, float> GetIncreaseResistances()
        {
            return cacheIncreaseResistances;
        }

        public Dictionary<DamageElement, float> GetIncreaseArmors()
        {
            return cacheIncreaseArmors;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages()
        {
            return cacheIncreaseDamages;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetDamageOverTimes()
        {
            return cacheDamageOverTimes;
        }

        public float GetRemoveBuffWhenAttackChance()
        {
            return cacheRemoveBuffWhenAttackChance;
        }

        public float GetRemoveBuffWhenAttackedChance()
        {
            return cacheRemoveBuffWhenAttackedChance;
        }

        public float GetRemoveBuffWhenUseSkillChance()
        {
            return cacheRemoveBuffWhenUseSkillChance;
        }

        public float GetRemoveBuffWhenUseItemChance()
        {
            return cacheRemoveBuffWhenUseItemChance;
        }

        public float GetRemoveBuffWhenPickupItemChance()
        {
            return cacheRemoveBuffWhenPickupItemChance;
        }

        public int MaxStack()
        {
            return cacheMaxStack;
        }
    }
}
