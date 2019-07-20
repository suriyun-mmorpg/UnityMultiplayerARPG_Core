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
        private AttributeIncremental[] attributes;
        [SerializeField]
        private ResistanceIncremental[] resistances;

        [Header("Skills")]
        [SerializeField]
        private SkillLevel[] skillLevels;

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

        public virtual SkillLevel[] SkillLevels
        {
            get { return skillLevels; }
        }

        private Dictionary<Skill, short> cacheSkillLevels;
        public Dictionary<Skill, short> CacheSkillLevels
        {
            get
            {
                if (cacheSkillLevels == null)
                    cacheSkillLevels = GameDataHelpers.CombineSkills(SkillLevels, new Dictionary<Skill, short>());
                return cacheSkillLevels;
            }
        }

        public CharacterStats GetCharacterStats(short level)
        {
            return Stats.GetCharacterStats(level);
        }

        public Dictionary<Attribute, short> GetCharacterAttributes(short level)
        {
            return GameDataHelpers.CombineAttributes(Attributes, new Dictionary<Attribute, short>(), level, 1f);
        }

        public Dictionary<DamageElement, float> GetCharacterResistances(short level)
        {
            return GameDataHelpers.CombineResistances(Resistances, new Dictionary<DamageElement, float>(), level, 1f);
        }
    }
}
