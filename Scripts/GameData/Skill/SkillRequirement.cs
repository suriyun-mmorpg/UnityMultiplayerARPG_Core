namespace MultiplayerARPG
{
    [System.Serializable]
    public class SkillRequirement
    {
        public IncrementalShort characterLevel = new IncrementalShort() { baseAmount = 0, amountIncreaseEachLevel = 0 };
        public IncrementalFloat skillPoint = new IncrementalFloat() { baseAmount = 1, amountIncreaseEachLevel = 0 };
        public IncrementalInt gold = new IncrementalInt() { baseAmount = 0, amountIncreaseEachLevel = 0 };
        [ArrayElementTitle("attribute")]
        public AttributeAmount[] attributeAmounts;
        [ArrayElementTitle("skill")]
        public SkillLevel[] skillLevels;
    }
}
