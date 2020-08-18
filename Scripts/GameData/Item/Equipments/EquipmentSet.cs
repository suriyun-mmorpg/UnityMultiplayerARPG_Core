using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Equipment Set", menuName = "Create GameData/Equipment Set", order = -4897)]
    public class EquipmentSet : BaseGameData
    {
        [Header("Equipment Set Configs")]
        public EquipmentBonus[] effects;

        public override bool Validate()
        {
            bool hasChanges = false;
            if (effects != null && effects.Length > 0)
            {
                EquipmentBonus effect;
                for (int i = 0; i < effects.Length; ++i)
                {
                    effect = effects[i];
                    if (GameDataMigration.MigrateArmor(effect.stats, effect.armors, out effect.stats, out effect.armors))
                    {
                        effects[i] = effect;
                        hasChanges = true;
                    }
                }
            }
            return hasChanges;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            if (effects != null && effects.Length > 0)
            {
                foreach (EquipmentBonus effect in effects)
                {
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
