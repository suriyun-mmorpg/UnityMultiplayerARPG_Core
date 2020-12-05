using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseSkill : BaseGameData, ICustomAimController
    {
        [Header("Skill Configs")]
        [Range(1, 100)]
        public short maxLevel = 1;
        [Range(0f, 1f)]
        [Tooltip("This is move speed rate while using this skill")]
        public float moveSpeedRateWhileUsingSkill = 0f;
        
        [Header("Casting Effects")]
        public GameEffect[] skillCastEffects;
        public IncrementalFloat castDuration;
        public bool canBeInterruptedWhileCasting;
        
        [Header("Casted Effects")]
        public GameEffect[] damageHitEffects;

        [Header("Available Weapons")]
        [Tooltip("An available weapons, if it not set every weapons is available")]
        public WeaponType[] availableWeapons;

        [Header("Consume Hp")]
        public IncrementalInt consumeHp;

        [Header("Consume Mp")]
        public IncrementalInt consumeMp;

        [Header("Consume Stamina")]
        public IncrementalInt consumeStamina;

        [Header("Cool Down")]
        public IncrementalFloat coolDownDuration;

        [Header("Requirements to Levelup")]
        public SkillRequirement requirement;

        public virtual string TypeTitle
        {
            get
            {
                switch (SkillType)
                {
                    case SkillType.Active:
                        return LanguageManager.GetText(UISkillTypeKeys.UI_SKILL_TYPE_ACTIVE.ToString());
                    case SkillType.Passive:
                        return LanguageManager.GetText(UISkillTypeKeys.UI_SKILL_TYPE_PASSIVE.ToString());
                    case SkillType.CraftItem:
                        return LanguageManager.GetText(UISkillTypeKeys.UI_SKILL_TYPE_CRAFT_ITEM.ToString());
                    default:
                        return LanguageManager.GetUnknowTitle();
                }
            }
        }

        [System.NonSerialized]
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

        [System.NonSerialized]
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

        [System.NonSerialized]
        private bool alreadySetAvailableWeaponsText;
        [System.NonSerialized]
        private string availableWeaponsText;
        public string AvailableWeaponsText
        {
            get
            {
                if (!alreadySetAvailableWeaponsText)
                {
                    string str = string.Empty;
                    foreach (WeaponType availableWeapon in availableWeapons)
                    {
                        if (!string.IsNullOrEmpty(str))
                            str += "/";
                        str += availableWeapon.Title;
                    }
                    availableWeaponsText = str;
                    alreadySetAvailableWeaponsText = true;
                }
                return availableWeaponsText;
            }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddWeaponTypes(availableWeapons);
            GameInstance.AddPoolingObjects(skillCastEffects);
            GameInstance.AddPoolingObjects(damageHitEffects);
            GameInstance.AddPoolingObjects(GetBuff().effects);
            GameInstance.AddPoolingObjects(GetDebuff().effects);
        }

        public GameEffect[] GetSkillCastEffect()
        {
            return skillCastEffects;
        }

        public float GetCastDuration(short skillLevel)
        {
            return castDuration.GetAmount(skillLevel);
        }

        public GameEffect[] GetDamageHitEffects()
        {
            return damageHitEffects;
        }

        public int GetConsumeHp(short level)
        {
            return consumeHp.GetAmount(level);
        }

        public int GetConsumeMp(short level)
        {
            return consumeMp.GetAmount(level);
        }

        public int GetConsumeStamina(short level)
        {
            return consumeStamina.GetAmount(level);
        }

        public float GetCoolDownDuration(short level)
        {
            float duration = coolDownDuration.GetAmount(level);
            if (duration < 0f)
                duration = 0f;
            return duration;
        }

        public short GetRequireCharacterLevel(short level)
        {
            return requirement.characterLevel.GetAmount((short)(level + 1));
        }

        public bool IsAvailable(ICharacterData character)
        {
            short skillLevel;
            return character.GetCaches().Skills.TryGetValue(this, out skillLevel) && skillLevel > 0;
        }

        public abstract SkillType SkillType { get; }
        public abstract bool IsAttack();
        public abstract bool IsBuff();
        public abstract bool IsDebuff();
        public abstract float GetCastDistance(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand);
        public abstract float GetCastFov(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand);
        public abstract KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, short skillLevel, bool isLeftHand);
        public abstract Dictionary<DamageElement, float> GetAttackWeaponDamageInflictions(ICharacterData skillUser, short skillLevel);
        public abstract Dictionary<DamageElement, MinMaxFloat> GetAttackAdditionalDamageAmounts(ICharacterData skillUser, short skillLevel);
        public virtual bool RequiredTarget() { return false; }
        public virtual bool IsIncreaseAttackDamageAmountsWithBuffs(ICharacterData skillUser, short skillLevel) { return false; }
        public virtual bool HasCustomAimControls() { return false; }
        public virtual Vector3? UpdateAimControls(Vector2 aimAxes, params object[] data) { return null; }
        public virtual void FinishAimControls(bool isCancel) { }
        public virtual short GetUseAmmoAmount() { return 0; }
        public virtual Buff GetBuff() { return new Buff(); }
        public virtual Buff GetDebuff() { return new Buff(); }
        public virtual SkillSummon GetSummon() { return new SkillSummon(); }
        public virtual SkillMount GetMount() { return new SkillMount(); }
        public virtual ItemCraft GetItemCraft() { return new ItemCraft(); }

        public bool IsActive()
        {
            return SkillType == SkillType.Active;
        }

        public bool IsPassive()
        {
            return SkillType == SkillType.Passive;
        }

        public bool IsCraftItem()
        {
            return SkillType == SkillType.CraftItem;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetAttackDamages(ICharacterData skillUser, short skillLevel, bool isLeftHand)
        {
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = new Dictionary<DamageElement, MinMaxFloat>();

            if (!IsAttack())
                return damageAmounts;

            // Base attack damage amount will sum with other variables later
            damageAmounts = GameDataHelpers.CombineDamages(
                damageAmounts,
                GetBaseAttackDamageAmount(skillUser, skillLevel, isLeftHand));

            // Sum damage with weapon damage inflictions
            Dictionary<DamageElement, float> damageInflictions = GetAttackWeaponDamageInflictions(skillUser, skillLevel);
            if (damageInflictions != null && damageInflictions.Count > 0)
            {
                // Prepare weapon damage amount
                KeyValuePair<DamageElement, MinMaxFloat> weaponDamageAmount = skillUser.GetWeaponDamage(ref isLeftHand);
                foreach (DamageElement element in damageInflictions.Keys)
                {
                    if (element == null) continue;
                    damageAmounts = GameDataHelpers.CombineDamages(
                        damageAmounts,
                        new KeyValuePair<DamageElement, MinMaxFloat>(element, weaponDamageAmount.Value * damageInflictions[element]));
                }
            }

            // Sum damage with additional damage amounts
            damageAmounts = GameDataHelpers.CombineDamages(
                damageAmounts,
                GetAttackAdditionalDamageAmounts(skillUser, skillLevel));

            // Sum damage with buffs
            if (IsIncreaseAttackDamageAmountsWithBuffs(skillUser, skillLevel))
            {
                damageAmounts = GameDataHelpers.CombineDamages(
                    damageAmounts,
                    skillUser.GetCaches().IncreaseDamages);
            }

            return damageAmounts;
        }

        /// <summary>
        /// Apply skill
        /// </summary>
        /// <param name="skillUser"></param>
        /// <param name="skillLevel"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="weapon"></param>
        /// <param name="hitIndex"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="aimPosition"></param>
        /// <returns></returns>
        public abstract void ApplySkill(
            BaseCharacterEntity skillUser,
            short skillLevel,
            bool isLeftHand,
            CharacterItem weapon,
            int hitIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            Vector3 aimPosition);

        /// <summary>
        /// Return TRUE if this will override default attack function
        /// </summary>
        /// <param name="skillUser"></param>
        /// <param name="skillLevel"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="weapon"></param>
        /// <param name="hitIndex"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="aimPosition"></param>
        /// <returns></returns>
        public virtual bool OnAttack(
            BaseCharacterEntity skillUser,
            short skillLevel,
            bool isLeftHand,
            CharacterItem weapon,
            int hitIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            Vector3 aimPosition)
        {
            return false;
        }

        public virtual bool CanLevelUp(IPlayerCharacterData character, short level, out GameMessage.Type gameMessageType, bool checkSkillPoint = true)
        {
            gameMessageType = GameMessage.Type.None;
            if (character == null || !character.GetDatabase().CacheSkillLevels.ContainsKey(this))
                return false;

            // Check is it pass attribute requirement or not
            Dictionary<Attribute, float> attributeAmountsDict = character.GetAttributes(false, false, null);
            Dictionary<Attribute, float> requireAttributeAmounts = CacheRequireAttributeAmounts;
            foreach (KeyValuePair<Attribute, float> requireAttributeAmount in requireAttributeAmounts)
            {
                if (!attributeAmountsDict.ContainsKey(requireAttributeAmount.Key) ||
                    attributeAmountsDict[requireAttributeAmount.Key] < requireAttributeAmount.Value)
                {
                    gameMessageType = GameMessage.Type.NotEnoughAttributeAmounts;
                    return false;
                }
            }
            // Check is it pass skill level requirement or not
            Dictionary<BaseSkill, int> skillLevelsDict = new Dictionary<BaseSkill, int>();
            foreach (CharacterSkill learnedSkill in character.Skills)
            {
                if (learnedSkill.GetSkill() == null)
                    continue;
                skillLevelsDict[learnedSkill.GetSkill()] = learnedSkill.level;
            }
            foreach (BaseSkill requireSkill in CacheRequireSkillLevels.Keys)
            {
                if (!skillLevelsDict.ContainsKey(requireSkill) ||
                    skillLevelsDict[requireSkill] < CacheRequireSkillLevels[requireSkill])
                {
                    gameMessageType = GameMessage.Type.NotEnoughSkillLevels;
                    return false;
                }
            }

            if (character.Level < GetRequireCharacterLevel(level))
            {
                gameMessageType = GameMessage.Type.NotEnoughLevel;
                return false;
            }

            if (maxLevel > 0 && level >= maxLevel)
            {
                gameMessageType = GameMessage.Type.SkillReachedMaxLevel;
                return false;
            }

            if (checkSkillPoint && character.SkillPoint <= 0)
            {
                gameMessageType = GameMessage.Type.NotEnoughSkillPoint;
                return false;
            }

            return true;
        }

        public virtual bool CanUse(BaseCharacterEntity character, short level, bool isLeftHand, out GameMessage.Type gameMessageType, bool isItem = false)
        {
            gameMessageType = GameMessage.Type.None;
            if (character == null)
                return false;

            if (level <= 0)
            {
                gameMessageType = GameMessage.Type.SkillLevelIsZero;
                return false;
            }

            bool available = true;
            BasePlayerCharacterEntity playerCharacter = character as BasePlayerCharacterEntity;
            if (playerCharacter != null)
            {
                // Only player character will check is skill is learned
                if (!isItem && !IsAvailable(character))
                {
                    gameMessageType = GameMessage.Type.SkillIsNotLearned;
                    return false;
                }

                // Only player character will check for available weapons
                switch (SkillType)
                {
                    case SkillType.Active:
                        available = availableWeapons == null || availableWeapons.Length == 0;
                        if (!available)
                        {
                            IWeaponItem rightWeaponItem = character.EquipWeapons.GetRightHandWeaponItem();
                            IWeaponItem leftWeaponItem = character.EquipWeapons.GetLeftHandWeaponItem();
                            foreach (WeaponType availableWeapon in availableWeapons)
                            {
                                if (rightWeaponItem != null && rightWeaponItem.WeaponType == availableWeapon)
                                {
                                    available = true;
                                    break;
                                }
                                else if (leftWeaponItem != null && leftWeaponItem.WeaponType == availableWeapon)
                                {
                                    available = true;
                                    break;
                                }
                                else if (rightWeaponItem == null && leftWeaponItem == null && GameInstance.Singleton.DefaultWeaponItem.WeaponType == availableWeapon)
                                {
                                    available = true;
                                    break;
                                }
                            }
                        }
                        break;
                    case SkillType.CraftItem:
                        if (playerCharacter == null || !GetItemCraft().CanCraft(playerCharacter, out gameMessageType))
                            return false;
                        break;
                    default:
                        return false;
                }
            }

            if (!available)
            {
                gameMessageType = GameMessage.Type.CannotUseSkillByCurrentWeapon;
                return false;
            }

            if (character.CurrentHp < GetConsumeHp(level))
            {
                gameMessageType = GameMessage.Type.NotEnoughHp;
                return false;
            }

            if (character.CurrentMp < GetConsumeMp(level))
            {
                gameMessageType = GameMessage.Type.NotEnoughMp;
                return false;
            }

            if (character.CurrentStamina < GetConsumeStamina(level))
            {
                gameMessageType = GameMessage.Type.NotEnoughStamina;
                return false;
            }

            int skillUsageIndex = character.IndexOfSkillUsage(DataId, SkillUsageType.Skill);
            if (skillUsageIndex >= 0 && character.SkillUsages[skillUsageIndex].coolDownRemainsDuration > 0f)
            {
                gameMessageType = GameMessage.Type.SkillIsCoolingDown;
                return false;
            }

            if (RequiredTarget())
            {
                BaseCharacterEntity targetEntity;
                if (!character.TryGetTargetEntity(out targetEntity))
                {
                    gameMessageType = GameMessage.Type.NoSkillTarget;
                    return false;
                }
                else if (!character.IsGameEntityInDistance(targetEntity, GetCastDistance(character, level, isLeftHand)))
                {
                    gameMessageType = GameMessage.Type.CharacterIsTooFar;
                    return false;
                }
            }

            CharacterItem weapon = character.GetAvailableWeapon(ref isLeftHand);
            if (IsAttack() && GetUseAmmoAmount() > 0 && !character.ValidateAmmo(weapon, GetUseAmmoAmount()))
            {
                gameMessageType = GameMessage.Type.NoAmmo;
                return false;
            }

            return true;
        }

        public virtual Transform GetApplyTransform(BaseCharacterEntity skillUser, bool isLeftHand)
        {
            return skillUser.MovementTransform;
        }
    }
}
