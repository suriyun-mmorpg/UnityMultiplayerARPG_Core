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
        [Range(0f, 1f)]
        [Tooltip("This is move speed rate while using this skill")]
        public float moveSpeedRateWhileUsingSkill = 0f;

        [Header("Casting Effects")]
        public GameEffectCollection castingEffects;

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
        public IncrementalFloat buffDistance;
        public Buff buff;

        [Header("Summon")]
        public Summon summon;

        [Header("Craft")]
        public ItemCraft itemCraft;

        private Dictionary<Attribute, short> cacheRequireAttributeAmounts;
        public Dictionary<Attribute, short> CacheRequireAttributeAmounts
        {
            get
            {
                if (cacheRequireAttributeAmounts == null)
                    cacheRequireAttributeAmounts = GameDataHelpers.MakeAttributes(requirement.attributeAmounts, new Dictionary<Attribute, short>(), 1f);
                return cacheRequireAttributeAmounts;
            }
        }

        private Dictionary<Skill, short> cacheRequireSkillLevels;
        public Dictionary<Skill, short> CacheRequireSkillLevels
        {
            get
            {
                if (cacheRequireSkillLevels == null)
                    cacheRequireSkillLevels = GameDataHelpers.MakeSkills(requirement.skillLevels, new Dictionary<Skill, short>());
                return cacheRequireSkillLevels;
            }
        }

        private Dictionary<Attribute, float> cacheEffectivenessAttributes;
        public Dictionary<Attribute, float> CacheEffectivenessAttributes
        {
            get
            {
                if (cacheEffectivenessAttributes == null)
                    cacheEffectivenessAttributes = GameDataHelpers.MakeDamageEffectivenessAttributes(effectivenessAttributes, new Dictionary<Attribute, float>());
                return cacheEffectivenessAttributes;
            }
        }

        /// <summary>
        /// Return TRUE if this will override default cast function
        /// </summary>
        /// <param name="character"></param>
        /// <param name="skillLevel"></param>
        /// <param name="triggerDuration"></param>
        /// <param name="totalDuration"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="weapon"></param>
        /// <param name="damageInfo"></param>
        /// <param name="allDamageAmounts"></param>
        /// <param name="hasAimPosition"></param>
        /// <param name="aimPosition"></param>
        /// <returns></returns>
        public virtual bool OnCastSkill(
            BaseCharacterEntity character,
            short skillLevel,
            float triggerDuration,
            float totalDuration,
            bool isLeftHand,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            bool hasAimPosition,
            Vector3 aimPosition)
        {
            return false;
        }

        /// <summary>
        /// Return TRUE if this will override default apply skill function
        /// </summary>
        /// <param name="character"></param>
        /// <param name="skillLevel"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="weapon"></param>
        /// <param name="damageInfo"></param>
        /// <param name="allDamageAmounts"></param>
        /// <param name="hasAimPosition"></param>
        /// <param name="aimPosition"></param>
        /// <returns></returns>
        public virtual bool OnApplySkill(
            BaseCharacterEntity character,
            short skillLevel,
            bool isLeftHand,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
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
        /// <param name="triggerDuration"></param>
        /// <param name="totalDuration"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="weapon"></param>
        /// <param name="damageInfo"></param>
        /// <param name="allDamageAmounts"></param>
        /// <param name="hasAimPosition"></param>
        /// <param name="aimPosition"></param>
        /// <returns></returns>
        public virtual bool OnAttack(
            BaseCharacterEntity character,
            short skillLevel,
            float triggerDuration,
            float totalDuration,
            bool isLeftHand,
            CharacterItem weapon,
            DamageInfo damageInfo,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            bool hasAimPosition,
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
}
