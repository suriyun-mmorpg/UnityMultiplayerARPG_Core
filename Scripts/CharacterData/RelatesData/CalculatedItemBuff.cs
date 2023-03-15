using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class CalculatedItemBuff
    {
        private IEquipmentItem item;
        private int level;
        private int randomSeed;
        private KeyValuePair<DamageElement, float> armorAmount = new KeyValuePair<DamageElement, float>();
        private KeyValuePair<DamageElement, MinMaxFloat> damageAmount = new KeyValuePair<DamageElement, MinMaxFloat>();
        private CharacterStats increaseStats = CharacterStats.Empty;
        private CharacterStats increaseStatsRate = CharacterStats.Empty;
        private Dictionary<Attribute, float> increaseAttributes = new Dictionary<Attribute, float>();
        private Dictionary<Attribute, float> increaseAttributesRate = new Dictionary<Attribute, float>();
        private Dictionary<DamageElement, float> increaseResistances = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, float> increaseArmors = new Dictionary<DamageElement, float>();
        private Dictionary<DamageElement, MinMaxFloat> increaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
        private Dictionary<BaseSkill, int> increaseSkills = new Dictionary<BaseSkill, int>();

        public CalculatedItemBuff()
        {

        }

        public CalculatedItemBuff(IEquipmentItem item, int level, int randomSeed)
        {
            Build(item, level, randomSeed);
        }

        public void Build(IEquipmentItem item, int level, int randomSeed)
        {
            this.item = item;
            this.level = level;
            this.randomSeed = randomSeed;

            increaseStats = CharacterStats.Empty;
            increaseStatsRate = CharacterStats.Empty;
            increaseAttributes.Clear();
            increaseAttributesRate.Clear();
            increaseResistances.Clear();
            increaseArmors.Clear();
            increaseDamages.Clear();
            increaseSkills.Clear();

            if (item == null || !item.IsEquipment())
                return;

            increaseStats = item.GetIncreaseStats(level, randomSeed);
            increaseStatsRate = item.GetIncreaseStatsRate(level, randomSeed);
            item.GetIncreaseAttributes(level, randomSeed, increaseAttributes);
            item.GetIncreaseAttributesRate(level, randomSeed, increaseAttributesRate);
            item.GetIncreaseResistances(level, randomSeed, increaseResistances);
            item.GetIncreaseArmors(level, randomSeed, increaseArmors);
            item.GetIncreaseDamages(level, randomSeed, increaseDamages);
            item.GetIncreaseSkills(level, randomSeed, increaseSkills);
        }

        public IEquipmentItem GetItem()
        {
            return item;
        }

        public int GetLevel()
        {
            return level;
        }

        public int GetRandomSeed()
        {
            return randomSeed;
        }

        public CharacterStats GetIncreaseStats()
        {
            return increaseStats;
        }

        public CharacterStats GetIncreaseStatsRate()
        {
            return increaseStatsRate;
        }

        public Dictionary<Attribute, float> GetIncreaseAttributes()
        {
            return increaseAttributes;
        }

        public Dictionary<Attribute, float> GetIncreaseAttributesRate()
        {
            return increaseAttributesRate;
        }

        public Dictionary<DamageElement, float> GetIncreaseResistances()
        {
            return increaseResistances;
        }

        public Dictionary<DamageElement, float> GetIncreaseArmors()
        {
            return increaseArmors;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages()
        {
            return increaseDamages;
        }

        public Dictionary<BaseSkill, int> GetIncreaseSkills()
        {
            return increaseSkills;
        }
    }
}
