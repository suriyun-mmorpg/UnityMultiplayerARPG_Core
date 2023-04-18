using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class CalculatedItemBuff
    {
        private IEquipmentItem _item;
        private int _level;
        private int _randomSeed;
        private CharacterStats _increaseStats = CharacterStats.Empty;
        private CharacterStats _increaseStatsRate = CharacterStats.Empty;
        private Dictionary<Attribute, float> _increaseAttributes = new Dictionary<Attribute, float>();
        private Dictionary<Attribute, float> _increaseAttributesRate = new Dictionary<Attribute, float>();
        private Dictionary<DamageElement, float> _increaseResistances = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, float> _increaseArmors = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, MinMaxFloat> _increaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
        private Dictionary<BaseSkill, int> _increaseSkills = new Dictionary<BaseSkill, int>();

        public CalculatedItemBuff()
        {

        }

        public CalculatedItemBuff(IEquipmentItem item, int level, int randomSeed)
        {
            Build(item, level, randomSeed);
        }

        public void Build(IEquipmentItem item, int level, int randomSeed)
        {
            this._item = item;
            this._level = level;
            this._randomSeed = randomSeed;

            _increaseStats = CharacterStats.Empty;
            _increaseStatsRate = CharacterStats.Empty;
            _increaseAttributes.Clear();
            _increaseAttributesRate.Clear();
            _increaseResistances.Clear();
            _increaseArmors.Clear();
            _increaseDamages.Clear();
            _increaseSkills.Clear();

            if (item == null || !item.IsEquipment())
                return;

            _increaseStats = item.GetIncreaseStats(level, randomSeed);
            _increaseStatsRate = item.GetIncreaseStatsRate(level, randomSeed);
            item.GetIncreaseAttributes(level, randomSeed, _increaseAttributes);
            item.GetIncreaseAttributesRate(level, randomSeed, _increaseAttributesRate);
            item.GetIncreaseResistances(level, randomSeed, _increaseResistances);
            item.GetIncreaseArmors(level, randomSeed, _increaseArmors);
            item.GetIncreaseDamages(level, randomSeed, _increaseDamages);
            item.GetIncreaseSkills(level, randomSeed, _increaseSkills);
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
            return _increaseStats;
        }

        public CharacterStats GetIncreaseStatsRate()
        {
            return _increaseStatsRate;
        }

        public Dictionary<Attribute, float> GetIncreaseAttributes()
        {
            return _increaseAttributes;
        }

        public Dictionary<Attribute, float> GetIncreaseAttributesRate()
        {
            return _increaseAttributesRate;
        }

        public Dictionary<DamageElement, float> GetIncreaseResistances()
        {
            return _increaseResistances;
        }

        public Dictionary<DamageElement, float> GetIncreaseArmors()
        {
            return _increaseArmors;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages()
        {
            return _increaseDamages;
        }

        public Dictionary<BaseSkill, int> GetIncreaseSkills()
        {
            return _increaseSkills;
        }
    }
}
