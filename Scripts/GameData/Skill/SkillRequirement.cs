namespace MultiplayerARPG
{
    [System.Serializable]
    public partial class SkillRequirement
    {
        public bool disallow;
        public IncrementalInt characterLevel;
        public IncrementalFloat skillPoint;
        public IncrementalInt gold;
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
