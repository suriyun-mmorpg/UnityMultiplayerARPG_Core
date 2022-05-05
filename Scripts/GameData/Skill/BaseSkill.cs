using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract partial class BaseSkill : BaseGameData, ICustomAimController
    {
        [Category("Skill Settings")]
        [Range(1, 100)]
        public short maxLevel = 1;

        [Category(2, "Activation Settings")]
        [Range(0f, 1f)]
        [Tooltip("This is move speed rate while using this skill")]
        public float moveSpeedRateWhileUsingSkill = 0f;
        public IncrementalInt consumeHp;
        public IncrementalInt consumeMp;
        public IncrementalInt consumeStamina;
        public IncrementalFloat consumeHpRate;
        public IncrementalFloat consumeMpRate;
        public IncrementalFloat consumeStaminaRate;
        public IncrementalFloat coolDownDuration;

        [Category(2, "Skill Casting")]
        [Header("Casting Effects")]
        public GameEffect[] skillCastEffects;
        public IncrementalFloat castDuration;
        public bool canBeInterruptedWhileCasting;

        [Header("Casted Effects")]
        public GameEffect[] damageHitEffects;

        [Category(11, "Requirement")]
        [Header("Requirements to Levelup (Tools)")]
        public SkillRequirement requirement = new SkillRequirement();
#if UNITY_EDITOR
        [InspectorButton(nameof(MakeRequirementEachLevels))]
        public bool makeRequirementEachLevels;
#endif

        [Header("Requirements to Levelup (Data)")]
        public List<SkillRequirementEntry> requirementEachLevels = new List<SkillRequirementEntry>();

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

        [Header("Requirements to Use")]
        [Tooltip("If this list is empty it won't decrease items from inventory. It will decrease one kind of item in this list when using skill, not all items in this list")]
        public ItemAmount[] requireItems;
        [Tooltip("If `Require Ammo Type` is `Based On Weapon` it will decrease ammo based on ammo type which set to the weapon, amount to decrease ammo can be set to `Require Ammo Amount`. If weapon has no require ammo, it will not able to use skill. If `Require Ammo Type` is `Based On Skill`, it will decrease ammo based on `Require Ammos` setting")]
        public RequireAmmoType requireAmmoType;
        [FormerlySerializedAs("useAmmoAmount")]
        [Tooltip("It will be used while `Require Ammo Type` is `Based On Weapon` to decrease ammo")]
        public short requireAmmoAmount;
        [Tooltip("If this list is empty it won't decrease ammo items from inventory. It will decrease one kind of item in this list when using skill, not all items in this list")]
        public AmmoTypeAmount[] requireAmmos;

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
                    StringBuilder str = new StringBuilder();
                    foreach (WeaponType availableWeapon in CacheAvailableWeapons)
                    {
                        if (availableWeapon == null)
                            continue;
                        if (str.Length > 0)
                            str.Append('/');
                        str.Append(availableWeapon.Title);
                    }
                    availableWeaponsText = str.ToString();
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
                    StringBuilder str = new StringBuilder();
                    foreach (ArmorType requireArmor in availableArmors)
                    {
                        if (requireArmor == null)
                            continue;
                        if (str.Length > 0)
                            str.Append('/');
                        str.Append(requireArmor.Title);
                    }
                    availableArmorsText = str.ToString();
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
                    StringBuilder str = new StringBuilder();
                    foreach (VehicleType requireVehicle in availableVehicles)
                    {
                        if (requireVehicle == null)
                            continue;
                        if (str.Length > 0)
                            str.Append('/');
                        str.Append(requireVehicle.Title);
                    }
                    availableVehiclesText = str.ToString();
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
            GameInstance.AddPoolingObjects(Buff.effects);
            GameInstance.AddPoolingObjects(Debuff.effects);
            Buff.PrepareRelatesData();
            Debuff.PrepareRelatesData();
            if (requirementEachLevels.Count <= 0)
                MakeRequirementEachLevels();
        }

        public GameEffect[] SkillCastEffect
        {
            get { return skillCastEffects; }
        }

        public float GetCastDuration(short skillLevel)
        {
            return castDuration.GetAmount(skillLevel);
        }

        public GameEffect[] DamageHitEffects
        {
            get { return damageHitEffects; }
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

        public float GetConsumeHpRate(short level)
        {
            return consumeHpRate.GetAmount(level);
        }

        public float GetConsumeMpRate(short level)
        {
            return consumeMpRate.GetAmount(level);
        }

        public float GetConsumeStaminaRate(short level)
        {
            return consumeStaminaRate.GetAmount(level);
        }

        public int GetTotalConsumeHp(short level, ICharacterData character)
        {
            return (int)(character.GetCaches().MaxHp * GetConsumeHpRate(level)) + GetConsumeHp(level);
        }

        public int GetTotalConsumeMp(short level, ICharacterData character)
        {
            return (int)(character.GetCaches().MaxMp * GetConsumeMpRate(level)) + GetConsumeMp(level);
        }

        public int GetTotalConsumeStamina(short level, ICharacterData character)
        {
            return (int)(character.GetCaches().MaxStamina * GetConsumeStaminaRate(level)) + GetConsumeStamina(level);
        }

        public float GetCoolDownDuration(short level)
        {
            float duration = coolDownDuration.GetAmount(level);
            if (duration < 0f)
                duration = 0f;
            return duration;
        }

        public bool IsDisallowToLevelUp(short level)
        {
            if (level >= requirementEachLevels.Count)
                return requirementEachLevels[requirementEachLevels.Count - 1].disallow;
            return requirementEachLevels[level].disallow;
        }

        public short GetRequireCharacterLevel(short level)
        {
            if (level >= requirementEachLevels.Count)
                return requirementEachLevels[requirementEachLevels.Count - 1].characterLevel;
            return requirementEachLevels[level].characterLevel;
        }

        public float GetRequireCharacterSkillPoint(short level)
        {
            if (level >= requirementEachLevels.Count)
                return requirementEachLevels[requirementEachLevels.Count - 1].skillPoint;
            return requirementEachLevels[level].skillPoint;
        }

        public int GetRequireCharacterGold(short level)
        {
            if (level >= requirementEachLevels.Count)
                return requirementEachLevels[requirementEachLevels.Count - 1].gold;
            return requirementEachLevels[level].gold;
        }

        public Dictionary<Attribute, float> GetRequireAttributeAmounts(short level)
        {
            if (level >= requirementEachLevels.Count)
                return GameDataHelpers.CombineAttributes(requirementEachLevels[requirementEachLevels.Count - 1].attributeAmounts, null, 1f);
            return GameDataHelpers.CombineAttributes(requirementEachLevels[level].attributeAmounts, null, 1f);
        }

        public Dictionary<BaseSkill, short> GetRequireSkillLevels(short level)
        {
            if (level >= requirementEachLevels.Count)
                return GameDataHelpers.CombineSkills(requirementEachLevels[requirementEachLevels.Count - 1].skillLevels, null);
            return GameDataHelpers.CombineSkills(requirementEachLevels[level].skillLevels, null);
        }

        public Dictionary<Currency, int> GetRequireCurrencyAmounts(short level)
        {
            if (level >= requirementEachLevels.Count)
                return GameDataHelpers.CombineCurrencies(requirementEachLevels[requirementEachLevels.Count - 1].currencyAmounts, null);
            return GameDataHelpers.CombineCurrencies(requirementEachLevels[level].currencyAmounts, null);
        }

        public Dictionary<BaseItem, short> GetRequireItemAmounts(short level)
        {
            if (level >= requirementEachLevels.Count)
                return GameDataHelpers.CombineItems(requirementEachLevels[requirementEachLevels.Count - 1].itemAmounts, null);
            return GameDataHelpers.CombineItems(requirementEachLevels[level].itemAmounts, null);
        }

        public bool IsAvailable(ICharacterData character)
        {
            short skillLevel;
            return character.GetCaches().Skills.TryGetValue(this, out skillLevel) && skillLevel > 0;
        }

        public abstract SkillType SkillType { get; }
        public virtual bool IsAttack { get { return false; } }
        public virtual bool IsBuff { get { return false; } }
        public virtual bool IsDebuff { get { return false; } }
        public abstract float GetCastDistance(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand);
        public abstract float GetCastFov(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand);
        public abstract KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, short skillLevel, bool isLeftHand);
        public abstract Dictionary<DamageElement, float> GetAttackWeaponDamageInflictions(ICharacterData skillUser, short skillLevel);
        public abstract Dictionary<DamageElement, MinMaxFloat> GetAttackAdditionalDamageAmounts(ICharacterData skillUser, short skillLevel);
        public virtual bool RequiredTarget { get { return false; } }
        public virtual bool IsIncreaseAttackDamageAmountsWithBuffs(ICharacterData skillUser, short skillLevel) { return false; }
        public virtual HarvestType GetHarvestType() { return HarvestType.None; }
        public virtual IncrementalMinMaxFloat GetHarvestDamageAmount() { return new IncrementalMinMaxFloat(); }
        public virtual bool HasCustomAimControls() { return false; }
        public virtual AimPosition UpdateAimControls(Vector2 aimAxes, params object[] data) { return default; }
        public virtual void FinishAimControls(bool isCancel) { }
        public virtual bool IsChanneledAbility() { return false; }
        public virtual Buff Buff { get { return Buff.Empty; } }
        public virtual Buff Debuff { get { return Buff.Empty; } }
        public virtual SkillSummon Summon { get { return SkillSummon.Empty; } }
        public virtual SkillMount Mount { get { return SkillMount.Empty; } }
        public virtual ItemCraft ItemCraft { get { return ItemCraft.Empty; } }

        public bool IsActive
        {
            get { return SkillType == SkillType.Active; }
        }

        public bool IsPassive
        {
            get { return SkillType == SkillType.Passive; }
        }

        public bool IsCraftItem
        {
            get { return SkillType == SkillType.CraftItem; }
        }

        public Dictionary<DamageElement, MinMaxFloat> GetAttackDamages(ICharacterData skillUser, short skillLevel, bool isLeftHand)
        {
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = new Dictionary<DamageElement, MinMaxFloat>();

            if (!IsAttack)
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
                KeyValuePair<DamageElement, MinMaxFloat> weaponDamageAmount = skillUser.GetWeaponDamages(ref isLeftHand);
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
        /// <param name="targetObjectId"></param>
        /// <param name="aimPosition"></param>
        /// <param name="randomSeed"></param>
        /// <returns></returns>
        public void ApplySkill(
            BaseCharacterEntity skillUser,
            short skillLevel,
            bool isLeftHand,
            CharacterItem weapon,
            int hitIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            uint targetObjectId,
            AimPosition aimPosition,
            int randomSeed)
        {
            if (skillUser == null)
                return;
            // Decrease player character entity's items
            if (skillUser.IsServer && skillUser is BasePlayerCharacterEntity)
            {
                // Not enough items
                if (!DecreaseItems(skillUser))
                    return;
                // Increase damage with ammo damage
                Dictionary<DamageElement, MinMaxFloat> increaseDamages;
                if (!DecreaseAmmos(skillUser, isLeftHand, out increaseDamages))
                    return;
                if (increaseDamages != null && increaseDamages.Count > 0)
                    damageAmounts = GameDataHelpers.CombineDamages(damageAmounts, increaseDamages);
            }
            ApplySkillImplement(
                skillUser,
                skillLevel,
                isLeftHand,
                weapon,
                hitIndex,
                damageAmounts,
                targetObjectId,
                aimPosition,
                randomSeed);
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
        /// <param name="targetObjectId"></param>
        /// <param name="aimPosition"></param>
        /// <param name="randomSeed"></param>
        /// <returns></returns>
        protected abstract void ApplySkillImplement(
            BaseCharacterEntity skillUser,
            short skillLevel,
            bool isLeftHand,
            CharacterItem weapon,
            int hitIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            uint targetObjectId,
            AimPosition aimPosition,
            int randomSeed);

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
            AimPosition aimPosition)
        {
            return false;
        }

        public virtual bool CanLevelUp(IPlayerCharacterData character, short level, out UITextKeys gameMessage, bool checkSkillPoint = true, bool checkGold = true)
        {
            gameMessage = UITextKeys.NONE;
            if (character == null || !character.GetDatabase().CacheSkillLevels.ContainsKey(this))
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_CHARACTER_DATA;
                return false;
            }

            if (IsDisallowToLevelUp(level))
            {
                gameMessage = UITextKeys.UI_ERROR_DISALLOW_SKILL_LEVEL_UP;
                return false;
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

            if (checkSkillPoint && character.SkillPoint < GetRequireCharacterSkillPoint(level))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_SKILL_POINT;
                return false;
            }

            if (checkGold && character.Gold < GetRequireCharacterGold(level))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD;
                return false;
            }

            // Check is it pass skill level requirement or not
            Dictionary<BaseSkill, short> currentSkillLevels;
            if (!character.HasEnoughSkillLevels(GetRequireSkillLevels(level), false, out gameMessage, out currentSkillLevels))
                return false;

            // Check is it pass attribute requirement or not
            if (!character.HasEnoughAttributeAmounts(GetRequireAttributeAmounts(level), false, false, currentSkillLevels, out gameMessage, out _))
                return false;

            // Check is it pass currency requirement or not
            if (!character.HasEnoughCurrencyAmounts(GetRequireCurrencyAmounts(level), out gameMessage, out _))
                return false;

            // Check is it pass item requirement or not
            if (!character.HasEnoughNonEquipItemAmounts(GetRequireItemAmounts(level), out gameMessage, out _))
                return false;

            return true;
        }

        public virtual bool CanUse(BaseCharacterEntity character, short level, bool isLeftHand, uint targetObjectId, out UITextKeys gameMessage, bool isItem = false)
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
                        if (playerCharacter == null || !ItemCraft.CanCraft(playerCharacter, out gameMessage))
                            return false;
                        break;
                    default:
                        return false;
                }

                if (!HasEnoughItems(character, out _, out _))
                {
                    gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS;
                    return false;
                }

                if (!HasEnoughAmmos(character, isLeftHand, out _, out _))
                {
                    gameMessage = UITextKeys.UI_ERROR_NO_AMMO;
                    return false;
                }
            }

            if (character.CurrentHp < GetTotalConsumeHp(level, character))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_HP;
                return false;
            }

            if (character.CurrentMp < GetTotalConsumeMp(level, character))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_MP;
                return false;
            }

            if (character.CurrentStamina < GetTotalConsumeStamina(level, character))
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

            if (RequiredTarget)
            {
                BaseCharacterEntity targetEntity;
                if (!character.CurrentGameManager.TryGetEntityByObjectId(targetObjectId, out targetEntity))
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

            return true;
        }

        /// <summary>
        /// Find one which has enough amount from require items
        /// </summary>
        /// <param name="character"></param>
        /// <param name="itemDataId"></param>
        /// <returns></returns>
        protected bool HasEnoughItems(BaseCharacterEntity character, out int itemDataId, out short amount)
        {
            itemDataId = 0;
            amount = 0;
            if (requireItems == null || requireItems.Length == 0)
                return true;
            foreach (ItemAmount requireItem in requireItems)
            {
                if (character.CountNonEquipItems(requireItem.item.DataId) >= requireItem.amount)
                {
                    itemDataId = requireItem.item.DataId;
                    amount = requireItem.amount;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Find one which has enough amount from require ammos
        /// </summary>
        /// <param name="character"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="ammoType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        protected bool HasEnoughAmmos(BaseCharacterEntity character, bool isLeftHand, out AmmoType ammoType, out short amount)
        {
            ammoType = null;
            amount = 0;
            switch (requireAmmoType)
            {
                case RequireAmmoType.BasedOnWeapon:
                    return character.ValidateAmmo(character.GetAvailableWeapon(ref isLeftHand), requireAmmoAmount, false);
                case RequireAmmoType.BasedOnSkill:
                    if (requireAmmos == null || requireAmmos.Length == 0)
                        return true;
                    foreach (AmmoTypeAmount requireAmmo in requireAmmos)
                    {
                        if (character.CountAmmos(requireAmmo.ammoType) >= requireAmmo.amount)
                        {
                            ammoType = requireAmmo.ammoType;
                            amount = requireAmmo.amount;
                            return true;
                        }
                    }
                    return false;
            }
            return true;
        }

        protected bool DecreaseItems(BaseCharacterEntity character)
        {
            int itemDataId;
            short amount;
            if (HasEnoughItems(character, out itemDataId, out amount))
            {
                if (itemDataId == 0 || amount == 0)
                {
                    // No required items, don't decrease items
                    return true;
                }
                if (character.DecreaseItems(itemDataId, amount))
                {
                    character.FillEmptySlots();
                    return true;
                }
            }
            return false;
        }

        protected bool DecreaseAmmos(BaseCharacterEntity character, bool isLeftHand, out Dictionary<DamageElement, MinMaxFloat> increaseDamages)
        {
            increaseDamages = null;
            AmmoType ammoType;
            short amount;
            switch (requireAmmoType)
            {
                case RequireAmmoType.BasedOnWeapon:
                    return character.DecreaseAmmos(character.GetAvailableWeapon(ref isLeftHand), isLeftHand, requireAmmoAmount, out increaseDamages, false);
                case RequireAmmoType.BasedOnSkill:
                    if (HasEnoughAmmos(character, isLeftHand, out ammoType, out amount))
                    {
                        if (ammoType == null || amount == 0)
                        {
                            // No required ammos, don't decrease ammos
                            return true;
                        }
                        if (character.DecreaseAmmos(ammoType, amount, out increaseDamages))
                        {
                            character.FillEmptySlots();
                            return true;
                        }
                    }
                    return false;
            }
            return true;
        }

        public virtual Transform GetApplyTransform(BaseCharacterEntity skillUser, bool isLeftHand)
        {
            return skillUser.MovementTransform;
        }

        public void MakeRequirementEachLevels()
        {
            requirementEachLevels.Clear();
            for (short i = 1; i <= maxLevel; ++i)
            {
                requirementEachLevels.Add(new SkillRequirementEntry()
                {
                    disallow = requirement.cannotLevelUp,
                    characterLevel = requirement.characterLevel.GetAmount(i),
                    skillPoint = requirement.skillPoint.GetAmount(i),
                    gold = requirement.gold.GetAmount(i),
                    attributeAmounts = requirement.attributeAmounts,
                    skillLevels = requirement.skillLevels,
                    currencyAmounts = requirement.currencyAmounts,
                    itemAmounts = requirement.itemAmounts,
                });
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorUtility.SetDirty(this);
#endif
        }
    }
}
