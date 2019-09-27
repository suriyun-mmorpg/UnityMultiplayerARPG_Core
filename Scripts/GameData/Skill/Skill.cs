using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public enum SkillType
    {
        Active,
        Passive,
        CraftItem,
    }

    public enum SkillDamageType
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

    [CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill", order = -4996)]
    public partial class Skill : BaseGameData
    {
        [Header("Skill Configs")]
        public SkillType skillType;
        [Range(1, 100)]
        public short maxLevel = 1;
        [Range(0f, 1f)]
        [Tooltip("This is move speed rate while using this skill")]
        public float moveSpeedRateWhileUsingSkill = 0f;

        [Header("Casting Effects")]
        public GameEffectCollection castEffects;
        public bool canBeInterruptedWhileCasting;
        public IncrementalFloat castDuration;

        [Header("Available Weapons")]
        [Tooltip("An available weapons, if it not set every weapons is available")]
        public WeaponType[] availableWeapons;

        [Header("Consume Mp")]
        public IncrementalInt consumeMp;

        [Header("Cool Down")]
        public IncrementalFloat coolDownDuration;

        [Header("Requirements to Levelup")]
        public SkillRequirement requirement;

        [Header("Attack")]
        [FormerlySerializedAs("skillAttackType")]
        public SkillDamageType skillDamageType;
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
        public IncrementalFloat buffDistance;
        public Buff buff;

        [Header("Summon")]
        public SkillSummon summon;

        [Header("Mount")]
        public SkillMount mount;

        [Header("Craft")]
        public ItemCraft itemCraft;

        private Dictionary<Attribute, float> cacheRequireAttributeAmounts;
        public Dictionary<Attribute, float> CacheRequireAttributeAmounts
        {
            get
            {
                if (cacheRequireAttributeAmounts == null)
                    cacheRequireAttributeAmounts = GameDataHelpers.CombineAttributes(requirement.attributeAmounts, new Dictionary<Attribute, float>(), 1f);
                return cacheRequireAttributeAmounts;
            }
        }

        private Dictionary<Skill, short> cacheRequireSkillLevels;
        public Dictionary<Skill, short> CacheRequireSkillLevels
        {
            get
            {
                if (cacheRequireSkillLevels == null)
                    cacheRequireSkillLevels = GameDataHelpers.CombineSkills(requirement.skillLevels, new Dictionary<Skill, short>());
                return cacheRequireSkillLevels;
            }
        }

        private Dictionary<Attribute, float> cacheEffectivenessAttributes;
        public Dictionary<Attribute, float> CacheEffectivenessAttributes
        {
            get
            {
                if (cacheEffectivenessAttributes == null)
                    cacheEffectivenessAttributes = GameDataHelpers.CombineDamageEffectivenessAttributes(effectivenessAttributes, new Dictionary<Attribute, float>());
                return cacheEffectivenessAttributes;
            }
        }

        public override bool Validate()
        {
            return GameDataMigration.MigrateBuffArmor(buff, out buff) ||
                GameDataMigration.MigrateBuffArmor(debuff, out debuff);
        }

        /// <summary>
        /// Return TRUE if this will override default apply skill function
        /// </summary>
        /// <param name="character"></param>
        /// <param name="skillLevel"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="weapon"></param>
        /// <param name="damageInfo"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="hasAimPosition"></param>
        /// <param name="aimPosition"></param>
        /// <returns></returns>
        public virtual bool OnApplySkill(
            BaseCharacterEntity character,
            short skillLevel,
            bool isLeftHand,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            bool hasAimPosition,
            Vector3 aimPosition)
        {
            return false;
        }

        /// <summary>
        /// Return TRUE if this will override default attack function
        /// </summary>
        /// <param name="character"></param>
        /// <param name="skillLevel"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="weapon"></param>
        /// <param name="damageInfo"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="aimPosition"></param>
        /// <returns></returns>
        public virtual bool OnAttack(
            BaseCharacterEntity character,
            short skillLevel,
            bool isLeftHand,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            Vector3 aimPosition)
        {
            return false;
        }
    }

    [System.Serializable]
    public struct SkillRequirement
    {
        public IncrementalShort characterLevel;
        public AttributeAmount[] attributeAmounts;
        public SkillLevel[] skillLevels;
    }

    [System.Serializable]
    public struct SkillLevel
    {
        public Skill skill;
        public short level;
    }

    [System.Serializable]
    public struct MonsterSkill
    {
        public Skill skill;
        public short level;
        [Range(0.01f, 1f)]
        [Tooltip("Monster will random to use skill by this rate")]
        public float useRate;
        [Range(0f, 1f)]
        [Tooltip("Monster will use skill only when its Hp lower than this rate")]
        public float useWhenHpRate;
    }
}
