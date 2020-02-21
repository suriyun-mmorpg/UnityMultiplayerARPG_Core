namespace MultiplayerARPG
{
    [System.Serializable]
    public struct EquipmentRequirement
    {
        public PlayerCharacter character;
        public short level;
        [ArrayElementTitle("attribute", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public AttributeAmount[] attributeAmounts;
    }
}
