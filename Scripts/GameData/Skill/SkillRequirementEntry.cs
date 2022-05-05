namespace MultiplayerARPG
{
    [System.Serializable]
    public struct SkillRequirementEntry
    {
        public bool disallow;
        public short characterLevel;
        public float skillPoint;
        public int gold;
        [ArrayElementTitle("attribute")]
        public AttributeAmount[] attributeAmounts;
        [ArrayElementTitle("skill")]
        public SkillLevel[] skillLevels;
        [ArrayElementTitle("currency")]
        public CurrencyAmount[] currencyAmounts;
        [ArrayElementTitle("item")]
        public ItemAmount[] itemAmounts;
    }
}
