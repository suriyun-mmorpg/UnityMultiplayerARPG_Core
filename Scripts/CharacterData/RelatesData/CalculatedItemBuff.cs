using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class CalculatedItemBuff
    {
        private IEquipmentItem _item;
        private int _level;
        private int _randomSeed;
        private CharacterStats _cacheIncreaseStats = CharacterStats.Empty;
        private CharacterStats _cacheIncreaseStatsRate = CharacterStats.Empty;
        private Dictionary<Attribute, float> _cacheIncreaseAttributes = new Dictionary<Attribute, float>();
        private Dictionary<Attribute, float> _cacheIncreaseAttributesRate = new Dictionary<Attribute, float>();
        private Dictionary<DamageElement, float> _cacheIncreaseResistances = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, float> _cacheIncreaseArmors = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, MinMaxFloat> _cacheIncreaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
        private Dictionary<BaseSkill, int> _cacheIncreaseSkills = new Dictionary<BaseSkill, int>();

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
            _cacheIncreaseDamages.Clear();
            _cacheIncreaseDamages = null;
            _cacheIncreaseSkills.Clear();
            _cacheIncreaseSkills = null;
        }

        public void Build(IEquipmentItem item, int level, int randomSeed, byte version)
        {
            _item = item;
            _level = level;
            _randomSeed = randomSeed;

            _cacheIncreaseStats = CharacterStats.Empty;
            _cacheIncreaseStatsRate = CharacterStats.Empty;
            _cacheIncreaseAttributes.Clear();
            _cacheIncreaseAttributesRate.Clear();
            _cacheIncreaseResistances.Clear();
            _cacheIncreaseArmors.Clear();
            _cacheIncreaseDamages.Clear();
            _cacheIncreaseSkills.Clear();

            if (item == null || !item.IsEquipment())
                return;

            _cacheIncreaseStats = item.GetIncreaseStats(level, randomSeed, version);
            _cacheIncreaseStatsRate = item.GetIncreaseStatsRate(level, randomSeed, version);
            item.GetIncreaseAttributes(level, randomSeed, version, _cacheIncreaseAttributes);
            item.GetIncreaseAttributesRate(level, randomSeed, version, _cacheIncreaseAttributesRate);
            item.GetIncreaseResistances(level, randomSeed, version, _cacheIncreaseResistances);
            item.GetIncreaseArmors(level, randomSeed, version, _cacheIncreaseArmors);
            item.GetIncreaseDamages(level, randomSeed, version, _cacheIncreaseDamages);
            item.GetIncreaseSkills(level, randomSeed, version, _cacheIncreaseSkills);
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

        public Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages()
        {
            return _cacheIncreaseDamages;
        }

        public Dictionary<BaseSkill, int> GetIncreaseSkills()
        {
            return _cacheIncreaseSkills;
        }
    }
}
