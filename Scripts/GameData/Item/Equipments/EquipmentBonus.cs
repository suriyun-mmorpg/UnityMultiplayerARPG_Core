namespace MultiplayerARPG
{
    [System.Serializable]
    public struct EquipmentBonus
    {
        public CharacterStats stats;
        public CharacterStats statsRate;
        [ArrayElementTitle("attribute")]
        public AttributeAmount[] attributes;
        [ArrayElementTitle("attribute")]
        public AttributeAmount[] attributesRate;
        [ArrayElementTitle("damageElement")]
        public ResistanceAmount[] resistances;
        [ArrayElementTitle("damageElement")]
        public ArmorAmount[] armors;
        [ArrayElementTitle("damageElement")]
        public ArmorAmount[] armorsRate;
        [ArrayElementTitle("damageElement")]
        public DamageAmount[] damages;
        [ArrayElementTitle("damageElement")]
        public DamageAmount[] damagesRate;
        [ArrayElementTitle("skill")]
        public SkillLevel[] skills;
    }
}
