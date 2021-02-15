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

        [Header("Required Equipments")]
        [Tooltip("If this is `TRUE`, character have to equip shield to use skill")]
        public bool requireShield;

        [Tooltip("Characters will be able to use skill if this list is empty or equipping one in this list")]
        public WeaponType[] availableWeapons;

        [Tooltip("Characters will be able to use skill if this list is empty or equipping one in this list")]
        public ArmorType[] availableArmors;

        [Header("Required Vehicles")]
        [Tooltip("Characters will be able to use skill if this list is empty or driving one in this list")]
        public VehicleType[] availableVehicles;

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
        private HashSet<WeaponType> cacheAvailableWeapons;
        public HashSet<WeaponType> CacheAvailableWeapons
        {
            get
            {
                if (cacheAvailableWeapons == null)
                {
                    cacheAvailableWeapons = new HashSet<WeaponType>();
                    if (availableWeapons == null || availableWeapons.Length == 0)
                        return cacheAvailableWeapons;
                    foreach (WeaponType availableWeapon in availableWeapons)
                    {
                        if (availableWeapon == null) continue;
                        cacheAvailableWeapons.Add(availableWeapon);
                    }
                }
                return cacheAvailableWeapons;
            }
        }

        [System.NonSerialized]
        private HashSet<ArmorType> cacheAvailableArmors;
        public HashSet<ArmorType> CacheAvailableArmors
        {
            get
            {
                if (cacheAvailableArmors == null)
                {
                    cacheAvailableArmors = new HashSet<ArmorType>();
                    if (availableArmors == null || availableArmors.Length == 0)
                        return cacheAvailableArmors;
                    foreach (ArmorType requireArmor in availableArmors)
                    {
                        if (requireArmor == null) continue;
                        cacheAvailableArmors.Add(requireArmor);
                    }
                }
                return cacheAvailableArmors;
            }
        }

        [System.NonSerialized]
        private HashSet<VehicleType> cacheAvailableVehicles;
        public HashSet<VehicleType> CacheAvailableVehicles
        {
            get
            {
                if (cacheAvailableVehicles == null)
                {
                    cacheAvailableVehicles = new HashSet<VehicleType>();
                    if (availableVehicles == null || availableVehicles.Length == 0)
                        return cacheAvailableVehicles;
                    foreach (VehicleType requireVehicle in availableVehicles)
                    {
                        if (requireVehicle == null) continue;
                        cacheAvailableVehicles.Add(requireVehicle);
                    }
                }
                return cacheAvailableVehicles;
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
                    foreach (WeaponType availableWeapon in CacheAvailableWeapons)
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

        [System.NonSerialized]
        private bool alreadySetAvailableArmorsText;
        [System.NonSerialized]
        private string availableArmorsText;
        public string AvailableArmorsText
        {
            get
            {
                if (!alreadySetAvailableArmorsText)
                {
                    string str = string.Empty;
                    foreach (ArmorType requireArmor in availableArmors)
                    {
                        if (!string.IsNullOrEmpty(str))
                            str += "/";
                        str += requireArmor.Title;
                    }
                    availableArmorsText = str;
                    alreadySetAvailableArmorsText = true;
                }
                return availableArmorsText;
            }
        }

        [System.NonSerialized]
        private bool alreadySetAvailableVehiclesText;
        [System.NonSerialized]
        private string availableVehiclesText;
        public string AvailableVehiclesText
        {
            get
            {
                if (!alreadySetAvailableVehiclesText)
                {
                    string str = string.Empty;
                    foreach (VehicleType requireVehicle in availableVehicles)
                    {
                        if (!string.IsNullOrEmpty(str))
                            str += "/";
                        str += requireVehicle.Title;
                    }
                    availableVehiclesText = str;
                    alreadySetAvailableVehiclesText = true;
                }
                return availableVehiclesText;
            }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
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

        public virtual bool CanLevelUp(IPlayerCharacterData character, short level, out UITextKeys gameMessage, bool checkSkillPoint = true)
        {
            gameMessage = UITextKeys.NONE;
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
                    gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ATTRIBUTE_AMOUNTS;
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
                    gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_SKILL_LEVELS;
                    return false;
                }
            }

            if (character.Level < GetRequireCharacterLevel(level))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_LEVEL;
                return false;
            }

            if (maxLevel > 0 && level >= maxLevel)
            {
                gameMessage = UITextKeys.UI_ERROR_SKILL_REACHED_MAX_LEVEL;
                return false;
            }

            if (checkSkillPoint && character.SkillPoint <= 0)
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_SKILL_POINT;
                return false;
            }

            return true;
        }

        public virtual bool CanUse(BaseCharacterEntity character, short level, bool isLeftHand, out UITextKeys gameMessage, bool isItem = false)
        {
            gameMessage = UITextKeys.NONE;
            if (character == null)
                return false;

            if (level <= 0)
            {
                gameMessage = UITextKeys.UI_ERROR_SKILL_LEVEL_IS_ZERO;
                return false;
            }

            BasePlayerCharacterEntity playerCharacter = character as BasePlayerCharacterEntity;
            if (playerCharacter != null)
            {
                // Only player character will check is skill is learned
                if (!isItem && !IsAvailable(character))
                {
                    gameMessage = UITextKeys.UI_ERROR_SKILL_IS_NOT_LEARNED;
                    return false;
                }

                // Only player character will check for available weapons
                switch (SkillType)
                {
                    case SkillType.Active:
                        if (requireShield)
                        {
                            IShieldItem leftShieldItem = character.EquipWeapons.GetLeftHandShieldItem();
                            if (leftShieldItem == null)
                            {
                                gameMessage = UITextKeys.UI_ERROR_CANNOT_USE_SKILL_WITHOUT_SHIELD;
                                return false;
                            }
                        }
                        if (CacheAvailableWeapons.Count > 0)
                        {
                            bool available = false;
                            IWeaponItem rightWeaponItem = character.EquipWeapons.GetRightHandWeaponItem();
                            IWeaponItem leftWeaponItem = character.EquipWeapons.GetLeftHandWeaponItem();
                            if (rightWeaponItem != null && CacheAvailableWeapons.Contains(rightWeaponItem.WeaponType))
                            {
                                available = true;
                            }
                            else if (leftWeaponItem != null && CacheAvailableWeapons.Contains(leftWeaponItem.WeaponType))
                            {
                                available = true;
                            }
                            else if (rightWeaponItem == null && leftWeaponItem == null && 
                                CacheAvailableWeapons.Contains(GameInstance.Singleton.DefaultWeaponItem.WeaponType))
                            {
                                available = true;
                            }
                            if (!available)
                            {
                                gameMessage = UITextKeys.UI_ERROR_CANNOT_USE_SKILL_BY_CURRENT_WEAPON;
                                return false;
                            }
                        }
                        if (CacheAvailableArmors.Count > 0)
                        {
                            bool available = false;
                            IArmorItem armorItem;
                            foreach (CharacterItem characterItem in character.EquipItems)
                            {
                                armorItem = characterItem.GetArmorItem();
                                if (armorItem != null && CacheAvailableArmors.Contains(armorItem.ArmorType))
                                {
                                    available = true;
                                    break;
                                }
                            }
                            if (!available)
                            {
                                gameMessage = UITextKeys.UI_ERROR_CANNOT_USE_SKILL_BY_CURRENT_ARMOR;
                                return false;
                            }
                        }
                        if (CacheAvailableVehicles.Count > 0)
                        {
                            if (character.PassengingVehicleType == null ||
                                !character.PassengingVehicleEntity.IsDriver(character.PassengingVehicle.seatIndex) ||
                                !CacheAvailableVehicles.Contains(character.PassengingVehicleType))
                            {
                                gameMessage = UITextKeys.UI_ERROR_CANNOT_USE_SKILL_BY_CURRENT_VEHICLE;
                                return false;
                            }
                        }
                        break;
                    case SkillType.CraftItem:
                        if (playerCharacter == null || !GetItemCraft().CanCraft(playerCharacter, out gameMessage))
                            return false;
                        break;
                    default:
                        return false;
                }
            }

            if (character.CurrentHp < GetConsumeHp(level))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_HP;
                return false;
            }

            if (character.CurrentMp < GetConsumeMp(level))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_MP;
                return false;
            }

            if (character.CurrentStamina < GetConsumeStamina(level))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_STAMINA;
                return false;
            }

            int skillUsageIndex = character.IndexOfSkillUsage(DataId, SkillUsageType.Skill);
            if (skillUsageIndex >= 0 && character.SkillUsages[skillUsageIndex].coolDownRemainsDuration > 0f)
            {
                gameMessage = UITextKeys.UI_ERROR_SKILL_IS_COOLING_DOWN;
                return false;
            }

            if (RequiredTarget())
            {
                BaseCharacterEntity targetEntity;
                if (!character.TryGetTargetEntity(out targetEntity))
                {
                    gameMessage = UITextKeys.UI_ERROR_NO_SKILL_TARGET;
                    return false;
                }
                else if (!character.IsGameEntityInDistance(targetEntity, GetCastDistance(character, level, isLeftHand)))
                {
                    gameMessage = UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR;
                    return false;
                }
            }

            CharacterItem weapon = character.GetAvailableWeapon(ref isLeftHand);
            if (IsAttack() && GetUseAmmoAmount() > 0 && !character.ValidateAmmo(weapon, GetUseAmmoAmount()))
            {
                gameMessage = UITextKeys.UI_ERROR_NO_AMMO;
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
