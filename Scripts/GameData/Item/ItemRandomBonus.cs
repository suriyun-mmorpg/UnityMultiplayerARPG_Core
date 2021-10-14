namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemRandomBonus
    {
        public AttributeRandomAmount[] randomAttributeAmounts;
        public AttributeRandomAmount[] randomAttributeAmountRates;
        public ResistanceRandomAmount[] randomResistanceAmounts;
        public ArmorRandomAmount[] randomArmorAmounts;
        public DamageRandomAmount[] randomDamageAmounts;
        public SkillRandomLevel[] randomSkillLevels;

        public void PrepareRelatesData()
        {
            GameInstance.AddAttributes(randomAttributeAmounts);
            GameInstance.AddAttributes(randomAttributeAmountRates);
            GameInstance.AddDamageElements(randomResistanceAmounts);
            GameInstance.AddDamageElements(randomArmorAmounts);
            GameInstance.AddDamageElements(randomDamageAmounts);
            GameInstance.AddSkills(randomSkillLevels);
        }
    }
}
