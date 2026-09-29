using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class CalculatedItemBuff
    {
        private IEquipmentItem _item;
        private int _level;
        private int _randomSeed;
        private byte _version;
        private int _runtimeSlotGeneration = -1;
        private CharacterStats _cacheIncreaseStats = new CharacterStats();
        private CharacterStats _cacheIncreaseStatsRate = new CharacterStats();
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
        private readonly Dictionary<StatusEffect, float> _cacheIncreaseStatusEffectResistances;
        private CalculatedItemRandomBonus _cacheRandomBonus = new CalculatedItemRandomBonus();

        public CalculatedItemBuff()
        {
            _cacheIncreaseSkills = new Dictionary<BaseSkill, int>();
            _cacheIncreaseStatusEffectResistances = new Dictionary<StatusEffect, float>();
        }

        public CalculatedItemBuff(IEquipmentItem item, int level, int randomSeed, byte version)
        {
            _cacheIncreaseSkills = new Dictionary<BaseSkill, int>();
            _cacheIncreaseStatusEffectResistances = new Dictionary<StatusEffect, float>();
            Build(item, level, randomSeed, version);
        }

        ~CalculatedItemBuff()
        {
            Clear();
            _cacheRandomBonus = null;
        }

        public void Clear()
        {
            _cacheIncreaseStats = new CharacterStats();
            _cacheIncreaseStatsRate = new CharacterStats();
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
            _cacheIncreaseStatusEffectResistances?.Clear();
            _cacheRandomBonus?.Clear();
        }

        public void Build(IEquipmentItem item, int level, int randomSeed, byte version)
        {
            // Don't rebuild if it has no difference
            if (_item != null && item != null && _item.DataId == item.DataId && _level == level && _randomSeed == randomSeed && _version == version &&
                _runtimeSlotGeneration == RuntimeGameDataSlots.Generation)
                return;

            _item = item;
            _level = level;
            _randomSeed = randomSeed;
            _version = version;

            Clear();
            _runtimeSlotGeneration = RuntimeGameDataSlots.Generation;

            if (item == null || !item.IsEquipment())
                return;

            _cacheRandomBonus.Build(item, level, randomSeed, version);

            _cacheIncreaseStats = item.GetIncreaseStats(_level) + _cacheRandomBonus.GetIncreaseStats();
            _cacheIncreaseStatsRate = item.GetIncreaseStatsRate(_level) + _cacheRandomBonus.GetIncreaseStatsRate();
            GameDataHelpers.CombineAttributes(item.IncreaseAttributes, ref _cacheIncreaseAttributes, _level, 1f);
            _cacheIncreaseAttributes.Combine(_cacheRandomBonus.GetIndexedIncreaseAttributes());
            GameDataHelpers.CombineAttributes(item.IncreaseAttributesRate, ref _cacheIncreaseAttributesRate, _level, 1f);
            _cacheIncreaseAttributesRate.Combine(_cacheRandomBonus.GetIndexedIncreaseAttributesRate());
            GameDataHelpers.CombineResistances(item.IncreaseResistances, ref _cacheIncreaseResistances, _level, 1f);
            _cacheIncreaseResistances.Combine(_cacheRandomBonus.GetIndexedIncreaseResistances());
            GameDataHelpers.CombineArmors(item.IncreaseArmors, ref _cacheIncreaseArmors, _level, 1f);
            _cacheIncreaseArmors.Combine(_cacheRandomBonus.GetIndexedIncreaseArmors());
            GameDataHelpers.CombineArmors(item.IncreaseArmorsRate, ref _cacheIncreaseArmorsRate, _level, 1f);
            _cacheIncreaseArmorsRate.Combine(_cacheRandomBonus.GetIndexedIncreaseArmorsRate());
            GameDataHelpers.CombineDamages(item.IncreaseDamages, ref _cacheIncreaseDamages, _level, 1f);
            _cacheIncreaseDamages.Combine(_cacheRandomBonus.GetIndexedIncreaseDamages());
            GameDataHelpers.CombineDamages(item.IncreaseDamagesRate, ref _cacheIncreaseDamagesRate, _level, 1f);
            _cacheIncreaseDamagesRate.Combine(_cacheRandomBonus.GetIndexedIncreaseDamagesRate());
            item.GetIncreaseSkills(_level, _cacheIncreaseSkills);
            GameDataHelpers.CombineSkills(_cacheIncreaseSkills, _cacheRandomBonus.GetIncreaseSkills());
            // TODO: Implement random bonus for increase status effect resistances
            item.GetIncreaseStatusEffectResistances(_level, _cacheIncreaseStatusEffectResistances);

            RefreshAllocatedViews();
            if (GameExtensionInstance.onBuildCalculatedItemBuff != null)
            {
                GetIncreaseAttributes();
                GetIncreaseAttributesRate();
                GetIncreaseResistances();
                GetIncreaseArmors();
                GetIncreaseArmorsRate();
                GetIncreaseDamages();
                GetIncreaseDamagesRate();
                GameExtensionInstance.onBuildCalculatedItemBuff(this);
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
            }
            if (GameExtensionInstance.onBuildCalculatedItemBuffIndexed != null)
            {
                GameExtensionInstance.onBuildCalculatedItemBuffIndexed(this);
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
        }

        public IEquipmentItem GetItem()
        {
            return _item;
        }

        public int GetLevel()
        {
            return _level;
        }

        public int GetRandomSeed()
        {
            return _randomSeed;
        }

        public AttributeAmounts GetIndexedIncreaseAttributes() => _cacheIncreaseAttributes;
        public AttributeAmounts GetIndexedIncreaseAttributesRate() => _cacheIncreaseAttributesRate;
        public DamageElementFloatAmounts GetIndexedIncreaseResistances() => _cacheIncreaseResistances;
        public DamageElementFloatAmounts GetIndexedIncreaseArmors() => _cacheIncreaseArmors;
        public DamageElementFloatAmounts GetIndexedIncreaseArmorsRate() => _cacheIncreaseArmorsRate;
        public DamageElementMinMaxFloatAmounts GetIndexedIncreaseDamages() => _cacheIncreaseDamages;
        public DamageElementMinMaxFloatAmounts GetIndexedIncreaseDamagesRate() => _cacheIncreaseDamagesRate;

        public void SetIndexedIncreaseAttributes(AttributeAmounts value) { _cacheIncreaseAttributes = value; RefreshAllocatedViews(); }
        public void SetIndexedIncreaseAttributesRate(AttributeAmounts value) { _cacheIncreaseAttributesRate = value; RefreshAllocatedViews(); }
        public void SetIndexedIncreaseResistances(DamageElementFloatAmounts value) { _cacheIncreaseResistances = value; RefreshAllocatedViews(); }
        public void SetIndexedIncreaseArmors(DamageElementFloatAmounts value) { _cacheIncreaseArmors = value; RefreshAllocatedViews(); }
        public void SetIndexedIncreaseArmorsRate(DamageElementFloatAmounts value) { _cacheIncreaseArmorsRate = value; RefreshAllocatedViews(); }
        public void SetIndexedIncreaseDamages(DamageElementMinMaxFloatAmounts value) { _cacheIncreaseDamages = value; RefreshAllocatedViews(); }
        public void SetIndexedIncreaseDamagesRate(DamageElementMinMaxFloatAmounts value) { _cacheIncreaseDamagesRate = value; RefreshAllocatedViews(); }

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

        public Dictionary<StatusEffect, float> GetIncreaseStatusEffectResistances()
        {
            return _cacheIncreaseStatusEffectResistances;
        }
    }
}
