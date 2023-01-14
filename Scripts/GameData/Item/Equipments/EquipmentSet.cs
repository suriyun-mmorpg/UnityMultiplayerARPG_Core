using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.EQUIPMENT_SET_FILE, menuName = GameDataMenuConsts.EQUIPMENT_SET_MENU, order = GameDataMenuConsts.EQUIPMENT_SET_ORDER)]
    public partial class EquipmentSet : BaseGameData
    {
        [Category("Equipment Set Settings")]
        [SerializeField]
        private EquipmentBonus[] effects = new EquipmentBonus[0];
        public EquipmentBonus[] Effects { get { return effects; } }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            if (effects != null && effects.Length > 0)
            {
                foreach (EquipmentBonus effect in effects)
                {
                    GameInstance.AddAttributes(effect.attributes);
                    GameInstance.AddAttributes(effect.attributesRate);
                    GameInstance.AddDamageElements(effect.resistances);
                    GameInstance.AddDamageElements(effect.resistances);
                    GameInstance.AddDamageElements(effect.armors);
                    GameInstance.AddDamageElements(effect.damages);
                    GameInstance.AddSkills(effect.skills);
                }
            }
        }
    }

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
        public DamageAmount[] damages;
        [ArrayElementTitle("skill")]
        public SkillLevel[] skills;
    }
}
