using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class CalculatedItemBuff
    {
        private IEquipmentItem _item;
        private int _level;
        private int _randomSeed;
        private byte _version;
        private CharacterStats _cacheIncreaseStats = CharacterStats.Empty;
        private CharacterStats _cacheIncreaseStatsRate = CharacterStats.Empty;
        private Dictionary<Attribute, float> _cacheIncreaseAttributes = new Dictionary<Attribute, float>();
        private Dictionary<Attribute, float> _cacheIncreaseAttributesRate = new Dictionary<Attribute, float>();
        private Dictionary<DamageElement, float> _cacheIncreaseResistances = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, float> _cacheIncreaseArmors = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, float> _cacheIncreaseArmorsRate = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, MinMaxFloat> _cacheIncreaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
        private Dictionary<DamageElement, MinMaxFloat> _cacheIncreaseDamagesRate = new Dictionary<DamageElement, MinMaxFloat>();
        private Dictionary<BaseSkill, int> _cacheIncreaseSkills = new Dictionary<BaseSkill, int>();
        private Dictionary<StatusEffect, float> _cacheIncreaseStatusEffectResistances = new Dictionary<StatusEffect, float>();
        private CalculatedItemRandomBonus _cacheRandomBonus = new CalculatedItemRandomBonus();

        public CalculatedItemBuff()
        {

        }

        public CalculatedItemBuff(IEquipmentItem item, int level, int randomSeed, byte version)
        {
            Build(item, level, randomSeed, version);
        }

        ~CalculatedItemBuff()
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
            _cacheIncreaseStatusEffectResistances.Clear();
            _cacheIncreaseStatusEffectResistances = null;
            _cacheRandomBonus = null;
        }

        public void Clear()
        {
            _cacheIncreaseStats = CharacterStats.Empty;
            _cacheIncreaseStatsRate = CharacterStats.Empty;
            _cacheIncreaseAttributes.Clear();
            _cacheIncreaseAttributesRate.Clear();
            _cacheIncreaseResistances.Clear();
            _cacheIncreaseArmors.Clear();
            _cacheIncreaseArmorsRate.Clear();
            _cacheIncreaseDamages.Clear();
            _cacheIncreaseDamagesRate.Clear();
            _cacheIncreaseSkills.Clear();
            _cacheIncreaseStatusEffectResistances.Clear();
        }

        public void Build(IEquipmentItem item, int level, int randomSeed, byte version)
        {
            // Don't rebuild if it has no difference
            if (_item != null && _item.DataId == item.DataId && _level == level && _randomSeed == randomSeed && _version == version)
                return;

            _item = item;
            _level = level;
            _randomSeed = randomSeed;
            _version = version;

            Clear();

            if (item == null || !item.IsEquipment())
                return;

            _cacheRandomBonus.Build(item, level, randomSeed, version);

            _cacheIncreaseStats = item.GetIncreaseStats(_level) + _cacheRandomBonus.GetIncreaseStats();
            _cacheIncreaseStatsRate = item.GetIncreaseStatsRate(_level) + _cacheRandomBonus.GetIncreaseStatsRate();
            _cacheIncreaseAttributes = GameDataHelpers.CombineAttributes(item.GetIncreaseAttributes(_level, _cacheIncreaseAttributes), _cacheRandomBonus.GetIncreaseAttributes());
            _cacheIncreaseAttributesRate = GameDataHelpers.CombineAttributes(item.GetIncreaseAttributesRate(_level, _cacheIncreaseAttributesRate), _cacheRandomBonus.GetIncreaseAttributesRate());
            _cacheIncreaseResistances = GameDataHelpers.CombineResistances(item.GetIncreaseResistances(_level, _cacheIncreaseResistances), _cacheRandomBonus.GetIncreaseResistances());
            _cacheIncreaseArmors = GameDataHelpers.CombineArmors(item.GetIncreaseArmors(_level, _cacheIncreaseArmors), _cacheRandomBonus.GetIncreaseArmors());
            _cacheIncreaseArmorsRate = GameDataHelpers.CombineArmors(item.GetIncreaseArmorsRate(_level, _cacheIncreaseArmorsRate), _cacheRandomBonus.GetIncreaseArmorsRate());
            _cacheIncreaseDamages = GameDataHelpers.CombineDamages(item.GetIncreaseDamages(_level, _cacheIncreaseDamages), _cacheRandomBonus.GetIncreaseDamages());
            _cacheIncreaseDamagesRate = GameDataHelpers.CombineDamages(item.GetIncreaseDamagesRate(_level, _cacheIncreaseDamagesRate), _cacheRandomBonus.GetIncreaseDamagesRate());
            _cacheIncreaseSkills = GameDataHelpers.CombineSkills(item.GetIncreaseSkills(_level, _cacheIncreaseSkills), _cacheRandomBonus.GetIncreaseSkills());
            // TODO: Implement random bonus for increase status effect resistances
            _cacheIncreaseStatusEffectResistances = item.GetIncreaseStatusEffectResistances(_level, _cacheIncreaseStatusEffectResistances);

            if (GameExtensionInstance.onBuildCalculatedItemBuff != null)
                GameExtensionInstance.onBuildCalculatedItemBuff(this);
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

        public Dictionary<StatusEffect, float> GetIncreaseStatusEffectResistances()
        {
            return _cacheIncreaseStatusEffectResistances;
        }
    }
}
