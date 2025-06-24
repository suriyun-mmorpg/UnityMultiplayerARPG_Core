using Insthync.UnityEditorUtils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class EquipmentBonus
    {
        public CharacterStats stats = new CharacterStats();
        public CharacterStats statsRate = new CharacterStats();
        [ArrayElementTitle("attribute")]
        public AttributeAmount[] attributes = new AttributeAmount[0];
        [ArrayElementTitle("attribute")]
        public AttributeAmount[] attributesRate = new AttributeAmount[0];
        [ArrayElementTitle("damageElement")]
        public ResistanceAmount[] resistances = new ResistanceAmount[0];
        [ArrayElementTitle("damageElement")]
        public ArmorAmount[] armors = new ArmorAmount[0];
        [ArrayElementTitle("damageElement")]
        public ArmorAmount[] armorsRate = new ArmorAmount[0];
        [ArrayElementTitle("damageElement")]
        public DamageAmount[] damages = new DamageAmount[0];
        [ArrayElementTitle("damageElement")]
        public DamageAmount[] damagesRate = new DamageAmount[0];
        [ArrayElementTitle("skill")]
        public SkillLevel[] skills = new SkillLevel[0];
        [ArrayElementTitle("statusEffect")]
        public StatusEffectResistanceAmount[] statusEffectResistances = new StatusEffectResistanceAmount[0];
    }
}
