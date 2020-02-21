namespace MultiplayerARPG
{
    [System.Serializable]
    public struct SkillRequirement
    {
        public IncrementalShort characterLevel;
        [ArrayElementTitle("attribute", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public AttributeAmount[] attributeAmounts;
        [ArrayElementTitle("skill", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public SkillLevel[] skillLevels;
    }
}
