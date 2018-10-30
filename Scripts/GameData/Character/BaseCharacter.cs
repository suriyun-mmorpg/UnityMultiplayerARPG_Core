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
        public CharacterStatsIncremental stats;
        public AttributeIncremental[] attributes;
        public ResistanceIncremental[] resistances;

        [Header("Skills")]
        public SkillLevel[] skillLevels;

        private Dictionary<int, SkillLevel> cacheSkillLevels;
        public Dictionary<int, SkillLevel> CacheSkillLevels
        {
            get
            {
                if (cacheSkillLevels == null)
                {
                    cacheSkillLevels = new Dictionary<int, SkillLevel>();
                    foreach (var skillLevel in skillLevels)
                    {
                        if (skillLevel.skill != null)
                            cacheSkillLevels[skillLevel.skill.DataId] = skillLevel;
                    }
                }
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
