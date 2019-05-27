using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseCharacter : BaseGameData
    {
        [Header("Stats/Attributes")]
        [SerializeField]
        public CharacterStatsIncremental stats;
        [SerializeField]
        public AttributeIncremental[] attributes;
        [SerializeField]
        public ResistanceIncremental[] resistances;

        [Header("Skills")]
        [SerializeField]
        private SkillLevel[] skillLevels;

        private Dictionary<Skill, short> cacheSkillLevels;
        public Dictionary<Skill, short> CacheSkillLevels
        {
            get
            {
                if (cacheSkillLevels == null)
                    cacheSkillLevels = GameDataHelpers.CombineSkills(skillLevels, new Dictionary<Skill, short>());
                return cacheSkillLevels;
            }
        }

        public CharacterStats GetCharacterStats(short level)
        {
            return stats.GetCharacterStats(level);
        }

        public Dictionary<Attribute, short> GetCharacterAttributes(short level)
        {
            return GameDataHelpers.CombineAttributes(attributes, new Dictionary<Attribute, short>(), level, 1f);
        }

        public Dictionary<DamageElement, float> GetCharacterResistances(short level)
        {
            return GameDataHelpers.CombineResistances(resistances, new Dictionary<DamageElement, float>(), level, 1f);
        }
    }
}
