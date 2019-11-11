using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.UI;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class UICharacterItem : UIDataForCharacter<UICharacterItemData>
    {
        public CharacterItem CharacterItem { get { return Data.characterItem; } }
        public short Level { get { return Data.targetLevel; } }
        public InventoryType InventoryType { get { return Data.inventoryType; } }
        public Item Item { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetItem() : null; } }
        public Item EquipmentItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetEquipmentItem() : null; } }
        public Item ArmorItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetArmorItem() : null; } }
        public Item ShieldItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetShieldItem() : null; } }
        public Item DefendItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetDefendItem() : null; } }
        public Item WeaponItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetWeaponItem() : null; } }
        public Item PotionItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetPotionItem() : null; } }
        public Item AmmoItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetAmmoItem() : null; } }
        public Item BuildingItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetBuildingItem() : null; } }
        public Item PetItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetPetItem() : null; } }
        public Item SocketEnhancerItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetSocketEnhancerItem() : null; } }
        public Item MountItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetMountItem() : null; } }
        public Item AttributeIncreaseItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetAttributeIncreaseItem() : null; } }
        public Item AttributeResetItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetAttributeResetItem() : null; } }
        public Item SkillItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetSkillItem() : null; } }
        public Item SkillLearnItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetSkillLearnItem() : null; } }
        public Item SkillResetItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetSkillResetItem() : null; } }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Description}")]
        public UILocaleKeySetting formatKeyDescription = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Rarity Title}")]
        public UILocaleKeySetting formatKeyRarityTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_RARITY);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);
        [Tooltip("Format => {0} = {Refine Level}")]
        public UILocaleKeySetting formatKeyRefineLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_REFINE_LEVEL);
        [Tooltip("Format => {0} = {Refine Level}")]
        public UILocaleKeySetting formatKeyTitleWithRefineLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_TITLE_WITH_REFINE_LEVEL);
        [Tooltip("Format => {0} = {Sell Price}")]
        public UILocaleKeySetting formatKeySellPrice = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SELL_PRICE);
        [Tooltip("Format => {0} = {Amount}, {1} = {Max Stack}")]
        public UILocaleKeySetting formatKeyStack = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_STACK);
        [Tooltip("Format => {0} = {Durability}, {1} = {Max Durability}")]
        public UILocaleKeySetting formatKeyDurability = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_DURABILITY);
        [Tooltip("Format => {0} = {Weight}")]
        public UILocaleKeySetting formatKeyWeight = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_WEIGHT);
        [Tooltip("Format => {0} = {Current Exp}, {1} = {Max Exp}")]
        public UILocaleKeySetting formatKeyExp = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_EXP);
        [Tooltip("Format => {0} = {Lock Remains Duration}")]
        public UILocaleKeySetting formatKeyLockRemainsDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Building Title}")]
        public UILocaleKeySetting formatKeyBuilding = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_BUILDING);
        [Tooltip("Format => {0} = {Pet Title}")]
        public UILocaleKeySetting formatKeyPet = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_PET);
        [Tooltip("Format => {0} = {Mount Title}")]
        public UILocaleKeySetting formatKeyMount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_MOUNT);
        [Tooltip("Format => {0} = {Skill Title}")]
        public UILocaleKeySetting formatKeySkill = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_SKILL);
        [Tooltip("Format => {0} = {Item Type Title}")]
        public UILocaleKeySetting formatKeyItemType = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_TYPE);

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public TextWrapper uiTextRarity;
        public TextWrapper uiTextLevel;
        public Image imageIcon;
        public TextWrapper uiTextItemType;
        public TextWrapper uiTextSellPrice;
        public TextWrapper uiTextStack;
        public TextWrapper uiTextDurability;
        public TextWrapper uiTextWeight;
        public TextWrapper uiTextExp;
        public TextWrapper uiTextLockRemainsDuration;

        [Header("Equipment - UI Elements")]
        public UIEquipmentItemRequirement uiRequirement;
        [FormerlySerializedAs("uiStats")]
        public UICharacterStats uiIncreaseStats;
        public UICharacterStats uiIncreaseStatsRate;
        public UIAttributeAmounts uiIncreaseAttributes;
        public UIAttributeAmounts uiIncreaseAttributesRate;
        public UIResistanceAmounts uiIncreaseResistances;
        public UIArmorAmounts uiIncreaseArmors;
        [FormerlySerializedAs("uiIncreaseDamageAmounts")]
        public UIDamageElementAmounts uiIncreaseDamages;
        public UISkillLevels uiIncreaseSkillLevels;
        public UIEquipmentSet uiEquipmentSet;
        public UIEquipmentSockets uiEquipmentSockets;

        [Header("Armor/Shield - UI Elements")]
        public UIArmorAmount uiArmorAmount;

        [Header("Weapon - UI Elements")]
        [FormerlySerializedAs("uiDamageAmounts")]
        public UIDamageElementAmount uiDamageAmount;

        [Header("Building - UI Elements")]
        public TextWrapper uiTextBuilding;

        [Header("Pet - UI Elements")]
        public TextWrapper uiTextPet;

        [Header("Mount - UI Elements")]
        public TextWrapper uiTextMount;

        [Header("Skill - UI Elements")]
        public TextWrapper uiTextSkill;

        [Header("Events")]
        public UnityEvent onSetLevelZeroData;
        public UnityEvent onSetNonLevelZeroData;
        public UnityEvent onSetEquippedData;
        public UnityEvent onSetUnEquippedData;
        public UnityEvent onSetUnEquippableData;
        public UnityEvent onSetStorageItemData;
        public UnityEvent onNpcSellItemDialogAppear;
        public UnityEvent onNpcSellItemDialogDisappear;
        public UnityEvent onRefineItemDialogAppear;
        public UnityEvent onRefineItemDialogDisappear;
        public UnityEvent onEnhanceSocketItemDialogAppear;
        public UnityEvent onEnhanceSocketItemDialogDisappear;
        public UnityEvent onStorageDialogAppear;
        public UnityEvent onStorageDialogDisappear;
        public UnityEvent onEnterDealingState;
        public UnityEvent onExitDealingState;

        [Header("Options")]
        [Tooltip("UIs set here will be cloned by this UI")]
        public UICharacterItem[] clones;
        public UICharacterItemDragHandler uiDragging;
        public UICharacterItem uiNextLevelItem;
        public bool showAmountWhenMaxIsOne;
        public bool showLevelAsDefault;
        public bool dontAppendRefineLevelToTitle;

        private bool isSellItemDialogAppeared;
        private bool isRefineItemDialogAppeared;
        private bool isEnhanceSocketItemDialogAppeared;
        private bool isStorageDialogAppeared;
        private bool isDealingStateEntered;
        private float lockRemainsDuration;

        public bool IsSetupAsEquipSlot { get; private set; }
        public string EquipPosition { get; private set; }
        public byte EquipSlotIndex { get; private set; }

        public void SetupAsEquipSlot(string equipPosition, byte equipSlotIndex)
        {
            IsSetupAsEquipSlot = true;
            EquipPosition = equipPosition;
            EquipSlotIndex = equipSlotIndex;
        }

        private void OnDisable()
        {
            lockRemainsDuration = 0f;
        }

        protected override void Update()
        {
            base.Update();

            if (lockRemainsDuration <= 0f)
            {
                lockRemainsDuration = CharacterItem.lockRemainsDuration;
                if (lockRemainsDuration <= 1f)
                    lockRemainsDuration = 0f;
            }

            if (lockRemainsDuration > 0f)
            {
                lockRemainsDuration -= Time.deltaTime;
                if (lockRemainsDuration <= 0f)
                    lockRemainsDuration = 0f;
            }
            else
                lockRemainsDuration = 0f;

            if (uiTextLockRemainsDuration != null)
            {
                uiTextLockRemainsDuration.text = string.Format(
                    LanguageManager.GetText(formatKeyLockRemainsDuration),
                    lockRemainsDuration.ToString("N0"));
                uiTextLockRemainsDuration.gameObject.SetActive(lockRemainsDuration > 0);
            }
        }

        protected override void UpdateUI()
        {
            Profiler.BeginSample("UICharacterItem - Update UI");
            if (!IsOwningCharacter() || !IsVisible())
                return;

            UpdateShopUIVisibility(false);
            UpdateRefineUIVisibility(false);
            UpdateEnhanceSocketUIVisibility(false);
            UpdateStorageUIVisibility(false);
            UpdateDealingState(false);
            Profiler.EndSample();
        }

        protected override void UpdateData()
        {
            if (Level <= 0)
                onSetLevelZeroData.Invoke();
            else
                onSetNonLevelZeroData.Invoke();

            if (InventoryType != InventoryType.StorageItems)
            {
                if (EquipmentItem != null)
                {
                    if (InventoryType != InventoryType.NonEquipItems)
                        onSetEquippedData.Invoke();
                    else
                        onSetUnEquippedData.Invoke();
                }
                else
                    onSetUnEquippableData.Invoke();
            }
            else
                onSetStorageItemData.Invoke();

            if (uiTextTitle != null)
            {
                string str = string.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Item == null ? LanguageManager.GetUnknowTitle() : Item.Title);
                if (!dontAppendRefineLevelToTitle && EquipmentItem != null && Level > 1)
                {
                    str = string.Format(
                        LanguageManager.GetText(formatKeyTitleWithRefineLevel),
                        Item == null ? LanguageManager.GetUnknowTitle() : Item.Title,
                        (Level - 1).ToString("N0"));
                }
                uiTextTitle.text = str;
            }

            if (uiTextDescription != null)
            {
                uiTextDescription.text = string.Format(
                    LanguageManager.GetText(formatKeyDescription),
                    Item == null ? LanguageManager.GetUnknowDescription() : Item.Description);
            }

            if (uiTextRarity != null)
            {
                uiTextRarity.text = string.Format(
                    LanguageManager.GetText(formatKeyRarityTitle),
                    Item == null ? LanguageManager.GetUnknowTitle() : Item.RarityTitle);
            }

            if (uiTextLevel != null)
            {
                if (EquipmentItem != null)
                {
                    if (showLevelAsDefault)
                    {
                        uiTextLevel.text = string.Format(
                            LanguageManager.GetText(formatKeyLevel),
                            Level.ToString("N0"));
                    }
                    else
                    {
                        uiTextLevel.text = string.Format(
                            LanguageManager.GetText(formatKeyRefineLevel),
                            (Level - 1).ToString("N0"));
                    }
                }
                else if (PetItem != null)
                {
                    uiTextLevel.text = string.Format(
                        LanguageManager.GetText(formatKeyLevel),
                        Level.ToString("N0"));
                }
                uiTextLevel.gameObject.SetActive(EquipmentItem != null || PetItem != null);
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = Item == null ? null : Item.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (uiTextItemType != null)
            {
                if (Item != null)
                {
                    switch (Item.itemType)
                    {
                        case ItemType.Junk:
                            uiTextItemType.text = string.Format(
                                LanguageManager.GetText(formatKeyItemType),
                                LanguageManager.GetText(UITextKeys.UI_ITEM_TYPE_JUNK.ToString()));
                            break;
                        case ItemType.Armor:
                            uiTextItemType.text = string.Format(
                                LanguageManager.GetText(formatKeyItemType),
                                ArmorItem.ArmorType.Title);
                            break;
                        case ItemType.Weapon:
                            uiTextItemType.text = string.Format(
                                LanguageManager.GetText(formatKeyItemType),
                                WeaponItem.WeaponType.Title);
                            break;
                        case ItemType.Shield:
                            uiTextItemType.text = string.Format(
                                LanguageManager.GetText(formatKeyItemType),
                                LanguageManager.GetText(UITextKeys.UI_ITEM_TYPE_SHIELD.ToString()));
                            break;
                        case ItemType.Potion:
                        case ItemType.AttributeIncrease:
                        case ItemType.AttributeReset:
                        case ItemType.SkillLearn:
                        case ItemType.SkillReset:
                            uiTextItemType.text = string.Format(
                                LanguageManager.GetText(formatKeyItemType),
                                LanguageManager.GetText(UITextKeys.UI_ITEM_TYPE_POTION.ToString()));
                            break;
                        case ItemType.Ammo:
                            uiTextItemType.text = string.Format(
                                LanguageManager.GetText(formatKeyItemType),
                                LanguageManager.GetText(UITextKeys.UI_ITEM_TYPE_AMMO.ToString()));
                            break;
                        case ItemType.Building:
                            uiTextItemType.text = string.Format(
                                LanguageManager.GetText(formatKeyItemType),
                                LanguageManager.GetText(UITextKeys.UI_ITEM_TYPE_BUILDING.ToString()));
                            break;
                        case ItemType.Pet:
                            uiTextItemType.text = string.Format(
                                LanguageManager.GetText(formatKeyItemType),
                                LanguageManager.GetText(UITextKeys.UI_ITEM_TYPE_PET.ToString()));
                            break;
                        case ItemType.SocketEnhancer:
                            uiTextItemType.text = string.Format(
                                LanguageManager.GetText(formatKeyItemType),
                                LanguageManager.GetText(UITextKeys.UI_ITEM_TYPE_SOCKET_ENHANCER.ToString()));
                            break;
                        case ItemType.Mount:
                            uiTextItemType.text = string.Format(
                                LanguageManager.GetText(formatKeyItemType),
                                LanguageManager.GetText(UITextKeys.UI_ITEM_TYPE_MOUNT.ToString()));
                            break;
                        case ItemType.Skill:
                            uiTextItemType.text = string.Format(
                                LanguageManager.GetText(formatKeyItemType),
                                LanguageManager.GetText(UITextKeys.UI_ITEM_TYPE_SKILL.ToString()));
                            break;
                    }
                }
            }

            if (uiTextSellPrice != null)
            {
                uiTextSellPrice.text = string.Format(
                    LanguageManager.GetText(formatKeySellPrice),
                    Item == null ? "0" : Item.sellPrice.ToString("N0"));
            }

            if (uiTextStack != null)
            {
                string stackString = "";
                if (Item == null)
                {
                    stackString = string.Format(
                        LanguageManager.GetText(formatKeyStack),
                        "0",
                        "0");
                }
                else
                {
                    stackString = string.Format(
                        LanguageManager.GetText(formatKeyStack),
                        CharacterItem.amount.ToString("N0"),
                        Item.maxStack);
                }
                uiTextStack.text = stackString;
                uiTextStack.gameObject.SetActive(CharacterItem.NotEmptySlot() && (showAmountWhenMaxIsOne || (Item != null && Item.maxStack > 1)));
            }

            if (uiTextDurability != null)
            {
                string durabilityString = "";
                if (Item == null)
                {
                    durabilityString = string.Format(
                        LanguageManager.GetText(formatKeyDurability),
                        "0",
                        "0");
                }
                else
                {
                    durabilityString = string.Format(
                        LanguageManager.GetText(formatKeyDurability),
                        CharacterItem.durability.ToString("N0"),
                        Item.maxDurability);
                }
                uiTextDurability.text = durabilityString;
                uiTextDurability.gameObject.SetActive(EquipmentItem != null && Item.maxDurability > 0);
            }

            if (uiTextWeight != null)
            {
                uiTextWeight.text = string.Format(
                    LanguageManager.GetText(formatKeyWeight),
                    Item == null ? "0" : Item.weight.ToString("N2"));
            }

            if (uiRequirement != null)
            {
                if (EquipmentItem == null || (EquipmentItem.requirement.level == 0 && EquipmentItem.requirement.character == null && EquipmentItem.CacheRequireAttributeAmounts.Count == 0))
                    uiRequirement.Hide();
                else
                {
                    uiRequirement.Show();
                    uiRequirement.Data = EquipmentItem;
                }
            }

            if (uiIncreaseStats != null)
            {
                CharacterStats stats = new CharacterStats();
                if (EquipmentItem != null)
                    stats += EquipmentItem.GetIncreaseStats(Level);
                else if (SocketEnhancerItem != null)
                    stats += SocketEnhancerItem.socketEnhanceEffect.stats;

                if (stats.IsEmpty())
                {
                    // Hide ui if stats is empty
                    uiIncreaseStats.Hide();
                }
                else
                {
                    uiIncreaseStats.displayType = UICharacterStats.DisplayType.Simple;
                    uiIncreaseStats.isBonus = true;
                    uiIncreaseStats.Show();
                    uiIncreaseStats.Data = stats;
                }
            }

            if (uiIncreaseStatsRate != null)
            {
                CharacterStats statsRate = new CharacterStats();
                if (EquipmentItem != null)
                    statsRate += EquipmentItem.GetIncreaseStatsRate(Level);
                else if (SocketEnhancerItem != null)
                    statsRate += SocketEnhancerItem.socketEnhanceEffect.statsRate;

                if (statsRate.IsEmpty())
                {
                    // Hide ui if stats is empty
                    uiIncreaseStatsRate.Hide();
                }
                else
                {
                    uiIncreaseStatsRate.displayType = UICharacterStats.DisplayType.Rate;
                    uiIncreaseStatsRate.isBonus = true;
                    uiIncreaseStatsRate.Show();
                    uiIncreaseStatsRate.Data = statsRate;
                }
            }

            if (uiIncreaseAttributes != null)
            {
                Dictionary<Attribute, float> attributes = null;
                if (EquipmentItem != null)
                    attributes = EquipmentItem.GetIncreaseAttributes(Level);
                else if (SocketEnhancerItem != null)
                    attributes = GameDataHelpers.CombineAttributes(SocketEnhancerItem.socketEnhanceEffect.attributes, attributes, 1f);

                if (attributes == null || attributes.Count == 0)
                {
                    // Hide ui if attributes is empty
                    uiIncreaseAttributes.Hide();
                }
                else
                {
                    uiIncreaseAttributes.displayType = UIAttributeAmounts.DisplayType.Simple;
                    uiIncreaseAttributes.isBonus = true;
                    uiIncreaseAttributes.Show();
                    uiIncreaseAttributes.Data = attributes;
                }
            }

            if (uiIncreaseAttributesRate != null)
            {
                Dictionary<Attribute, float> attributesRate = null;
                if (EquipmentItem != null)
                    attributesRate = EquipmentItem.GetIncreaseAttributesRate(Level);
                else if (SocketEnhancerItem != null)
                    attributesRate = GameDataHelpers.CombineAttributes(SocketEnhancerItem.socketEnhanceEffect.attributesRate, attributesRate, 1f);

                if (attributesRate == null || attributesRate.Count == 0)
                {
                    // Hide ui if attributes is empty
                    uiIncreaseAttributesRate.Hide();
                }
                else
                {
                    uiIncreaseAttributesRate.displayType = UIAttributeAmounts.DisplayType.Rate;
                    uiIncreaseAttributesRate.isBonus = true;
                    uiIncreaseAttributesRate.Show();
                    uiIncreaseAttributesRate.Data = attributesRate;
                }
            }

            if (uiIncreaseResistances != null)
            {
                Dictionary<DamageElement, float> resistances = null;
                if (EquipmentItem != null)
                    resistances = EquipmentItem.GetIncreaseResistances(Level);
                else if (SocketEnhancerItem != null)
                    resistances = GameDataHelpers.CombineResistances(SocketEnhancerItem.socketEnhanceEffect.resistances, resistances, 1f);

                if (resistances == null || resistances.Count == 0)
                {
                    // Hide ui if resistances is empty
                    uiIncreaseResistances.Hide();
                }
                else
                {
                    uiIncreaseResistances.isBonus = true;
                    uiIncreaseResistances.Show();
                    uiIncreaseResistances.Data = resistances;
                }
            }

            if (uiIncreaseArmors != null)
            {
                Dictionary<DamageElement, float> armors = null;
                if (EquipmentItem != null)
                    armors = EquipmentItem.GetIncreaseArmors(Level);
                else if (SocketEnhancerItem != null)
                    armors = GameDataHelpers.CombineArmors(SocketEnhancerItem.socketEnhanceEffect.armors, armors, 1f);

                if (armors == null || armors.Count == 0)
                {
                    // Hide ui if armors is empty
                    uiIncreaseArmors.Hide();
                }
                else
                {
                    uiIncreaseArmors.isBonus = true;
                    uiIncreaseArmors.Show();
                    uiIncreaseArmors.Data = armors;
                }
            }

            if (uiIncreaseDamages != null)
            {
                Dictionary<DamageElement, MinMaxFloat> damageAmounts = null;
                if (EquipmentItem != null)
                    damageAmounts = EquipmentItem.GetIncreaseDamages(Level);
                else if (SocketEnhancerItem != null)
                    damageAmounts = GameDataHelpers.CombineDamages(SocketEnhancerItem.socketEnhanceEffect.damages, damageAmounts, 1f);

                if (damageAmounts == null || damageAmounts.Count == 0)
                {
                    // Hide ui if damage amounts is empty
                    uiIncreaseDamages.Hide();
                }
                else
                {
                    uiIncreaseDamages.isBonus = true;
                    uiIncreaseDamages.Show();
                    uiIncreaseDamages.Data = damageAmounts;
                }
            }

            if (uiIncreaseSkillLevels != null)
            {
                Dictionary<BaseSkill, short> skillLevels = null;
                if (EquipmentItem != null)
                    skillLevels = EquipmentItem.GetIncreaseSkills();
                else if (SocketEnhancerItem != null)
                    skillLevels = GameDataHelpers.CombineSkills(SocketEnhancerItem.socketEnhanceEffect.skills, skillLevels);

                if (skillLevels == null || skillLevels.Count == 0)
                {
                    // Hide ui if skill levels is empty
                    uiIncreaseSkillLevels.Hide();
                }
                else
                {
                    uiIncreaseSkillLevels.displayType = UISkillLevels.DisplayType.Simple;
                    uiIncreaseSkillLevels.isBonus = true;
                    uiIncreaseSkillLevels.Show();
                    uiIncreaseSkillLevels.Data = skillLevels;
                }
            }

            if (uiEquipmentSet != null)
            {
                if (EquipmentItem == null || EquipmentItem.equipmentSet == null || EquipmentItem.equipmentSet.effects.Length == 0)
                {
                    // Only equipment item has equipment set data
                    uiEquipmentSet.Hide();
                }
                else
                {
                    uiEquipmentSet.Show();
                    int equippedCount = 0;
                    Character.GetCaches().EquipmentSets.TryGetValue(EquipmentItem.equipmentSet, out equippedCount);
                    uiEquipmentSet.Data = new UIEquipmentSetData(EquipmentItem.equipmentSet, equippedCount);
                }
            }

            if (uiEquipmentSockets != null)
            {
                if (EquipmentItem == null || EquipmentItem.maxSocket <= 0)
                    uiEquipmentSockets.Hide();
                else
                {
                    uiEquipmentSockets.Show();
                    uiEquipmentSockets.Data = new UIEquipmentSocketsData(CharacterItem.Sockets, EquipmentItem.maxSocket);
                }
            }

            if (uiArmorAmount != null)
            {
                if (DefendItem == null)
                    uiArmorAmount.Hide();
                else
                {
                    uiArmorAmount.Show();
                    KeyValuePair<DamageElement, float> kvPair = CharacterItem.GetArmorAmount();
                    uiArmorAmount.Data = new UIArmorAmountData(kvPair.Key, kvPair.Value);
                }
            }

            if (uiDamageAmount != null)
            {
                if (WeaponItem == null)
                    uiDamageAmount.Hide();
                else
                {
                    uiDamageAmount.Show();
                    KeyValuePair<DamageElement, MinMaxFloat> kvPair = CharacterItem.GetDamageAmount(null);
                    uiDamageAmount.Data = new UIDamageElementAmountData(kvPair.Key, kvPair.Value);
                }
            }

            if (PetItem != null && PetItem.petEntity != null)
            {
                int[] expTree = GameInstance.Singleton.ExpTree;
                int currentExp = 0;
                int nextLevelExp = 0;
                if (CharacterItem.GetNextLevelExp() > 0)
                {
                    currentExp = CharacterItem.exp;
                    nextLevelExp = CharacterItem.GetNextLevelExp();
                }
                else if (Level - 2 > 0 && Level - 2 < expTree.Length)
                {
                    int maxExp = expTree[Level - 2];
                    currentExp = maxExp;
                    nextLevelExp = maxExp;
                }

                if (uiTextExp != null)
                {
                    uiTextExp.text = string.Format(
                        LanguageManager.GetText(formatKeyExp),
                        currentExp.ToString("N0"),
                        nextLevelExp.ToString("N0"));
                    uiTextExp.gameObject.SetActive(true);
                }
            }
            else
            {
                if (uiTextExp != null)
                    uiTextExp.gameObject.SetActive(false);
            }

            if (uiTextBuilding != null)
            {
                if (BuildingItem == null || BuildingItem.petEntity == null)
                    uiTextBuilding.gameObject.SetActive(false);
                else
                {
                    uiTextBuilding.gameObject.SetActive(true);
                    uiTextBuilding.text = string.Format(
                        LanguageManager.GetText(formatKeyBuilding),
                        BuildingItem.buildingEntity.Title);
                }
            }

            if (uiTextPet != null)
            {
                if (PetItem == null || PetItem.petEntity == null)
                    uiTextPet.gameObject.SetActive(false);
                else
                {
                    uiTextPet.gameObject.SetActive(true);
                    uiTextPet.text = string.Format(
                        LanguageManager.GetText(formatKeyPet),
                        PetItem.petEntity.Title);
                }
            }

            if (uiTextMount != null)
            {
                if (MountItem == null || MountItem.mountEntity == null)
                    uiTextMount.gameObject.SetActive(false);
                else
                {
                    uiTextMount.gameObject.SetActive(true);
                    uiTextMount.text = string.Format(
                        LanguageManager.GetText(formatKeyMount),
                        MountItem.mountEntity.Title);
                }
            }

            if (uiTextSkill != null)
            {
                if (SkillItem == null || SkillItem.skillLevel.skill == null)
                    uiTextSkill.gameObject.SetActive(false);
                else
                {
                    uiTextSkill.gameObject.SetActive(true);
                    uiTextSkill.text = string.Format(
                        LanguageManager.GetText(formatKeySkill),
                        SkillItem.skillLevel.skill.Title,
                        SkillItem.skillLevel.level);
                }
            }

            if (clones != null && clones.Length > 0)
            {
                for (int i = 0; i < clones.Length; ++i)
                {
                    if (clones[i] == null) continue;
                    clones[i].Data = Data;
                }
            }

            if (uiNextLevelItem != null)
            {
                if (Level + 1 > Item.MaxLevel)
                    uiNextLevelItem.Hide();
                else
                {
                    uiNextLevelItem.Setup(new UICharacterItemData(CharacterItem, (short)(Level + 1), InventoryType), Character, IndexOfData);
                    uiNextLevelItem.Show();
                }
            }
            UpdateShopUIVisibility(true);
            UpdateRefineUIVisibility(true);
            UpdateEnhanceSocketUIVisibility(true);
            UpdateStorageUIVisibility(true);
            UpdateDealingState(true);
        }

        private void UpdateShopUIVisibility(bool initData)
        {
            if (!IsOwningCharacter())
            {
                if (initData || isSellItemDialogAppeared)
                {
                    isSellItemDialogAppeared = false;
                    if (onNpcSellItemDialogDisappear != null)
                        onNpcSellItemDialogDisappear.Invoke();
                }
                return;
            }
            // Check visible item dialog
            UISceneGameplay uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiNpcDialog != null &&
                uiGameplay.uiNpcDialog.IsVisible() &&
                uiGameplay.uiNpcDialog.Data != null &&
                uiGameplay.uiNpcDialog.Data.type == NpcDialogType.Shop &&
                InventoryType == InventoryType.NonEquipItems)
            {
                if (initData || !isSellItemDialogAppeared)
                {
                    isSellItemDialogAppeared = true;
                    if (onNpcSellItemDialogAppear != null)
                        onNpcSellItemDialogAppear.Invoke();
                }
            }
            else
            {
                if (initData || isSellItemDialogAppeared)
                {
                    isSellItemDialogAppeared = false;
                    if (onNpcSellItemDialogDisappear != null)
                        onNpcSellItemDialogDisappear.Invoke();
                }
            }
        }

        private void UpdateRefineUIVisibility(bool initData)
        {
            if (!IsOwningCharacter())
            {
                if (initData || isRefineItemDialogAppeared)
                {
                    isRefineItemDialogAppeared = false;
                    if (onRefineItemDialogDisappear != null)
                        onRefineItemDialogDisappear.Invoke();
                }
                return;
            }
            // Check visible item dialog
            UISceneGameplay uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiRefineItem != null &&
                uiGameplay.uiRefineItem.IsVisible() &&
                Data.characterItem.GetEquipmentItem() != null &&
                InventoryType == InventoryType.NonEquipItems)
            {
                if (initData || !isRefineItemDialogAppeared)
                {
                    isRefineItemDialogAppeared = true;
                    if (onRefineItemDialogAppear != null)
                        onRefineItemDialogAppear.Invoke();
                }
            }
            else
            {
                if (initData || isRefineItemDialogAppeared)
                {
                    isRefineItemDialogAppeared = false;
                    if (onRefineItemDialogDisappear != null)
                        onRefineItemDialogDisappear.Invoke();
                }
            }
        }

        private void UpdateEnhanceSocketUIVisibility(bool initData)
        {
            if (!IsOwningCharacter())
            {
                if (initData || isEnhanceSocketItemDialogAppeared)
                {
                    isEnhanceSocketItemDialogAppeared = false;
                    if (onEnhanceSocketItemDialogDisappear != null)
                        onEnhanceSocketItemDialogDisappear.Invoke();
                }
                return;
            }
            // Check visible item dialog
            UISceneGameplay uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiEnhanceSocketItem != null &&
                uiGameplay.uiEnhanceSocketItem.IsVisible() &&
                Data.characterItem.GetEquipmentItem() != null &&
                InventoryType == InventoryType.NonEquipItems)
            {
                if (initData || !isEnhanceSocketItemDialogAppeared)
                {
                    isEnhanceSocketItemDialogAppeared = true;
                    if (onEnhanceSocketItemDialogAppear != null)
                        onEnhanceSocketItemDialogAppear.Invoke();
                }
            }
            else
            {
                if (initData || isEnhanceSocketItemDialogAppeared)
                {
                    isEnhanceSocketItemDialogAppeared = false;
                    if (onEnhanceSocketItemDialogDisappear != null)
                        onEnhanceSocketItemDialogDisappear.Invoke();
                }
            }
        }

        private void UpdateStorageUIVisibility(bool initData)
        {
            if (!IsOwningCharacter())
            {
                if (initData || isStorageDialogAppeared)
                {
                    isStorageDialogAppeared = false;
                    if (onStorageDialogDisappear != null)
                        onStorageDialogDisappear.Invoke();
                }
                return;
            }
            // Check visible item dialog
            UISceneGameplay uiGameplay = UISceneGameplay.Singleton;
            bool isAnyStorageVisible =
                (uiGameplay.uiPlayerStorageItems != null && uiGameplay.uiPlayerStorageItems.IsVisible()) ||
                (uiGameplay.uiGuildStorageItems != null && uiGameplay.uiGuildStorageItems.IsVisible()) ||
                (uiGameplay.uiBuildingStorageItems != null && uiGameplay.uiBuildingStorageItems.IsVisible());
            if (isAnyStorageVisible &&
                InventoryType == InventoryType.NonEquipItems)
            {
                if (initData || !isStorageDialogAppeared)
                {
                    isStorageDialogAppeared = true;
                    if (onStorageDialogAppear != null)
                        onStorageDialogAppear.Invoke();
                }
            }
            else
            {
                if (initData || isStorageDialogAppeared)
                {
                    isStorageDialogAppeared = false;
                    if (onStorageDialogDisappear != null)
                        onStorageDialogDisappear.Invoke();
                }
            }
        }

        private void UpdateDealingState(bool initData)
        {
            if (!IsOwningCharacter())
            {
                if (initData || isDealingStateEntered)
                {
                    isDealingStateEntered = false;
                    if (onExitDealingState != null)
                        onExitDealingState.Invoke();
                }
                return;
            }
            // Check visible dealing dialog
            UISceneGameplay uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiDealing.IsVisible() &&
                uiGameplay.uiDealing.dealingState == DealingState.Dealing &&
                InventoryType == InventoryType.NonEquipItems)
            {
                if (initData || !isDealingStateEntered)
                {
                    isDealingStateEntered = true;
                    if (onEnterDealingState != null)
                        onEnterDealingState.Invoke();
                }
            }
            else
            {
                if (initData || isDealingStateEntered)
                {
                    isDealingStateEntered = false;
                    if (onExitDealingState != null)
                        onExitDealingState.Invoke();
                }
            }
        }

        public void OnClickEquip()
        {
            // Only unequpped equipment can be equipped
            if (!IsOwningCharacter() || InventoryType != InventoryType.NonEquipItems)
                return;

            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();

            OwningCharacter.RequestEquipItem((short)IndexOfData);
        }

        public void OnClickUnEquip()
        {
            // Only equipped equipment can be unequipped
            if (!IsOwningCharacter() || InventoryType == InventoryType.NonEquipItems)
                return;

            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();

            OwningCharacter.RequestUnEquipItem(InventoryType, (short)IndexOfData, CharacterItem.equipSlotIndex);
        }

        #region Drop Item Functions
        public void OnClickDrop()
        {
            // Only unequipped equipment can be dropped
            if (!IsOwningCharacter() || InventoryType != InventoryType.NonEquipItems)
                return;

            if (CharacterItem.amount == 1)
            {
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
                OwningCharacter.RequestDropItem((short)IndexOfData, 1);
            }
            else
                UISceneGlobal.Singleton.ShowInputDialog(LanguageManager.GetText(UITextKeys.UI_DROP_ITEM.ToString()), LanguageManager.GetText(UITextKeys.UI_DROP_ITEM_DESCRIPTION.ToString()), OnDropAmountConfirmed, 1, CharacterItem.amount, CharacterItem.amount);
        }

        private void OnDropAmountConfirmed(int amount)
        {
            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();
            OwningCharacter.RequestDropItem((short)IndexOfData, (short)amount);
        }
        #endregion

        #region Sell Item Functions
        public void OnClickSell()
        {
            // Only unequipped equipment can be sell
            if (!IsOwningCharacter() || InventoryType != InventoryType.NonEquipItems)
                return;

            if (CharacterItem.amount == 1)
            {
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
                OwningCharacter.RequestSellItem((short)IndexOfData, 1);
            }
            else
                UISceneGlobal.Singleton.ShowInputDialog(LanguageManager.GetText(UITextKeys.UI_SELL_ITEM.ToString()), LanguageManager.GetText(UITextKeys.UI_SELL_ITEM_DESCRIPTION.ToString()), OnSellItemAmountConfirmed, 1, CharacterItem.amount, CharacterItem.amount);
        }

        private void OnSellItemAmountConfirmed(int amount)
        {
            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();
            OwningCharacter.RequestSellItem((short)IndexOfData, (short)amount);
        }
        #endregion

        #region Set Dealing Item Functions
        public void OnClickSetDealingItem()
        {
            // Only unequipped equipment can be sold
            if (!IsOwningCharacter() || InventoryType != InventoryType.NonEquipItems)
                return;

            if (CharacterItem.amount == 1)
            {
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
                OwningCharacter.RequestSetDealingItem((short)IndexOfData, 1);
            }
            else
                UISceneGlobal.Singleton.ShowInputDialog(LanguageManager.GetText(UITextKeys.UI_OFFER_ITEM.ToString()), LanguageManager.GetText(UITextKeys.UI_OFFER_ITEM_DESCRIPTION.ToString()), OnSetDealingItemAmountConfirmed, 1, CharacterItem.amount, CharacterItem.amount);
        }

        private void OnSetDealingItemAmountConfirmed(int amount)
        {
            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();
            OwningCharacter.RequestSetDealingItem((short)IndexOfData, (short)amount);
        }
        #endregion

        #region Move To Storage Functions
        public void OnClickMoveToStorage()
        {
            OnClickMoveToStorage(-1);
        }

        public void OnClickMoveToStorage(int storageIndex)
        {
            // Only unequipped equipment can be moved to storage
            if (!IsOwningCharacter() || InventoryType != InventoryType.NonEquipItems)
                return;

            if (CharacterItem.amount == 1)
            {
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
                OwningCharacter.RequestMoveItemToStorage((short)IndexOfData, 1, (short)storageIndex);
            }
            else
            {
                UISceneGlobal.Singleton.ShowInputDialog(LanguageManager.GetText(UITextKeys.UI_MOVE_ITEM_TO_STORAGE.ToString()), LanguageManager.GetText(UITextKeys.UI_MOVE_ITEM_TO_STORAGE_DESCRIPTION.ToString()), (amount) =>
                {
                    OnClickMoveToStorage(amount, storageIndex);
                }, 1, CharacterItem.amount, CharacterItem.amount);
            }
        }

        private void OnClickMoveToStorage(int amount, int storageIndex)
        {
            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();
            OwningCharacter.RequestMoveItemToStorage((short)IndexOfData, (short)amount, (short)storageIndex);
        }
        #endregion

        #region Move From Storage Functions
        public void OnClickMoveFromStorage()
        {
            OnClickMoveFromStorage(-1);
        }

        public void OnClickMoveFromStorage(int nonEquipIndex)
        {
            // Only storage items can be moved from storage
            if (!IsOwningCharacter() || InventoryType != InventoryType.StorageItems)
                return;

            if (CharacterItem.amount == 1)
            {
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
                OwningCharacter.RequestMoveItemFromStorage((short)IndexOfData, 1, (short)nonEquipIndex);
            }
            else
            {
                UISceneGlobal.Singleton.ShowInputDialog(LanguageManager.GetText(UITextKeys.UI_MOVE_ITEM_FROM_STORAGE.ToString()), LanguageManager.GetText(UITextKeys.UI_MOVE_ITEM_FROM_STORAGE_DESCRIPTION.ToString()), (amount) =>
                {
                    OnClickMoveFromStorage(amount, nonEquipIndex);
                }, 1, CharacterItem.amount, CharacterItem.amount);
            }
        }

        private void OnClickMoveFromStorage(int amount, int nonEquipIndex)
        {
            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();
            OwningCharacter.RequestMoveItemFromStorage((short)IndexOfData, (short)amount, (short)nonEquipIndex);
        }
        #endregion

        #region Set Refine Item Functions
        public void OnClickSetRefineItem()
        {
            // Only owning character can refine item
            if (!IsOwningCharacter())
                return;

            UISceneGameplay uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiRefineItem != null &&
                CharacterItem.GetEquipmentItem() != null)
            {
                uiGameplay.uiRefineItem.Data = new UICharacterItemByIndexData(InventoryType, IndexOfData);
                uiGameplay.uiRefineItem.Show();
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
            }
        }
        #endregion

        #region Set Enhance Socket Item Functions
        public void OnClickSetEnhanceSocketItem()
        {
            // Only owning character can refine item
            if (!IsOwningCharacter())
                return;

            UISceneGameplay uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiEnhanceSocketItem != null &&
                CharacterItem.GetEquipmentItem() != null)
            {
                uiGameplay.uiEnhanceSocketItem.Data = new UICharacterItemByIndexData(InventoryType, IndexOfData);
                uiGameplay.uiEnhanceSocketItem.Show();
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
            }
        }
        #endregion
    }
}
