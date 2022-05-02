namespace MultiplayerARPG
{
    [System.Serializable]
    public struct SkillRequirementEntry
    {
        public short characterLevel;
        public float skillPoint;
        public int gold;
        [ArrayElementTitle("attribute")]
        public AttributeAmount[] attributeAmounts;
        [ArrayElementTitle("skill")]
        public SkillLevel[] skillLevels;
    }
}
