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
        [ArrayElementTitle("attribute", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public AttributeAmount[] attributes;
        [ArrayElementTitle("attribute", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public AttributeAmount[] attributesRate;
        [ArrayElementTitle("damageElement", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ResistanceAmount[] resistances;
        [ArrayElementTitle("damageElement", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ArmorAmount[] armors;
        [ArrayElementTitle("damageElement", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public DamageAmount[] damages;
        [ArrayElementTitle("skill", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public SkillLevel[] skills;
    }
}
