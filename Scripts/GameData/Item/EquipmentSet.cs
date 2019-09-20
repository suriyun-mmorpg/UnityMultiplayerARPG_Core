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
            EquipmentBonus effect;
            for (int i  = 0; i < effects.Length; ++i)
            {
                effect = effects[i];
                if (GameDataMigration.MigrateArmor(effect.stats, effect.armors, out effect.stats, out effect.armors))
                {
                    effects[i] = effect;
                    hasChanges = true;
                }
            }
            return hasChanges;
        }
    }

    [System.Serializable]
    public struct EquipmentBonus
    {
        public CharacterStats stats;
        public CharacterStats statsRate;
        public AttributeAmount[] attributes;
        public AttributeAmount[] attributesRate;
        public ResistanceAmount[] resistances;
        public ArmorAmount[] armors;
        public DamageAmount[] damages;
        public SkillLevel[] skills;
    }
}
