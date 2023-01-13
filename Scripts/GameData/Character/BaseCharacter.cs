using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseCharacter : BaseGameData
    {
        [Category(3, "Character Stats")]
        [SerializeField]
        private CharacterStatsIncremental stats;
        public virtual CharacterStatsIncremental Stats { get { return stats; } set { stats = value; } }
        [SerializeField]
        [ArrayElementTitle("attribute")]
        private AttributeIncremental[] attributes;
        public virtual AttributeIncremental[] Attributes { get { return attributes; } set { attributes = value; } }
        [SerializeField]
        [ArrayElementTitle("damageElement")]
        private ResistanceIncremental[] resistances;
        public virtual ResistanceIncremental[] Resistances { get { return resistances; } set { resistances = value; } }
        [SerializeField]
        [ArrayElementTitle("damageElement")]
        private ArmorIncremental[] armors;
        public virtual ArmorIncremental[] Armors { get { return armors; } set { armors = value; } }

        public abstract Dictionary<BaseSkill, int> CacheSkillLevels { get; }

        public CharacterStats GetCharacterStats(int level)
        {
            return Stats.GetCharacterStats(level);
        }

        public Dictionary<Attribute, float> GetCharacterAttributes(int level)
        {
            return GameDataHelpers.CombineAttributes(Attributes, new Dictionary<Attribute, float>(), level, 1f);
        }

        public Dictionary<DamageElement, float> GetCharacterResistances(int level)
        {
            return GameDataHelpers.CombineResistances(Resistances, new Dictionary<DamageElement, float>(), level, 1f);
        }

        public Dictionary<DamageElement, float> GetCharacterArmors(int level)
        {
            return GameDataHelpers.CombineArmors(Armors, new Dictionary<DamageElement, float>(), level, 1f);
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddAttributes(Attributes);
            GameInstance.AddDamageElements(Resistances);
            GameInstance.AddDamageElements(Armors);
            GameInstance.AddSkills(CacheSkillLevels.Keys);
        }
    }
}
