namespace MultiplayerARPG
{
    [System.Serializable]
    public class SkillRequirement
    {
        public bool disallow;
        public IncrementalInt characterLevel = new IncrementalInt() { baseAmount = 0, amountIncreaseEachLevel = 0 };
        public IncrementalFloat skillPoint = new IncrementalFloat() { baseAmount = 1, amountIncreaseEachLevel = 0 };
        public IncrementalInt gold = new IncrementalInt() { baseAmount = 0, amountIncreaseEachLevel = 0 };
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
