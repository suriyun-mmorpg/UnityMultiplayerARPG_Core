using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Equipment Set", menuName = "Create GameData/Equipment Set")]
    public class EquipmentSet : BaseGameData
    {
        public EquipmentSetEffect[] effects;
    }

    [System.Serializable]
    public struct EquipmentSetEffect
    {
        public AttributeAmount[] attributes;
        public ResistanceAmount[] resistances;
        public DamageAmount[] damages;
        public CharacterStats stats;
        public SkillLevel[] skills;
    }
}
