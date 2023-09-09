using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class CalculatedBuff
    {
        private Buff _buff;
        private int _level;
        private float _cacheDuration;
        private int _cacheRecoveryHp;
        private int _cacheRecoveryMp;
        private int _cacheRecoveryStamina;
        private int _cacheRecoveryFood;
        private int _cacheRecoveryWater;
        private CharacterStats _cacheIncreaseStats;
        private CharacterStats _cacheIncreaseStatsRate;
        private Dictionary<Attribute, float> _cacheIncreaseAttributes = new Dictionary<Attribute, float>();
        private Dictionary<Attribute, float> _cacheIncreaseAttributesRate = new Dictionary<Attribute, float>();
        private Dictionary<DamageElement, float> _cacheIncreaseResistances = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, float> _cacheIncreaseArmors = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, float> _cacheIncreaseArmorsRate = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, MinMaxFloat> _cacheIncreaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
        private Dictionary<DamageElement, MinMaxFloat> _cacheIncreaseDamagesRate = new Dictionary<DamageElement, MinMaxFloat>();
        private Dictionary<DamageElement, MinMaxFloat> _cacheDamageOverTimes = new Dictionary<DamageElement, MinMaxFloat>();
        private float _cacheRemoveBuffWhenAttackChance;
        private float _cacheRemoveBuffWhenAttackedChance;
        private float _cacheRemoveBuffWhenUseSkillChance;
        private float _cacheRemoveBuffWhenUseItemChance;
        private float _cacheRemoveBuffWhenPickupItemChance;
        private int _cacheMaxStack;

        public CalculatedBuff()
        {

        }

        public CalculatedBuff(Buff buff, int level)
        {
            Build(buff, level);
        }

        ~CalculatedBuff()
        {
            _cacheIncreaseAttributes.Clear();
            _cacheIncreaseAttributes = null;
            _cacheIncreaseAttributesRate.Clear();
            _cacheIncreaseAttributesRate = null;
            _cacheIncreaseResistances.Clear();
            _cacheIncreaseResistances = null;
            _cacheIncreaseArmors.Clear();
            _cacheIncreaseArmors = null;
            _cacheIncreaseArmorsRate.Clear();
            _cacheIncreaseArmorsRate = null;
            _cacheIncreaseDamages.Clear();
            _cacheIncreaseDamages = null;
            _cacheIncreaseDamagesRate.Clear();
            _cacheIncreaseDamagesRate = null;
            _cacheDamageOverTimes.Clear();
            _cacheDamageOverTimes = null;
        }

        public void Build(Buff buff, int level)
        {
            _buff = buff;
            _level = level;

            _cacheIncreaseAttributes.Clear();
            _cacheIncreaseAttributesRate.Clear();
            _cacheIncreaseResistances.Clear();
            _cacheIncreaseArmors.Clear();
            _cacheIncreaseArmorsRate.Clear();
            _cacheIncreaseDamages.Clear();
            _cacheIncreaseDamagesRate.Clear();
            _cacheDamageOverTimes.Clear();

            _cacheDuration = buff.GetDuration(level);
            _cacheRecoveryHp = buff.GetRecoveryHp(level);
            _cacheRecoveryMp = buff.GetRecoveryMp(level);
            _cacheRecoveryStamina = buff.GetRecoveryStamina(level);
            _cacheRecoveryFood = buff.GetRecoveryFood(level);
            _cacheRecoveryWater = buff.GetRecoveryWater(level);
            _cacheIncreaseStats = buff.GetIncreaseStats(level);
            _cacheIncreaseStatsRate = buff.GetIncreaseStatsRate(level);
            buff.GetIncreaseAttributes(level, _cacheIncreaseAttributes);
            buff.GetIncreaseAttributesRate(level, _cacheIncreaseAttributesRate);
            buff.GetIncreaseResistances(level, _cacheIncreaseResistances);
            buff.GetIncreaseArmors(level, _cacheIncreaseArmors);
            buff.GetIncreaseArmorsRate(level, _cacheIncreaseArmorsRate);
            buff.GetIncreaseDamages(level, _cacheIncreaseDamages);
            buff.GetIncreaseDamagesRate(level, _cacheIncreaseDamagesRate);
            buff.GetDamageOverTimes(level, _cacheDamageOverTimes);
            _cacheRemoveBuffWhenAttackChance = buff.GetRemoveBuffWhenAttackChance(level);
            _cacheRemoveBuffWhenAttackedChance = buff.GetRemoveBuffWhenAttackedChance(level);
            _cacheRemoveBuffWhenUseSkillChance = buff.GetRemoveBuffWhenUseSkillChance(level);
            _cacheRemoveBuffWhenUseItemChance = buff.GetRemoveBuffWhenUseItemChance(level);
            _cacheRemoveBuffWhenPickupItemChance = buff.GetRemoveBuffWhenPickupItemChance(level);
            _cacheMaxStack = buff.GetMaxStack(level);

            if (GameExtensionInstance.onBuildCalculatedBuff != null)
                GameExtensionInstance.onBuildCalculatedBuff(this);
        }

        public Buff GetBuff()
        {
            return _buff;
        }

        public int GetLevel()
        {
            return _level;
        }

        public float GetDuration()
        {
            return _cacheDuration;
        }

        public int GetRecoveryHp()
        {
            return _cacheRecoveryHp;
        }

        public int GetRecoveryMp()
        {
            return _cacheRecoveryMp;
        }

        public int GetRecoveryStamina()
        {
            return _cacheRecoveryStamina;
        }

        public int GetRecoveryFood()
        {
            return _cacheRecoveryFood;
        }

        public int GetRecoveryWater()
        {
            return _cacheRecoveryWater;
        }

        public CharacterStats GetIncreaseStats()
        {
            return _cacheIncreaseStats;
        }

        public CharacterStats GetIncreaseStatsRate()
        {
            return _cacheIncreaseStatsRate;
        }

        public Dictionary<Attribute, float> GetIncreaseAttributes()
        {
            return _cacheIncreaseAttributes;
        }

        public Dictionary<Attribute, float> GetIncreaseAttributesRate()
        {
            return _cacheIncreaseAttributesRate;
        }

        public Dictionary<DamageElement, float> GetIncreaseResistances()
        {
            return _cacheIncreaseResistances;
        }

        public Dictionary<DamageElement, float> GetIncreaseArmors()
        {
            return _cacheIncreaseArmors;
        }

        public Dictionary<DamageElement, float> GetIncreaseArmorsRate()
        {
            return _cacheIncreaseArmorsRate;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages()
        {
            return _cacheIncreaseDamages;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamagesRate()
        {
            return _cacheIncreaseDamagesRate;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetDamageOverTimes()
        {
            return _cacheDamageOverTimes;
        }

        public float GetRemoveBuffWhenAttackChance()
        {
            return _cacheRemoveBuffWhenAttackChance;
        }

        public float GetRemoveBuffWhenAttackedChance()
        {
            return _cacheRemoveBuffWhenAttackedChance;
        }

        public float GetRemoveBuffWhenUseSkillChance()
        {
            return _cacheRemoveBuffWhenUseSkillChance;
        }

        public float GetRemoveBuffWhenUseItemChance()
        {
            return _cacheRemoveBuffWhenUseItemChance;
        }

        public float GetRemoveBuffWhenPickupItemChance()
        {
            return _cacheRemoveBuffWhenPickupItemChance;
        }

        public int MaxStack()
        {
            return _cacheMaxStack;
        }
    }
}
