using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemRandomBonus
    {
        [Tooltip("0 = Unlimit")]
        public int maxRandomStatsAmount;
        public RandomCharacterStats randomCharacterStats;
        public RandomCharacterStats randomCharacterStatsRate;
        public AttributeRandomAmount[] randomAttributeAmounts;
        public AttributeRandomAmount[] randomAttributeAmountRates;
        public ResistanceRandomAmount[] randomResistanceAmounts;
        public ArmorRandomAmount[] randomArmorAmounts;
        public ArmorRandomAmount[] randomArmorAmountRates;
        public DamageRandomAmount[] randomDamageAmounts;
        public DamageRandomAmount[] randomDamageAmountRates;
        public SkillRandomLevel[] randomSkillLevels;

        public void PrepareRelatesData()
        {
            GameInstance.AddAttributes(randomAttributeAmounts);
            GameInstance.AddAttributes(randomAttributeAmountRates);
            GameInstance.AddDamageElements(randomResistanceAmounts);
            GameInstance.AddDamageElements(randomArmorAmounts);
            GameInstance.AddDamageElements(randomArmorAmountRates);
            GameInstance.AddDamageElements(randomDamageAmounts);
            GameInstance.AddDamageElements(randomDamageAmountRates);
            GameInstance.AddSkills(randomSkillLevels);
        }
    }
}
