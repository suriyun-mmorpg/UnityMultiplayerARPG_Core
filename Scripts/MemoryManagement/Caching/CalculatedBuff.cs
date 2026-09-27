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
        private AttributeAmounts _cacheIncreaseAttributes;
        private Dictionary<Attribute, float> _viewIncreaseAttributes;
        private AttributeAmounts _cacheIncreaseAttributesRate;
        private Dictionary<Attribute, float> _viewIncreaseAttributesRate;
        private DamageElementFloatAmounts _cacheIncreaseResistances;
        private Dictionary<DamageElement, float> _viewIncreaseResistances;
        private DamageElementFloatAmounts _cacheIncreaseArmors;
        private Dictionary<DamageElement, float> _viewIncreaseArmors;
        private DamageElementFloatAmounts _cacheIncreaseArmorsRate;
        private Dictionary<DamageElement, float> _viewIncreaseArmorsRate;
        private DamageElementMinMaxFloatAmounts _cacheIncreaseDamages;
        private Dictionary<DamageElement, MinMaxFloat> _viewIncreaseDamages;
        private DamageElementMinMaxFloatAmounts _cacheIncreaseDamagesRate;
        private Dictionary<DamageElement, MinMaxFloat> _viewIncreaseDamagesRate;
        private readonly Dictionary<BaseSkill, int> _cacheIncreaseSkills;
        private readonly Dictionary<BaseSkill, int> _cacheOverrideSkills;
        private readonly Dictionary<StatusEffect, float> _cacheIncreaseStatusEffectResistances;
        private readonly Dictionary<BuffRemoval, float> _cacheBuffRemovals;
        private DamageElementMinMaxFloatAmounts _cacheDamageOverTimes;
        private Dictionary<DamageElement, MinMaxFloat> _viewDamageOverTimes;
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
            _cacheIncreaseSkills = new Dictionary<BaseSkill, int>();
            _cacheOverrideSkills = new Dictionary<BaseSkill, int>();
            _cacheIncreaseStatusEffectResistances = new Dictionary<StatusEffect, float>();
            _cacheBuffRemovals = new Dictionary<BuffRemoval, float>();
        }

        public CalculatedBuff(Buff buff, int level)
        {
            _cacheIncreaseSkills = new Dictionary<BaseSkill, int>();
            _cacheOverrideSkills = new Dictionary<BaseSkill, int>();
            _cacheIncreaseStatusEffectResistances = new Dictionary<StatusEffect, float>();
            _cacheBuffRemovals = new Dictionary<BuffRemoval, float>();
            Build(buff, level);
        }

        ~CalculatedBuff()
        {
            Clear();
        }

        public void Clear()
        {
            _cacheIncreaseAttributes.Clear();
            _viewIncreaseAttributes?.Clear();
            _cacheIncreaseAttributesRate.Clear();
            _viewIncreaseAttributesRate?.Clear();
            _cacheIncreaseResistances.Clear();
            _viewIncreaseResistances?.Clear();
            _cacheIncreaseArmors.Clear();
            _viewIncreaseArmors?.Clear();
            _cacheIncreaseArmorsRate.Clear();
            _viewIncreaseArmorsRate?.Clear();
            _cacheIncreaseDamages.Clear();
            _viewIncreaseDamages?.Clear();
            _cacheIncreaseDamagesRate.Clear();
            _viewIncreaseDamagesRate?.Clear();
            _cacheIncreaseSkills?.Clear();
            _cacheOverrideSkills?.Clear();
            _cacheIncreaseStatusEffectResistances?.Clear();
            _cacheBuffRemovals?.Clear();
            _cacheDamageOverTimes.Clear();
            _viewDamageOverTimes?.Clear();
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
                GameDataHelpers.CombineAttributes(buff.increaseAttributes, ref _cacheIncreaseAttributes, level, 1f);
                GameDataHelpers.CombineAttributes(buff.increaseAttributesRate, ref _cacheIncreaseAttributesRate, level, 1f);
                GameDataHelpers.CombineResistances(buff.increaseResistances, ref _cacheIncreaseResistances, level, 1f);
                GameDataHelpers.CombineArmors(buff.increaseArmors, ref _cacheIncreaseArmors, level, 1f);
                GameDataHelpers.CombineArmors(buff.increaseArmorsRate, ref _cacheIncreaseArmorsRate, level, 1f);
                GameDataHelpers.CombineDamages(buff.increaseDamages, ref _cacheIncreaseDamages, level, 1f);
                GameDataHelpers.CombineDamages(buff.increaseDamagesRate, ref _cacheIncreaseDamagesRate, level, 1f);
                buff.GetIncreaseSkills(level, _cacheIncreaseSkills);
                if (buff.isOverrideSkills)
                    buff.GetOverrideSkills(level, _cacheOverrideSkills);
                buff.GetIncreaseStatusEffectResistances(level, _cacheIncreaseStatusEffectResistances);
                buff.GetBuffRemovals(level, _cacheBuffRemovals);
                GameDataHelpers.CombineDamages(buff.damageOverTimes, ref _cacheDamageOverTimes, level, 1f);
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
            else
            {
                buff = Buff.Empty;
            }

            RefreshAllocatedViews();
            if (GameExtensionInstance.onBuildCalculatedBuff != null)
            {
                GetIncreaseAttributes();
                GetIncreaseAttributesRate();
                GetIncreaseResistances();
                GetIncreaseArmors();
                GetIncreaseArmorsRate();
                GetIncreaseDamages();
                GetIncreaseDamagesRate();
                GetDamageOverTimes();
                GameExtensionInstance.onBuildCalculatedBuff(this);
                _cacheIncreaseAttributes = default;
                _cacheIncreaseAttributes.Combine(_viewIncreaseAttributes);
                _cacheIncreaseAttributesRate = default;
                _cacheIncreaseAttributesRate.Combine(_viewIncreaseAttributesRate);
                _cacheIncreaseResistances = default;
                _cacheIncreaseResistances.Combine(_viewIncreaseResistances);
                _cacheIncreaseArmors = default;
                _cacheIncreaseArmors.Combine(_viewIncreaseArmors);
                _cacheIncreaseArmorsRate = default;
                _cacheIncreaseArmorsRate.Combine(_viewIncreaseArmorsRate);
                _cacheIncreaseDamages = default;
                _cacheIncreaseDamages.Combine(_viewIncreaseDamages);
                _cacheIncreaseDamagesRate = default;
                _cacheIncreaseDamagesRate.Combine(_viewIncreaseDamagesRate);
                _cacheDamageOverTimes = default;
                _cacheDamageOverTimes.Combine(_viewDamageOverTimes);
            }
            if (GameExtensionInstance.onBuildCalculatedBuffIndexed != null)
            {
                GameExtensionInstance.onBuildCalculatedBuffIndexed(this);
                RefreshAllocatedViews();
            }
        }

        private void RefreshAllocatedViews()
        {
            if (_viewIncreaseAttributes != null)
                _cacheIncreaseAttributes.CopyTo(_viewIncreaseAttributes);
            if (_viewIncreaseAttributesRate != null)
                _cacheIncreaseAttributesRate.CopyTo(_viewIncreaseAttributesRate);
            if (_viewIncreaseResistances != null)
                _cacheIncreaseResistances.CopyTo(_viewIncreaseResistances);
            if (_viewIncreaseArmors != null)
                _cacheIncreaseArmors.CopyTo(_viewIncreaseArmors);
            if (_viewIncreaseArmorsRate != null)
                _cacheIncreaseArmorsRate.CopyTo(_viewIncreaseArmorsRate);
            if (_viewIncreaseDamages != null)
                _cacheIncreaseDamages.CopyToDictionary(_viewIncreaseDamages);
            if (_viewIncreaseDamagesRate != null)
                _cacheIncreaseDamagesRate.CopyToDictionary(_viewIncreaseDamagesRate);
            if (_viewDamageOverTimes != null)
                _cacheDamageOverTimes.CopyToDictionary(_viewDamageOverTimes);
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
            // Intend to fix duration to 1 if no duration
            return NoDuration() ? 1f : _cacheDuration;
        }

        public bool NoDuration()
        {
            return _buff.noDuration;
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

        public AttributeAmounts GetIndexedIncreaseAttributes() => _cacheIncreaseAttributes;
        public AttributeAmounts GetIndexedIncreaseAttributesRate() => _cacheIncreaseAttributesRate;
        public DamageElementFloatAmounts GetIndexedIncreaseResistances() => _cacheIncreaseResistances;
        public DamageElementFloatAmounts GetIndexedIncreaseArmors() => _cacheIncreaseArmors;
        public DamageElementFloatAmounts GetIndexedIncreaseArmorsRate() => _cacheIncreaseArmorsRate;
        public DamageElementMinMaxFloatAmounts GetIndexedIncreaseDamages() => _cacheIncreaseDamages;
        public DamageElementMinMaxFloatAmounts GetIndexedIncreaseDamagesRate() => _cacheIncreaseDamagesRate;
        public DamageElementMinMaxFloatAmounts GetIndexedDamageOverTimes() => _cacheDamageOverTimes;

        public void SetIndexedIncreaseAttributes(AttributeAmounts value) { _cacheIncreaseAttributes = value; RefreshAllocatedViews(); }
        public void SetIndexedIncreaseAttributesRate(AttributeAmounts value) { _cacheIncreaseAttributesRate = value; RefreshAllocatedViews(); }
        public void SetIndexedIncreaseResistances(DamageElementFloatAmounts value) { _cacheIncreaseResistances = value; RefreshAllocatedViews(); }
        public void SetIndexedIncreaseArmors(DamageElementFloatAmounts value) { _cacheIncreaseArmors = value; RefreshAllocatedViews(); }
        public void SetIndexedIncreaseArmorsRate(DamageElementFloatAmounts value) { _cacheIncreaseArmorsRate = value; RefreshAllocatedViews(); }
        public void SetIndexedIncreaseDamages(DamageElementMinMaxFloatAmounts value) { _cacheIncreaseDamages = value; RefreshAllocatedViews(); }
        public void SetIndexedIncreaseDamagesRate(DamageElementMinMaxFloatAmounts value) { _cacheIncreaseDamagesRate = value; RefreshAllocatedViews(); }
        public void SetIndexedDamageOverTimes(DamageElementMinMaxFloatAmounts value) { _cacheDamageOverTimes = value; RefreshAllocatedViews(); }

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
            if (_viewIncreaseAttributes == null)
            {
                _viewIncreaseAttributes = new Dictionary<Attribute, float>();
                _cacheIncreaseAttributes.CopyTo(_viewIncreaseAttributes);
            }
            return _viewIncreaseAttributes;
        }

        public Dictionary<Attribute, float> GetIncreaseAttributesRate()
        {
            if (_viewIncreaseAttributesRate == null)
            {
                _viewIncreaseAttributesRate = new Dictionary<Attribute, float>();
                _cacheIncreaseAttributesRate.CopyTo(_viewIncreaseAttributesRate);
            }
            return _viewIncreaseAttributesRate;
        }

        public Dictionary<DamageElement, float> GetIncreaseResistances()
        {
            if (_viewIncreaseResistances == null)
            {
                _viewIncreaseResistances = new Dictionary<DamageElement, float>();
                _cacheIncreaseResistances.CopyTo(_viewIncreaseResistances);
            }
            return _viewIncreaseResistances;
        }

        public Dictionary<DamageElement, float> GetIncreaseArmors()
        {
            if (_viewIncreaseArmors == null)
            {
                _viewIncreaseArmors = new Dictionary<DamageElement, float>();
                _cacheIncreaseArmors.CopyTo(_viewIncreaseArmors);
            }
            return _viewIncreaseArmors;
        }

        public Dictionary<DamageElement, float> GetIncreaseArmorsRate()
        {
            if (_viewIncreaseArmorsRate == null)
            {
                _viewIncreaseArmorsRate = new Dictionary<DamageElement, float>();
                _cacheIncreaseArmorsRate.CopyTo(_viewIncreaseArmorsRate);
            }
            return _viewIncreaseArmorsRate;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages()
        {
            if (_viewIncreaseDamages == null)
            {
                _viewIncreaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
                _cacheIncreaseDamages.CopyToDictionary(_viewIncreaseDamages);
            }
            return _viewIncreaseDamages;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamagesRate()
        {
            if (_viewIncreaseDamagesRate == null)
            {
                _viewIncreaseDamagesRate = new Dictionary<DamageElement, MinMaxFloat>();
                _cacheIncreaseDamagesRate.CopyToDictionary(_viewIncreaseDamagesRate);
            }
            return _viewIncreaseDamagesRate;
        }

        public Dictionary<BaseSkill, int> GetIncreaseSkills()
        {
            return _cacheIncreaseSkills;
        }

        public bool IsOverrideDamageInfo()
        {
            return _buff.isOverrideDamageInfo;
        }

        public DamageInfo GetOverrideDamageInfo()
        {
            return _buff.overrideDamageInfo;
        }

        public bool IsOverrideSkills()
        {
            return _buff.isOverrideSkills;
        }

        public Dictionary<BaseSkill, int> GetOverrideSkills()
        {
            return _cacheOverrideSkills;
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
            if (_viewDamageOverTimes == null)
            {
                _viewDamageOverTimes = new Dictionary<DamageElement, MinMaxFloat>();
                _cacheDamageOverTimes.CopyToDictionary(_viewDamageOverTimes);
            }
            return _viewDamageOverTimes;
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
