namespace MultiplayerARPG
{
    [System.Serializable]
    public struct EquipmentRequirement
    {
        public PlayerCharacter character;
        public short level;
        [ArrayElementTitle("attribute")]
        public AttributeAmount[] attributeAmounts;
    }
}
