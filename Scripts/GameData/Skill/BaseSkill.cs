using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseSkill : BaseGameData
    {
        [Header("Skill Configs")]
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

        private Dictionary<BaseSkill, short> cacheRequireSkillLevels;
        public Dictionary<BaseSkill, short> CacheRequireSkillLevels
        {
            get
            {
                if (cacheRequireSkillLevels == null)
                    cacheRequireSkillLevels = GameDataHelpers.CombineSkills(requirement.skillLevels, new Dictionary<BaseSkill, short>());
                return cacheRequireSkillLevels;
            }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddWeaponTypes(availableWeapons);
        }

        public abstract SkillType GetSkillType();
        public abstract bool IsAttack();
        public abstract bool IsBuff();
        public abstract Buff GetBuff();
        public abstract bool IsDebuff();
        public abstract Buff GetDebuff();
        public abstract BaseMonsterCharacterEntity GetSummonMonsterEntity();
        public abstract MountEntity GetMountEntity();
        public abstract ItemCraft GetItemCraft();
        public abstract GameEffectCollection GetHitEffect();
        public abstract float GetAttackDistance(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand);
        public abstract float GetAttackFov(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand);
        public abstract Dictionary<DamageElement, MinMaxFloat> GetAttackDamages(ICharacterData skillUser, short skillLevel, bool isLeftHand);
        public abstract KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, short skillLevel, bool isLeftHand);
        public abstract Dictionary<DamageElement, float> GetAttackWeaponDamageInflictions(ICharacterData skillUser, short skillLevel);
        public abstract Dictionary<DamageElement, MinMaxFloat> GetAttackAdditionalDamageAmounts(ICharacterData skillUser, short skillLevel);
        public abstract bool HasCustomAimControls();
        public abstract Vector3? UpdateAimControls(short skillLevel);

        /// <summary>
        /// Apply skill
        /// </summary>
        /// <param name="skillUser"></param>
        /// <param name="skillLevel"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="weapon"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="aimPosition"></param>
        /// <returns></returns>
        public abstract void ApplySkill(
            BaseCharacterEntity skillUser,
            short skillLevel,
            bool isLeftHand,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            Vector3 aimPosition);

        /// <summary>
        /// Return TRUE if this will override default attack function
        /// </summary>
        /// <param name="skillUser"></param>
        /// <param name="skillLevel"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="weapon"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="aimPosition"></param>
        /// <returns></returns>
        public virtual bool OnAttack(
            BaseCharacterEntity skillUser,
            short skillLevel,
            bool isLeftHand,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            Vector3 aimPosition)
        {
            return false;
        }
    }

    public enum SkillType : byte
    {
        Active,
        Passive,
        CraftItem,
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
        public BaseSkill skill;
        public short level;
    }

    [System.Serializable]
    public struct MonsterSkill
    {
        public BaseSkill skill;
        public short level;
        [Range(0.01f, 1f)]
        [Tooltip("Monster will random to use skill by this rate")]
        public float useRate;
        [Range(0f, 1f)]
        [Tooltip("Monster will use skill only when its Hp lower than this rate")]
        public float useWhenHpRate;
    }
}
