using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseCharacter : BaseGameData
    {
        [Header("Stats/Attributes")]
        [SerializeField]
        private CharacterStatsIncremental stats;
        [SerializeField]
        [ArrayElementTitle("attribute", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        private AttributeIncremental[] attributes;
        [SerializeField]
        [ArrayElementTitle("damageElement", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        private ResistanceIncremental[] resistances;
        [SerializeField]
        [ArrayElementTitle("damageElement", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        private ArmorIncremental[] armors;

        public virtual CharacterStatsIncremental Stats
        {
            get { return stats; }
        }

        public virtual AttributeIncremental[] Attributes
        {
            get { return attributes; }
        }

        public virtual ResistanceIncremental[] Resistances
        {
            get { return resistances; }
        }

        public virtual ArmorIncremental[] Armors
        {
            get { return armors; }
        }

        public abstract Dictionary<BaseSkill, short> CacheSkillLevels { get; }

        public override bool Validate()
        {
            return GameDataMigration.MigrateArmor(stats, armors, out stats, out armors);
        }

        public CharacterStats GetCharacterStats(short level)
        {
            return Stats.GetCharacterStats(level);
        }

        public Dictionary<Attribute, float> GetCharacterAttributes(short level)
        {
            return GameDataHelpers.CombineAttributes(Attributes, new Dictionary<Attribute, float>(), level, 1f);
        }

        public Dictionary<DamageElement, float> GetCharacterResistances(short level)
        {
            return GameDataHelpers.CombineResistances(Resistances, new Dictionary<DamageElement, float>(), level, 1f);
        }

        public Dictionary<DamageElement, float> GetCharacterArmors(short level)
        {
            return GameDataHelpers.CombineArmors(Armors, new Dictionary<DamageElement, float>(), level, 1f);
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            // Add skills
            List<BaseSkill> skills = new List<BaseSkill>();
            if (CacheSkillLevels.Count > 0)
                skills.AddRange(CacheSkillLevels.Keys);
            GameInstance.AddSkills(skills);
        }
    }
}
