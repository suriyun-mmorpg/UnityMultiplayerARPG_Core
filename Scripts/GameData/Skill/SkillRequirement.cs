namespace MultiplayerARPG
{
    [System.Serializable]
    public struct SkillRequirement
    {
        public IncrementalShort characterLevel;
        public IncrementalFloat skillPoint;
        public IncrementalInt gold;
        [ArrayElementTitle("attribute")]
        public AttributeAmount[] attributeAmounts;
        [ArrayElementTitle("skill")]
        public SkillLevel[] skillLevels;
    }
}
