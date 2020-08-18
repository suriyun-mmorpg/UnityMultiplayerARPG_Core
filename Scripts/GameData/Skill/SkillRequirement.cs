namespace MultiplayerARPG
{
    [System.Serializable]
    public struct SkillRequirement
    {
        public IncrementalShort characterLevel;
        [ArrayElementTitle("attribute")]
        public AttributeAmount[] attributeAmounts;
        [ArrayElementTitle("skill")]
        public SkillLevel[] skillLevels;
    }
}
