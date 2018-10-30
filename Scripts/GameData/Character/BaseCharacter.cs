using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseCharacter : BaseGameData
    {
        [Header("Entity (Going to be deprecated)")]
        [System.Obsolete("BaseCharacter -> entityPrefab is going to be deprecated, it will be removed on next version")]
        public BaseCharacterEntity entityPrefab;

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
                    cacheSkillLevels = GameDataHelpers.MakeSkillLevelsDictionary(skillLevels, new Dictionary<Skill, short>());
                return cacheSkillLevels;
            }
        }

        public CharacterStats GetCharacterStats(short level)
        {
            return stats.GetCharacterStats(level);
        }

        public Dictionary<Attribute, short> GetCharacterAttributes(short level)
        {
            return GameDataHelpers.MakeAttributeAmountsDictionary(attributes, new Dictionary<Attribute, short>(), level, 1f);
        }

        public Dictionary<DamageElement, float> GetCharacterResistances(short level)
        {
            return GameDataHelpers.MakeResistanceAmountsDictionary(resistances, new Dictionary<DamageElement, float>(), level, 1f);
        }
    }
}
