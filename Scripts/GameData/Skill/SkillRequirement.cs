namespace MultiplayerARPG
{
    [System.Serializable]
    public partial class SkillRequirement
    {
        public bool disallow = false;
        public IncrementalInt characterLevel = default;
        public IncrementalFloat skillPoint = default;
        public IncrementalInt gold = default;
        [ArrayElementTitle("attribute")]
        public AttributeAmount[] attributeAmounts = new AttributeAmount[0];
        [ArrayElementTitle("skill")]
        public SkillLevel[] skillLevels = new SkillLevel[0];
        [ArrayElementTitle("currency")]
        public CurrencyAmount[] currencyAmounts = new CurrencyAmount[0];
        [ArrayElementTitle("item")]
        public ItemAmount[] itemAmounts = new ItemAmount[0];
    }
}
