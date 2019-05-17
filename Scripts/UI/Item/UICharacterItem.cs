using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterItem : UIDataForCharacter<CharacterItemTuple>
    {
        public CharacterItem CharacterItem { get { return Data.characterItem; } }
        public short Level { get { return Data.targetLevel; } }
        public InventoryType InventoryType { get { return Data.inventoryType; } }
        public Item Item { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetItem() : null; } }
        public Item EquipmentItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetEquipmentItem() : null; } }
        public Item ArmorItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetArmorItem() : null; } }
        public Item WeaponItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetWeaponItem() : null; } }
        public Item PotionItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetPotionItem() : null; } }
        public Item AmmoItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetAmmoItem() : null; } }
        public Item BuildingItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetBuildingItem() : null; } }
        public Item PetItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetPetItem() : null; } }
        public Item SocketEnhancerItem { get { return CharacterItem != null && CharacterItem.NotEmptySlot() ? CharacterItem.GetSocketEnhancerItem() : null; } }

        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("Description Format => {0} = {Description}")]
        public string descriptionFormat = "{0}";
        [Tooltip("Rarity Title Format => {0} = {Rarity Title}, {1} = {Rarity Label}")]
        public string rarityTitleFormat = "{1}: {0}";
        [Tooltip("Level Format => {0} = {Level}, {1} = {Level Label}")]
        public string levelFormat = "{1}: {0}";
        [Tooltip("Refine Level Format => {0} = {Refine Level}, {1} = {Level Label}")]
        public string refineLevelFormat = "{1}: +{0}";
        [Tooltip("Title Refine Level Format => {0} = {Refine Level}")]
        public string titleRefineLevelFormat = " (+{0})";
        [Tooltip("Sell Price Format => {0} = {Sell price}, {1} = {Sell price Label}")]
        public string sellPriceFormat = "{1}: {0}";
        [Tooltip("Stack Format => {0} = {Amount}, {1} = {Max stack}, {2} = {Stack Amount Label}")]
        public string stackFormat = "{2}: {0}/{1}";
        [Tooltip("Durability Format => {0} = {Durability}, {1} = {Max durability}, {2} = {Durability Label}")]
        public string durabilityFormat = "{2}: {0}/{1}";
        [Tooltip("Weight Format => {0} = {Weight}, {1} = {Weight Label}")]
        public string weightFormat = "{1}: {0}";
        [Tooltip("Exp Format => {0} = {Current exp}, {1} = {Max exp}, {2} = {Exp Label}")]
        public string expFormat = "{2}: {0}/{1}";
        [Tooltip("Lock Remains Duration Format => {0} = {Lock Remains duration}")]
        public string lockRemainsDurationFormat = "{0}";
        [Tooltip("Item Type Format => {0} = {Item Type title}, {1} = {Item Type Label}")]
        public string itemTypeFormat = "{1}: {0}";

        [Header("Input Dialog Settings")]
        public string dropInputTitle = "Drop Item";
        public string dropInputDescription = "";
        public string sellInputTitle = "Sell Item";
        public string sellInputDescription = "";
        public string setDealingInputTitle = "Offer Item";
        public string setDealingInputDescription = "";
        public string moveToStorageInputTitle = "Move To Storage";
        public string moveToStorageInputDescription = "";
        public string moveFromStorageInputTitle = "Move From Storage";
        public string moveFromStorageInputDescription = "";

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
        public UICharacterStats uiStats;
        public UIAttributeAmounts uiIncreaseAttributes;
        public UIResistanceAmounts uiIncreaseResistances;
        public UIDamageElementAmounts uiIncreaseDamageAmounts;
        public UISkillLevels uiIncreaseSkillLevels;
        public UIEquipmentSet uiEquipmentSet;
        public UIEquipmentSockets uiEquipmentSockets;

        [Header("Weapon - UI Elements")]
        public UIDamageElementAmount uiDamageAmounts;

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

        protected float lockRemainsDuration;

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
                uiTextLockRemainsDuration.text = string.Format(lockRemainsDurationFormat, Mathf.CeilToInt(lockRemainsDuration).ToString("N0"));
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
                string str = string.Format(titleFormat, Item == null ? LanguageManager.GetUnknowTitle() : Item.Title);
                if (!dontAppendRefineLevelToTitle && EquipmentItem != null)
                    str += string.Format(titleRefineLevelFormat, (Level - 1).ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_LEVEL.ToString()));
                uiTextTitle.text = str;
            }

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, Item == null ? LanguageManager.GetUnknowDescription() : Item.Description);

            if (uiTextRarity != null)
                uiTextRarity.text = string.Format(rarityTitleFormat, Item == null ? LanguageManager.GetUnknowTitle() : Item.RarityTitle, LanguageManager.GetText(UILocaleKeys.UI_ITEM_RARITY.ToString()));

            if (uiTextLevel != null)
            {
                if (EquipmentItem != null)
                {
                    if (showLevelAsDefault)
                        uiTextLevel.text = string.Format(levelFormat, Level.ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_LEVEL.ToString()));
                    else
                        uiTextLevel.text = string.Format(refineLevelFormat, (Level - 1).ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_LEVEL.ToString()));
                }
                else if (PetItem != null)
                {
                    uiTextLevel.text = string.Format(levelFormat, Level.ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_LEVEL.ToString()));
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
                            uiTextItemType.text = string.Format(itemTypeFormat, LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_JUNK.ToString()), LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_LABEL.ToString()));
                            break;
                        case ItemType.Armor:
                            uiTextItemType.text = string.Format(itemTypeFormat, ArmorItem.ArmorType.Title, LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_LABEL.ToString()));
                            break;
                        case ItemType.Weapon:
                            uiTextItemType.text = string.Format(itemTypeFormat, WeaponItem.WeaponType.Title, LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_LABEL.ToString()));
                            break;
                        case ItemType.Shield:
                            uiTextItemType.text = string.Format(itemTypeFormat, LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_SHIELD.ToString()), LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_LABEL.ToString()));
                            break;
                        case ItemType.Potion:
                            uiTextItemType.text = string.Format(itemTypeFormat, LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_POTION.ToString()), LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_LABEL.ToString()));
                            break;
                        case ItemType.Ammo:
                            uiTextItemType.text = string.Format(itemTypeFormat, LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_AMMO.ToString()), LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_LABEL.ToString()));
                            break;
                        case ItemType.Building:
                            uiTextItemType.text = string.Format(itemTypeFormat, LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_BUILDING.ToString()), LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_LABEL.ToString()));
                            break;
                        case ItemType.Pet:
                            uiTextItemType.text = string.Format(itemTypeFormat, LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_PET.ToString()), LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_LABEL.ToString()));
                            break;
                        case ItemType.SocketEnhancer:
                            uiTextItemType.text = string.Format(itemTypeFormat, LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_SOCKET_ENHANCER.ToString()), LanguageManager.GetText(UILocaleKeys.UI_ITEM_TYPE_LABEL.ToString()));
                            break;
                    }
                }
            }

            if (uiTextSellPrice != null)
                uiTextSellPrice.text = string.Format(sellPriceFormat, Item == null ? "0" : Item.sellPrice.ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_SELL_PRICE.ToString()));

            if (uiTextStack != null)
            {
                string stackString = "";
                if (Item == null)
                    stackString = string.Format(stackFormat, "0", "0", LanguageManager.GetText(UILocaleKeys.UI_ITEM_AMOUNT.ToString()));
                else
                    stackString = string.Format(stackFormat, CharacterItem.amount.ToString("N0"), Item.maxStack, LanguageManager.GetText(UILocaleKeys.UI_ITEM_AMOUNT.ToString()));
                uiTextStack.text = stackString;
                uiTextStack.gameObject.SetActive(showAmountWhenMaxIsOne || (Item != null && Item.maxStack > 1));
            }

            if (uiTextDurability != null)
            {
                string durabilityString = "";
                if (Item == null)
                    durabilityString = string.Format(durabilityFormat, "0", "0", LanguageManager.GetText(UILocaleKeys.UI_ITEM_DURABILITY.ToString()));
                else
                    durabilityString = string.Format(durabilityFormat, CharacterItem.durability.ToString("N0"), Item.maxDurability, LanguageManager.GetText(UILocaleKeys.UI_ITEM_DURABILITY.ToString()));
                uiTextDurability.text = durabilityString;
                uiTextDurability.gameObject.SetActive(EquipmentItem != null && Item.maxDurability > 0);
            }

            if (uiTextWeight != null)
                uiTextWeight.text = string.Format(weightFormat, Item == null ? "0" : Item.weight.ToString("N2"), LanguageManager.GetText(UILocaleKeys.UI_WEIGHT.ToString()));

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

            if (uiStats != null)
            {
                CharacterStats stats = CharacterStats.Empty;
                if (EquipmentItem != null)
                    stats = EquipmentItem.GetIncreaseStats(Level, CharacterItem.GetEquipmentBonusRate());
                else if (SocketEnhancerItem != null)
                    stats = SocketEnhancerItem.socketEnhanceEffect.stats;

                if (stats.IsEmpty())
                {
                    // Hide ui if stats is empty
                    uiStats.Hide();
                }
                else
                {
                    uiStats.Show();
                    uiStats.Data = stats;
                }
            }

            if (uiIncreaseAttributes != null)
            {
                Dictionary<Attribute, short> attributes = null;
                if (EquipmentItem != null)
                    attributes = EquipmentItem.GetIncreaseAttributes(Level, CharacterItem.GetEquipmentBonusRate());
                else if (SocketEnhancerItem != null)
                    attributes = GameDataHelpers.CombineAttributes(SocketEnhancerItem.socketEnhanceEffect.attributes, attributes, 1f);
                
                if (attributes == null || attributes.Count == 0)
                {
                    // Hide ui if attributes is empty
                    uiIncreaseAttributes.Hide();
                }
                else
                {
                    uiIncreaseAttributes.Show();
                    uiIncreaseAttributes.Data = attributes;
                }
            }

            if (uiIncreaseResistances != null)
            {
                Dictionary<DamageElement, float> resistances = null;
                if (EquipmentItem != null)
                    resistances = EquipmentItem.GetIncreaseResistances(Level, CharacterItem.GetEquipmentBonusRate());
                else if (SocketEnhancerItem != null)
                    resistances = GameDataHelpers.CombineResistances(SocketEnhancerItem.socketEnhanceEffect.resistances, resistances, 1f);

                if (resistances == null || resistances.Count == 0)
                {
                    // Hide ui if resistances is empty
                    uiIncreaseResistances.Hide();
                }
                else
                {
                    uiIncreaseResistances.Show();
                    uiIncreaseResistances.Data = resistances;
                }
            }

            if (uiIncreaseDamageAmounts != null)
            {
                Dictionary<DamageElement, MinMaxFloat> damageAmounts = null;
                if (EquipmentItem != null)
                    damageAmounts = EquipmentItem.GetIncreaseDamages(Level, CharacterItem.GetEquipmentBonusRate());
                else if (SocketEnhancerItem != null)
                    damageAmounts = GameDataHelpers.CombineDamages(SocketEnhancerItem.socketEnhanceEffect.damages, damageAmounts, 1f);

                if (damageAmounts == null || damageAmounts.Count == 0)
                {
                    // Hide ui if damage amounts is empty
                    uiIncreaseDamageAmounts.Hide();
                }
                else
                {
                    uiIncreaseDamageAmounts.Show();
                    uiIncreaseDamageAmounts.Data = damageAmounts;
                }
            }

            if (uiIncreaseSkillLevels != null)
            {
                Dictionary<Skill, short> skillLevels = null;
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
                    Character.CacheEquipmentSets.TryGetValue(EquipmentItem.equipmentSet, out equippedCount);
                    uiEquipmentSet.Data = new EquipmentSetWithEquippedCountTuple(EquipmentItem.equipmentSet, equippedCount);
                }
            }

            if (uiEquipmentSockets != null)
            {
                if (EquipmentItem == null || EquipmentItem.maxSocket <= 0)
                    uiEquipmentSockets.Hide();
                else
                {
                    uiEquipmentSockets.Show();
                    uiEquipmentSockets.Data = new EnhancedSocketsWithMaxSocketTuple(CharacterItem.Sockets, EquipmentItem.maxSocket);
                }
            }

            if (uiDamageAmounts != null)
            {
                if (WeaponItem == null)
                    uiDamageAmounts.Hide();
                else
                {
                    uiDamageAmounts.Show();
                    KeyValuePair<DamageElement, MinMaxFloat> keyValuePair = WeaponItem.GetDamageAmount(Level, CharacterItem.GetEquipmentBonusRate(), null);
                    uiDamageAmounts.Data = new DamageElementAmountTuple(keyValuePair.Key, keyValuePair.Value);
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
                    uiTextExp.text = string.Format(expFormat, currentExp.ToString("N0"), nextLevelExp.ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_EXP.ToString()));
                    uiTextExp.gameObject.SetActive(true);
                }
            }
            else
            {
                if (uiTextExp != null)
                    uiTextExp.gameObject.SetActive(false);
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
                    uiNextLevelItem.Setup(new CharacterItemTuple(CharacterItem, (short)(Level + 1), InventoryType), Character, IndexOfData);
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

            OwningCharacter.RequestUnEquipItem((byte)InventoryType, (short)IndexOfData);
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
                UISceneGlobal.Singleton.ShowInputDialog(dropInputTitle, dropInputDescription, OnDropAmountConfirmed, 1, CharacterItem.amount, CharacterItem.amount);
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
                UISceneGlobal.Singleton.ShowInputDialog(sellInputTitle, sellInputDescription, OnSellItemAmountConfirmed, 1, CharacterItem.amount, CharacterItem.amount);
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
                UISceneGlobal.Singleton.ShowInputDialog(setDealingInputTitle, setDealingInputDescription, OnSetDealingItemAmountConfirmed, 1, CharacterItem.amount, CharacterItem.amount);
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
                UISceneGlobal.Singleton.ShowInputDialog(moveToStorageInputTitle, moveToStorageInputDescription, (amount) =>
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
                UISceneGlobal.Singleton.ShowInputDialog(moveFromStorageInputTitle, moveFromStorageInputDescription, (amount) =>
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
                uiGameplay.uiRefineItem.Data = new CharacterItemByIndexTuple(InventoryType, IndexOfData);
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
                uiGameplay.uiEnhanceSocketItem.Data = new CharacterItemByIndexTuple(InventoryType, IndexOfData);
                uiGameplay.uiEnhanceSocketItem.Show();
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
            }
        }
        #endregion
    }
}
