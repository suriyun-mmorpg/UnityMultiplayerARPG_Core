using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.LEGACY_ITEM_FILE, menuName = GameDataMenuConsts.LEGACY_ITEM_MENU, order = GameDataMenuConsts.LEGACY_ITEM_ORDER)]
    public partial class Item : BaseItem,
        IAmmoItem, IArmorItem, IShieldItem, IWeaponItem,
        IPotionItem, IBuildingItem, IPetItem, IMountItem, ISkillItem,
        ISocketEnhancerItem
    {
        public enum LegacyItemType : byte
        {
            Junk,
            Armor,
            Weapon,
            Shield,
            Potion,
            Ammo,
            Building,
            Pet,
            SocketEnhancer,
            Mount,
            AttributeIncrease,
            AttributeReset,
            Skill,
            SkillLearn,
            SkillReset,
        }

        [Category("Item Settings")]
        public LegacyItemType itemType;
        public float useItemCooldown;
        public string autoUseKey;

        [Category("In-Scene Objects/Appearance")]
        public EquipmentModel[] equipmentModels;
        [Tooltip("This will be available with `Weapon` item, set it in case that it will be equipped at left hand")]
        [FormerlySerializedAs("subEquipmentModels")]
        public EquipmentModel[] offHandEquipmentModels;
        public EquipmentModel[] sheathModels;
        public EquipmentModel[] offHandSheathModels;

        [Category(2, "Equipment Settings")]
        [Header("Generic Equipment Settings")]
        public EquipmentRequirement requirement;
        public EquipmentSet equipmentSet;
        [Tooltip("Equipment durability, If this set to 0 it will not broken")]
        [Range(0f, 1000f)]
        public float maxDurability;
        [Tooltip("If this is TRUE, your equipment will be destroyed when durability = 0")]
        public bool destroyIfBroken;
        [Range(0, 6)]
        public byte maxSocket;

        [Header("Armor/Shield Settings")]
        public ArmorType armorType;
        public ArmorIncremental armorAmount;

        [Header("Weapon Settings")]
        public WeaponType weaponType;
        public DamageIncremental damageAmount;
        public IncrementalMinMaxFloat harvestDamageAmount;
        [Range(0f, 1f)]
        [Tooltip("This is move speed rate while reloading this weapon")]
        public float moveSpeedRateWhileReloading = 1f;
        [Range(0f, 1f)]
        [Tooltip("This is move speed rate while charging this weapon")]
        public float moveSpeedRateWhileCharging = 1f;
        [Range(0f, 1f)]
        [Tooltip("This is move speed rate while attacking with this weapon")]
        public float moveSpeedRateWhileAttacking = 0f;
        public MovementRestriction movementRestrictionWhileReloading = default;
        public MovementRestriction movementRestrictionWhileCharging = default;
        public MovementRestriction movementRestrictionWhileAttacking = default;
        public ActionRestriction attackRestriction = default;
        public ActionRestriction reloadRestriction = default;
        [Tooltip("For macine gun may set this to 30 as magazine capacity, if this is 0 it will not need to have ammo loaded to shoot but still need ammo in inventory")]
        public int ammoCapacity;
        public BaseWeaponAbility weaponAbility;
        public CrosshairSetting crosshairSetting = new CrosshairSetting()
        {
            expandPerFrameWhileMoving = 3f,
            expandPerFrameWhileAttacking = 5f,
            shrinkPerFrame = 8f,
            minSpread = 10f,
            maxSpread = 50f
        };

        [HideInInspector]
        public AudioClip launchClip;
        public AudioClipWithVolumeSettings[] launchClipSettings = new AudioClipWithVolumeSettings[0];

        [HideInInspector]
        public AudioClip reloadClip;
        public AudioClipWithVolumeSettings[] reloadClipSettings = new AudioClipWithVolumeSettings[0];

        [HideInInspector]
        public AudioClip reloadedClip;
        public AudioClipWithVolumeSettings[] reloadedClipSettings = new AudioClipWithVolumeSettings[0];

        [HideInInspector]
        public AudioClip emptyClip;
        public AudioClipWithVolumeSettings[] emptyClipSettings = new AudioClipWithVolumeSettings[0];

        [Header("Fire Configs")]
        public FireType fireType;
        public Vector2 fireStagger;
        public byte fireSpread;
        public float chargeDuration;
        public bool destroyImmediatelyAfterFired;

        [Category(3, "Buff/Bonus Settings")]
        public Buff buff;
        public CharacterStatsIncremental increaseStats;
        public CharacterStatsIncremental increaseStatsRate;
        [ArrayElementTitle("attribute")]
        public AttributeIncremental[] increaseAttributes;
        [ArrayElementTitle("attribute")]
        public AttributeIncremental[] increaseAttributesRate;
        [ArrayElementTitle("damageElement")]
        public ResistanceIncremental[] increaseResistances;
        [ArrayElementTitle("damageElement")]
        public ArmorIncremental[] increaseArmors;
        [ArrayElementTitle("damageElement")]
        public DamageIncremental[] increaseDamages;
        [ArrayElementTitle("skill")]
        [HideInInspector]
        public SkillLevel[] increaseSkillLevels;
        public SkillIncremental[] increaseSkills;

        [Category(2, "Ammo Settings")]
        public AmmoType ammoType;

        [Category(2, "Building Settings")]
        public BuildingEntity buildingEntity;

        [Category(2, "Pet Settings")]
        public BaseMonsterCharacterEntity petEntity;

        [Category(3, "Mount Settings")]
        public VehicleEntity mountEntity;

        [Category("Buff/Bonus Settings")]
        // For socket enhancer items
        public EquipmentBonus socketEnhanceEffect;

        // For attribute increase/reset items
        public AttributeAmount attributeAmount;

        // For skill learn/reset items
        public SkillLevel skillLevel;

        [ArrayElementTitle("statusEffect")]
        public StatusEffectApplying[] selfStatusEffectsWhenAttacking;
        [ArrayElementTitle("statusEffect")]
        public StatusEffectApplying[] enemyStatusEffectsWhenAttacking;
        [ArrayElementTitle("statusEffect")]
        public StatusEffectApplying[] selfStatusEffectsWhenAttacked;
        [ArrayElementTitle("statusEffect")]
        public StatusEffectApplying[] enemyStatusEffectsWhenAttacked;

        // For equipment items
        public ItemRandomBonus randomBonus;

        public override ItemType ItemType
        {
            get
            {
                switch (itemType)
                {
                    case LegacyItemType.Junk:
                        return ItemType.Junk;
                    case LegacyItemType.Armor:
                        return ItemType.Armor;
                    case LegacyItemType.Weapon:
                        return ItemType.Weapon;
                    case LegacyItemType.Shield:
                        return ItemType.Shield;
                    case LegacyItemType.Potion:
                    case LegacyItemType.AttributeIncrease:
                    case LegacyItemType.AttributeReset:
                    case LegacyItemType.SkillLearn:
                    case LegacyItemType.SkillReset:
                        return ItemType.Potion;
                    case LegacyItemType.Ammo:
                        return ItemType.Ammo;
                    case LegacyItemType.Building:
                        return ItemType.Building;
                    case LegacyItemType.Pet:
                        return ItemType.Pet;
                    case LegacyItemType.SocketEnhancer:
                        return ItemType.SocketEnhancer;
                    case LegacyItemType.Mount:
                        return ItemType.Mount;
                    case LegacyItemType.Skill:
                        return ItemType.Skill;
                    default:
                        return ItemType.Junk;
                }
            }
        }

        public override string TypeTitle
        {
            get
            {
                switch (itemType)
                {
                    case LegacyItemType.Junk:
                        return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_JUNK.ToString());
                    case LegacyItemType.Armor:
                        return ArmorType.Title;
                    case LegacyItemType.Weapon:
                        return WeaponType.Title;
                    case LegacyItemType.Shield:
                        return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_SHIELD.ToString());
                    case LegacyItemType.Potion:
                    case LegacyItemType.AttributeIncrease:
                    case LegacyItemType.AttributeReset:
                    case LegacyItemType.SkillLearn:
                    case LegacyItemType.SkillReset:
                        return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_POTION.ToString());
                    case LegacyItemType.Ammo:
                        return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_AMMO.ToString());
                    case LegacyItemType.Building:
                        return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_BUILDING.ToString());
                    case LegacyItemType.Pet:
                        return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_PET.ToString());
                    case LegacyItemType.SocketEnhancer:
                        return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_SOCKET_ENHANCER.ToString());
                    case LegacyItemType.Mount:
                        return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_MOUNT.ToString());
                    case LegacyItemType.Skill:
                        return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_SKILL.ToString());
                    default:
                        return LanguageManager.GetUnknowTitle();
                }
            }
        }

        public float UseItemCooldown
        {
            get { return useItemCooldown; }
        }

        public string AutoUseKey
        {
            get { return autoUseKey; }
        }

        #region Implement IAmmoItem
        public AmmoType AmmoType
        {
            get { return ammoType; }
        }
        #endregion

        #region Implement IEquipmentItem
        public EquipmentRequirement Requirement
        {
            get { return requirement; }
        }

        [System.NonSerialized]
        private Dictionary<Attribute, float> cacheRequireAttributeAmounts;
        public Dictionary<Attribute, float> RequireAttributeAmounts
        {
            get
            {
                if (cacheRequireAttributeAmounts == null)
                    cacheRequireAttributeAmounts = GameDataHelpers.CombineAttributes(requirement.attributeAmounts, new Dictionary<Attribute, float>(), 1f);
                return cacheRequireAttributeAmounts;
            }
        }

        public EquipmentSet EquipmentSet
        {
            get { return equipmentSet; }
        }

        public float MaxDurability
        {
            get { return maxDurability; }
        }

        public bool DestroyIfBroken
        {
            get { return destroyIfBroken; }
        }

        public byte MaxSocket
        {
            get { return maxSocket; }
        }

        public EquipmentModel[] EquipmentModels
        {
            get { return equipmentModels; }
        }

        public CharacterStatsIncremental IncreaseStats
        {
            get { return increaseStats; }
        }

        public CharacterStatsIncremental IncreaseStatsRate
        {
            get { return increaseStatsRate; }
        }

        public AttributeIncremental[] IncreaseAttributes
        {
            get { return increaseAttributes; }
        }

        public AttributeIncremental[] IncreaseAttributesRate
        {
            get { return increaseAttributesRate; }
        }

        public ResistanceIncremental[] IncreaseResistances
        {
            get { return increaseResistances; }
        }

        public ArmorIncremental[] IncreaseArmors
        {
            get { return increaseArmors; }
        }

        public DamageIncremental[] IncreaseDamages
        {
            get { return increaseDamages; }
        }

        public SkillIncremental[] IncreaseSkills
        {
            get { return increaseSkills; }
        }

        public StatusEffectApplying[] SelfStatusEffectsWhenAttacking
        {
            get { return selfStatusEffectsWhenAttacking; }
        }

        public StatusEffectApplying[] EnemyStatusEffectsWhenAttacking
        {
            get { return enemyStatusEffectsWhenAttacking; }
        }

        public StatusEffectApplying[] SelfStatusEffectsWhenAttacked
        {
            get { return selfStatusEffectsWhenAttacked; }
        }

        public StatusEffectApplying[] EnemyStatusEffectsWhenAttacked
        {
            get { return enemyStatusEffectsWhenAttacked; }
        }

        public ItemRandomBonus RandomBonus
        {
            get { return randomBonus; }
        }
        #endregion

        #region Implement IDefendEquipmentItem
        public ArmorIncremental ArmorAmount
        {
            get { return armorAmount; }
        }
        #endregion

        #region Implement IArmorItem
        public ArmorType ArmorType
        {
            get
            {
                if (armorType == null && GameInstance.Singleton != null)
                    armorType = GameInstance.Singleton.DefaultArmorType;
                return armorType;
            }
        }
        #endregion

        #region Implement IWeaponItem
        public WeaponType WeaponType
        {
            get
            {
                if (weaponType == null && GameInstance.Singleton != null)
                    weaponType = GameInstance.Singleton.DefaultWeaponType;
                return weaponType;
            }
        }

        public EquipmentModel[] OffHandEquipmentModels
        {
            get { return offHandEquipmentModels; }
        }

        public EquipmentModel[] SheathModels
        {
            get { return sheathModels; }
            set { sheathModels = value; }
        }

        public EquipmentModel[] OffHandSheathModels
        {
            get { return offHandSheathModels; }
            set { offHandSheathModels = value; }
        }

        public DamageIncremental DamageAmount
        {
            get { return damageAmount; }
        }

        public IncrementalMinMaxFloat HarvestDamageAmount
        {
            get { return harvestDamageAmount; }
        }

        public float MoveSpeedRateWhileReloading
        {
            get { return moveSpeedRateWhileReloading; }
        }

        public float MoveSpeedRateWhileCharging
        {
            get { return moveSpeedRateWhileCharging; }
        }

        public float MoveSpeedRateWhileAttacking
        {
            get { return moveSpeedRateWhileAttacking; }
        }

        public MovementRestriction MovementRestrictionWhileReloading
        {
            get { return movementRestrictionWhileReloading; }
        }

        public MovementRestriction MovementRestrictionWhileCharging
        {
            get { return movementRestrictionWhileCharging; }
        }

        public MovementRestriction MovementRestrictionWhileAttacking
        {
            get { return movementRestrictionWhileAttacking; }
        }

        public ActionRestriction AttackRestriction
        {
            get { return attackRestriction; }
        }

        public ActionRestriction ReloadRestriction
        {
            get { return reloadRestriction; }
        }

        public int AmmoCapacity
        {
            get { return ammoCapacity; }
        }

        public BaseWeaponAbility WeaponAbility
        {
            get { return weaponAbility; }
        }

        public CrosshairSetting CrosshairSetting
        {
            get { return crosshairSetting; }
        }

        public AudioClipWithVolumeSettings LaunchClip
        {
            get
            {
                if (launchClipSettings != null && launchClipSettings.Length > 0)
                    return launchClipSettings[Random.Range(0, launchClipSettings.Length - 1)];
                return null;
            }
        }

        public AudioClipWithVolumeSettings ReloadClip
        {
            get
            {
                if (reloadClipSettings != null && reloadClipSettings.Length > 0)
                    return reloadClipSettings[Random.Range(0, reloadClipSettings.Length - 1)];
                return null;
            }
        }

        public AudioClipWithVolumeSettings ReloadedClip
        {
            get
            {
                if (reloadedClipSettings != null && reloadedClipSettings.Length > 0)
                    return reloadedClipSettings[Random.Range(0, reloadedClipSettings.Length - 1)];
                return null;
            }
        }

        public AudioClipWithVolumeSettings EmptyClip
        {
            get
            {
                if (emptyClipSettings != null && emptyClipSettings.Length > 0)
                    return emptyClipSettings[Random.Range(0, emptyClipSettings.Length - 1)];
                return null;
            }
        }

        public FireType FireType
        {
            get { return fireType; }
        }

        public Vector2 FireStagger
        {
            get { return fireStagger; }
        }

        public byte FireSpread
        {
            get { return fireSpread; }
        }

        public float ChargeDuration
        {
            get { return chargeDuration; }
        }

        public bool DestroyImmediatelyAfterFired
        {
            get { return destroyImmediatelyAfterFired; }
        }
        #endregion

        #region Implement IPotionItem, IBuildingItem, IPetItem, IMountItem, ISkillItem
        public Buff Buff
        {
            get { return buff; }
        }

        public BuildingEntity BuildingEntity
        {
            get { return buildingEntity; }
        }

        public BaseMonsterCharacterEntity PetEntity
        {
            get { return petEntity; }
        }

        public VehicleEntity MountEntity
        {
            get { return mountEntity; }
        }

        public BaseSkill UsingSkill
        {
            get { return skillLevel.skill; }
        }

        public int UsingSkillLevel
        {
            get { return skillLevel.level; }
        }
        #endregion

        #region Implement ISocketEnhancerItem
        public EquipmentBonus SocketEnhanceEffect
        {
            get { return socketEnhanceEffect; }
        }
        #endregion

        public override bool Validate()
        {
            bool hasChanges = false;
            if (MigrateAudioClips(ref launchClip, ref launchClipSettings))
                hasChanges = true;
            if (MigrateAudioClips(ref reloadClip, ref reloadClipSettings))
                hasChanges = true;
            if (MigrateAudioClips(ref reloadedClip, ref reloadedClipSettings))
                hasChanges = true;
            if (MigrateAudioClips(ref emptyClip, ref emptyClipSettings))
                hasChanges = true;
            if (increaseSkillLevels != null && increaseSkillLevels.Length > 0)
            {
                List<SkillIncremental> skills = new List<SkillIncremental>();
                foreach (SkillLevel increaseSkillLevel in increaseSkillLevels)
                {
                    if (increaseSkillLevel.skill == null)
                        continue;
                    skills.Add(new SkillIncremental()
                    {
                        skill = increaseSkillLevel.skill,
                        level = new IncrementalInt()
                        {
                            baseAmount = increaseSkillLevel.level
                        },
                    });
                }
                increaseSkills = skills.ToArray();
                increaseSkillLevels = null;
                hasChanges = true;
            }
            return hasChanges || base.Validate();
        }

        private bool MigrateAudioClips(ref AudioClip singleClip, ref AudioClipWithVolumeSettings[] destinationSettings)
        {
            if (singleClip == null)
                return false;

            bool hasChanges = false;

            List<AudioClip> clips = new List<AudioClip>();
            if (singleClip != null && !clips.Contains(singleClip))
            {
                clips.Add(singleClip);
                singleClip = null;
                hasChanges = true;
            }
            if (!hasChanges)
                return false;

            List<AudioClipWithVolumeSettings> clipSettings = new List<AudioClipWithVolumeSettings>();
            if (destinationSettings != null && destinationSettings.Length > 0)
                clipSettings.AddRange(destinationSettings);
            for (int i = 0; i < clips.Count; ++i)
            {
                clipSettings.Add(new AudioClipWithVolumeSettings()
                {
                    audioClip = clips[i],
                    minRandomVolume = 1f,
                    maxRandomVolume = 1f,
                });
            }
            destinationSettings = clipSettings.ToArray();
            return true;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddBuildingEntities(buildingEntity);
            GameInstance.AddCharacterEntities(petEntity);
            GameInstance.AddVehicleEntities(mountEntity);
            GameInstance.AddAttributes(increaseAttributes);
            GameInstance.AddAttributes(increaseAttributesRate);
            GameInstance.AddDamageElements(increaseResistances);
            GameInstance.AddDamageElements(increaseArmors);
            GameInstance.AddDamageElements(increaseDamages);
            GameInstance.AddDamageElements(damageAmount);
            GameInstance.AddSkills(increaseSkills);
            GameInstance.AddSkills(skillLevel);
            GameInstance.AddStatusEffects(selfStatusEffectsWhenAttacking);
            GameInstance.AddStatusEffects(enemyStatusEffectsWhenAttacking);
            GameInstance.AddStatusEffects(selfStatusEffectsWhenAttacked);
            GameInstance.AddStatusEffects(enemyStatusEffectsWhenAttacked);
            GameInstance.AddEquipmentSets(equipmentSet);
            GameInstance.AddPoolingWeaponLaunchEffects(equipmentModels);
            GameInstance.AddPoolingWeaponLaunchEffects(offHandEquipmentModels);
            GameInstance.AddArmorTypes(armorType);
            GameInstance.AddWeaponTypes(weaponType);
            GameInstance.AddAmmoTypes(ammoType);
            randomBonus.PrepareRelatesData();
            buff.PrepareRelatesData();
            // Data migration
            GameInstance.MigrateEquipmentEntities(equipmentModels);
            GameInstance.MigrateEquipmentEntities(offHandEquipmentModels);
        }

        public Item GenerateDefaultItem(WeaponType type)
        {
            name = GameDataConst.DEFAULT_WEAPON_ID;
            defaultTitle = GameDataConst.DEFAULT_WEAPON_TITLE;
            itemType = LegacyItemType.Weapon;
            weaponType = type;
            // Default damage amount
            IncrementalMinMaxFloat damageAmountMinMax = new IncrementalMinMaxFloat();
            damageAmountMinMax.baseAmount = new MinMaxFloat() { min = 1, max = 1 };
            damageAmountMinMax.amountIncreaseEachLevel = new MinMaxFloat() { min = 0, max = 0 };
            damageAmount = new DamageIncremental()
            {
                amount = damageAmountMinMax,
            };
            // Default harvest damage amount
            harvestDamageAmount = new IncrementalMinMaxFloat()
            {
                baseAmount = new MinMaxFloat() { min = 1, max = 1 },
                amountIncreaseEachLevel = new MinMaxFloat() { min = 0, max = 0 }
            };
            return this;
        }

        public void UseItem(BaseCharacterEntity character, int itemIndex, CharacterItem characterItem)
        {
            switch (itemType)
            {
                case LegacyItemType.Potion:
                    UseItemPotion(character, itemIndex, characterItem.level);
                    break;
                case LegacyItemType.Pet:
                    UseItemPet(character, itemIndex, characterItem.level, characterItem.exp);
                    break;
                case LegacyItemType.Mount:
                    UseItemMount(character, itemIndex, characterItem.level);
                    break;
                case LegacyItemType.AttributeIncrease:
                    UseItemAttributeIncrease(character as BasePlayerCharacterEntity, itemIndex);
                    break;
                case LegacyItemType.AttributeReset:
                    UseItemAttributeReset(character as BasePlayerCharacterEntity, itemIndex);
                    break;
                case LegacyItemType.SkillLearn:
                    UseItemSkillLearn(character as BasePlayerCharacterEntity, itemIndex);
                    break;
                case LegacyItemType.SkillReset:
                    UseItemSkillReset(character as BasePlayerCharacterEntity, itemIndex);
                    break;
            }
        }

        protected void UseItemPotion(BaseCharacterEntity character, int itemIndex, int level)
        {
            if (!character.CanUseItem() || level <= 0 || !character.DecreaseItemsByIndex(itemIndex, 1, false))
                return;
            character.FillEmptySlots();
            character.ApplyBuff(DataId, BuffType.PotionBuff, level, character.GetInfo(), null);
        }

        protected void UseItemPet(BaseCharacterEntity character, int itemIndex, int level, int exp)
        {
            if (!character.CanUseItem() || level <= 0 || !character.DecreaseItemsByIndex(itemIndex, 1, false))
                return;
            character.FillEmptySlots();
            // Clear all summoned pets
            CharacterSummon tempSummon;
            for (int i = character.Summons.Count - 1; i >= 0; --i)
            {
                tempSummon = character.Summons[i];
                if (tempSummon.type != SummonType.PetItem)
                    continue;
                character.Summons.RemoveAt(i);
                tempSummon.UnSummon(character);
            }
            // Summon new pet
            CharacterSummon newSummon = CharacterSummon.Create(SummonType.PetItem, DataId);
            newSummon.Summon(character, level, 0f, exp);
            character.Summons.Add(newSummon);
        }

        protected void UseItemMount(BaseCharacterEntity character, int itemIndex, int level)
        {
            if (!character.CanUseItem() || level <= 0)
                return;

            character.Mount(MountEntity);
        }

        protected void UseItemAttributeIncrease(BasePlayerCharacterEntity character, int itemIndex)
        {
            if (!character.CanUseItem() || attributeAmount.attribute == null)
                return;

            UITextKeys gameMessage;
            if (!character.AddAttribute(out gameMessage, attributeAmount.attribute.DataId, (int)attributeAmount.amount, itemIndex))
                GameInstance.ServerGameMessageHandlers.SendGameMessage(character.ConnectionId, gameMessage);
        }

        protected void UseItemAttributeReset(BasePlayerCharacterEntity character, int itemIndex)
        {
            if (!character.CanUseItem())
                return;

            character.ResetAttributes(itemIndex);
        }

        protected void UseItemSkillLearn(BasePlayerCharacterEntity character, int itemIndex)
        {
            if (!character.CanUseItem() || UsingSkill == null)
                return;

            UITextKeys gameMessage;
            if (!character.AddSkill(out gameMessage, UsingSkill.DataId, UsingSkillLevel, itemIndex))
                GameInstance.ServerGameMessageHandlers.SendGameMessage(character.ConnectionId, gameMessage);
        }

        protected void UseItemSkillReset(BasePlayerCharacterEntity character, int itemIndex)
        {
            if (!character.CanUseItem())
                return;

            character.ResetSkills(itemIndex);
        }

        public bool HasCustomAimControls()
        {
            switch (itemType)
            {
                case LegacyItemType.Potion:
                    return false;
                case LegacyItemType.Building:
                    return true;
                case LegacyItemType.Pet:
                    return false;
                case LegacyItemType.Mount:
                    return false;
                case LegacyItemType.Skill:
                    return skillLevel.skill.HasCustomAimControls();
            }
            return false;
        }

        public AimPosition UpdateAimControls(Vector2 aimAxes, params object[] data)
        {
            switch (itemType)
            {
                case LegacyItemType.Potion:
                    return default;
                case LegacyItemType.Building:
                    return BasePlayerCharacterController.Singleton.BuildAimController.UpdateAimControls(aimAxes, BuildingEntity);
                case LegacyItemType.Pet:
                    return default;
                case LegacyItemType.Mount:
                    return default;
                case LegacyItemType.Skill:
                    return UsingSkill.UpdateAimControls(aimAxes, UsingSkillLevel);
            }
            return default;
        }

        public void FinishAimControls(bool isCancel)
        {
            switch (itemType)
            {
                case LegacyItemType.Potion:
                    break;
                case LegacyItemType.Building:
                    BasePlayerCharacterController.Singleton.BuildAimController.FinishAimControls(isCancel);
                    break;
                case LegacyItemType.Pet:
                    break;
                case LegacyItemType.Mount:
                    break;
                case LegacyItemType.Skill:
                    skillLevel.skill.FinishAimControls(isCancel);
                    break;
            }
        }

        public bool IsChanneledAbility()
        {
            switch (itemType)
            {
                case LegacyItemType.Potion:
                    return false;
                case LegacyItemType.Building:
                    return false;
                case LegacyItemType.Pet:
                    return false;
                case LegacyItemType.Mount:
                    return false;
                case LegacyItemType.Skill:
                    return skillLevel.skill.IsChanneledAbility();
            }
            return false;
        }
    }
}
