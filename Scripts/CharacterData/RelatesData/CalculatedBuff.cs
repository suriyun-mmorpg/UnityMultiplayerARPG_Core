using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class CalculatedBuff
    {
        private Buff buff;
        private short level;
        private float cacheDuration;
        private int cacheRecoveryHp;
        private int cacheRecoveryMp;
        private int cacheRecoveryStamina;
        private int cacheRecoveryFood;
        private int cacheRecoveryWater;
        private CharacterStats cacheIncreaseStats;
        private CharacterStats cacheIncreaseStatsRate;
        private Dictionary<Attribute, float> cacheIncreaseAttributes;
        private Dictionary<Attribute, float> cacheIncreaseAttributesRate;
        private Dictionary<DamageElement, float> cacheIncreaseResistances;
        private Dictionary<DamageElement, float> cacheIncreaseArmors;
        private Dictionary<DamageElement, MinMaxFloat> cacheIncreaseDamages;
        private Dictionary<DamageElement, MinMaxFloat> cacheDamageOverTimes;
        private int cacheMaxStack;

        public CalculatedBuff(Buff buff, short level)
        {
            this.buff = buff;
            this.level = level;
            cacheDuration = buff.GetDuration(level);
            cacheRecoveryHp = buff.GetRecoveryHp(level);
            cacheRecoveryMp = buff.GetRecoveryMp(level);
            cacheRecoveryStamina = buff.GetRecoveryStamina(level);
            cacheRecoveryFood = buff.GetRecoveryFood(level);
            cacheRecoveryWater = buff.GetRecoveryWater(level);
            cacheIncreaseStats = buff.GetIncreaseStats(level);
            cacheIncreaseStatsRate = buff.GetIncreaseStatsRate(level);
            cacheIncreaseAttributes = buff.GetIncreaseAttributes(level);
            cacheIncreaseAttributesRate = buff.GetIncreaseAttributesRate(level);
            cacheIncreaseResistances = buff.GetIncreaseResistances(level);
            cacheIncreaseArmors = buff.GetIncreaseArmors(level);
            cacheIncreaseDamages = buff.GetIncreaseDamages(level);
            cacheDamageOverTimes = buff.GetDamageOverTimes(level);
            cacheMaxStack = buff.GetMaxStack(level);
        }

        public Buff GetBuff()
        {
            return buff;
        }

        public short GetLevel()
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

        public int MaxStack()
        {
            return cacheMaxStack;
        }
    }
}
