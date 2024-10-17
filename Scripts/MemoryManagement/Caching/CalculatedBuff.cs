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
        private Dictionary<BaseSkill, int> _cacheIncreaseSkills = new Dictionary<BaseSkill, int>();
        private Dictionary<BaseSkill, int> _cacheReplaceSkills = new Dictionary<BaseSkill, int>();
        private Dictionary<StatusEffect, float> _cacheIncreaseStatusEffectResistances = new Dictionary<StatusEffect, float>();
        private Dictionary<BuffRemoval, float> _cacheBuffRemovals = new Dictionary<BuffRemoval, float>();
        private Dictionary<DamageElement, MinMaxFloat> _cacheDamageOverTimes = new Dictionary<DamageElement, MinMaxFloat>();
        private float _cacheRemoveBuffWhenAttackChance;
        private float _cacheRemoveBuffWhenAttackedChance;
        private float _cacheRemoveBuffWhenUseSkillChance;
        private float _cacheRemoveBuffWhenUseItemChance;
        private float _cacheRemoveBuffWhenPickupItemChance;
        private int _cacheMaxStack;
        private BuffMount _cacheMount;
        private int _cacheMountLevel;

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
            _cacheIncreaseSkills.Clear();
            _cacheIncreaseSkills = null;
            _cacheReplaceSkills.Clear();
            _cacheReplaceSkills = null;
            _cacheIncreaseStatusEffectResistances.Clear();
            _cacheIncreaseStatusEffectResistances = null;
            _cacheBuffRemovals.Clear();
            _cacheBuffRemovals = null;
            _cacheDamageOverTimes.Clear();
            _cacheDamageOverTimes = null;
            _cacheMount = null;
        }

        public void Clear()
        {
            _cacheIncreaseAttributes.Clear();
            _cacheIncreaseAttributesRate.Clear();
            _cacheIncreaseResistances.Clear();
            _cacheIncreaseArmors.Clear();
            _cacheIncreaseArmorsRate.Clear();
            _cacheIncreaseDamages.Clear();
            _cacheIncreaseDamagesRate.Clear();
            _cacheIncreaseSkills.Clear();
            _cacheReplaceSkills.Clear();
            _cacheIncreaseStatusEffectResistances.Clear();
            _cacheBuffRemovals.Clear();
            _cacheDamageOverTimes.Clear();
            _cacheMount = null;
        }

        public void Build(Buff buff, int level)
        {
            _buff = buff;
            _level = level;

            Clear();

            if (buff != null)
            {
                _cacheDuration = buff.GetDuration(level);
                _cacheRecoveryHp = buff.GetRecoveryHp(level);
                _cacheRecoveryMp = buff.GetRecoveryMp(level);
                _cacheRecoveryStamina = buff.GetRecoveryStamina(level);
                _cacheRecoveryFood = buff.GetRecoveryFood(level);
                _cacheRecoveryWater = buff.GetRecoveryWater(level);
                _cacheIncreaseStats = buff.GetIncreaseStats(level);
                _cacheIncreaseStatsRate = buff.GetIncreaseStatsRate(level);
                _cacheIncreaseAttributes = buff.GetIncreaseAttributes(level, _cacheIncreaseAttributes);
                _cacheIncreaseAttributesRate = buff.GetIncreaseAttributesRate(level, _cacheIncreaseAttributesRate);
                _cacheIncreaseResistances = buff.GetIncreaseResistances(level, _cacheIncreaseResistances);
                _cacheIncreaseArmors = buff.GetIncreaseArmors(level, _cacheIncreaseArmors);
                _cacheIncreaseArmorsRate = buff.GetIncreaseArmorsRate(level, _cacheIncreaseArmorsRate);
                _cacheIncreaseDamages = buff.GetIncreaseDamages(level, _cacheIncreaseDamages);
                _cacheIncreaseDamagesRate = buff.GetIncreaseDamagesRate(level, _cacheIncreaseDamagesRate);
                _cacheIncreaseSkills = buff.GetIncreaseSkills(level, _cacheIncreaseSkills);
                _cacheReplaceSkills = buff.GetReplaceSkills(level, _cacheReplaceSkills);
                _cacheIncreaseStatusEffectResistances = buff.GetIncreaseStatusEffectResistances(level, _cacheIncreaseStatusEffectResistances);
                _cacheBuffRemovals = buff.GetBuffRemovals(level, _cacheBuffRemovals);
                _cacheDamageOverTimes = buff.GetDamageOverTimes(level, _cacheDamageOverTimes);
                _cacheRemoveBuffWhenAttackChance = buff.GetRemoveBuffWhenAttackChance(level);
                _cacheRemoveBuffWhenAttackedChance = buff.GetRemoveBuffWhenAttackedChance(level);
                _cacheRemoveBuffWhenUseSkillChance = buff.GetRemoveBuffWhenUseSkillChance(level);
                _cacheRemoveBuffWhenUseItemChance = buff.GetRemoveBuffWhenUseItemChance(level);
                _cacheRemoveBuffWhenPickupItemChance = buff.GetRemoveBuffWhenPickupItemChance(level);
                _cacheMaxStack = buff.GetMaxStack(level);
                _cacheMountLevel = 0;
                if (buff.TryGetMount(out BuffMount mount))
                {
                    _cacheMount = mount;
                    _cacheMountLevel = mount.Level.GetAmount(_level);
                }
            }

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

        public Dictionary<BaseSkill, int> GetIncreaseSkills()
        {
            return _cacheIncreaseSkills;
        }

        public Dictionary<BaseSkill, int> GetReplaceSkills()
        {
            return _cacheReplaceSkills;
        }

        public Dictionary<StatusEffect, float> GetIncreaseStatusEffectResistances()
        {
            return _cacheIncreaseStatusEffectResistances;
        }

        public Dictionary<BuffRemoval, float> GetBuffRemovals()
        {
            return _cacheBuffRemovals;
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

        public bool TryGetMount(out BuffMount mount)
        {
            mount = _cacheMount;
            return mount != null;
        }

        public int GetMountLevel()
        {
            return _cacheMountLevel;
        }
    }
}
