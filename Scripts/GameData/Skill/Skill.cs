using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum SkillType
    {
        Active,
        Passive,
        CraftItem,
    }

    public enum SkillAttackType
    {
        None,
        Normal,
        BasedOnWeapon,
    }

    public enum SkillBuffType
    {
        None,
        BuffToUser,
        BuffToNearbyAllies,
        BuffToNearbyCharacters,
    }

    [CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill")]
    public partial class Skill : BaseGameData
    {
        public SkillType skillType;
        [Range(1, 100)]
        public short maxLevel = 1;

        [Header("Available Weapons")]
        [Tooltip("An available weapons, if it not set every weapons is available")]
        public WeaponType[] availableWeapons;

        [Header("Consume Mp")]
        public IncrementalInt consumeMp;

        [Header("Cool Down")]
        public IncrementalFloat coolDownDuration;

        [Header("Requirements")]
        public SkillRequirement requirement;

        [Header("Attack")]
        public SkillAttackType skillAttackType;
        public GameEffectCollection hitEffects;
        public DamageInfo damageInfo;
        public DamageEffectivenessAttribute[] effectivenessAttributes;
        public DamageIncremental damageAmount;
        public DamageInflictionIncremental[] weaponDamageInflictions;
        public DamageIncremental[] additionalDamageAmounts;
        public bool isDebuff;
        public Buff debuff;

        [Header("Buffs")]
        public SkillBuffType skillBuffType;
        public float buffDistance;
        public Buff buff;

        [Header("Craft")]
        public ItemCraft itemCraft;

        private Dictionary<Skill, short> cacheRequireSkillLevels;
        public Dictionary<Skill, short> CacheRequireSkillLevels
        {
            get
            {
                if (cacheRequireSkillLevels == null)
                    cacheRequireSkillLevels = GameDataHelpers.MakeSkillLevelsDictionary(requirement.skillLevels, new Dictionary<Skill, short>());
                return cacheRequireSkillLevels;
            }
        }

        private Dictionary<Attribute, float> cacheEffectivenessAttributes;
        public Dictionary<Attribute, float> CacheEffectivenessAttributes
        {
            get
            {
                if (cacheEffectivenessAttributes == null)
                    cacheEffectivenessAttributes = GameDataHelpers.MakeDamageEffectivenessAttributesDictionary(effectivenessAttributes, new Dictionary<Attribute, float>());
                return cacheEffectivenessAttributes;
            }
        }
    }

[System.Serializable]
    public struct SkillRequirement
    {
        public IncrementalShort characterLevel;
        public SkillLevel[] skillLevels;
    }

    [System.Serializable]
    public struct SkillLevel
    {
        public Skill skill;
        public short level;
    }
}
