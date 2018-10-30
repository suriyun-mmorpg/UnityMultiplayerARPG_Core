using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterItem : UIDataForCharacter<CharacterItemTuple>
    {
        public CharacterItem characterItem { get { return Data.characterItem; } }
        public short level { get { return Data.targetLevel; } }
        public string equipPosition { get { return Data.equipPosition; } }
        public Item item { get { return characterItem != null ? characterItem.GetItem() : null; } }
        public Item equipmentItem { get { return characterItem != null ? characterItem.GetEquipmentItem() : null; } }
        public Item armorItem { get { return characterItem != null ? characterItem.GetArmorItem() : null; } }
        public Item weaponItem { get { return characterItem != null ? characterItem.GetWeaponItem() : null; } }

        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("Description Format => {0} = {Description}")]
        public string descriptionFormat = "{0}";
        [Tooltip("Level Format => {0} = {Level}")]
        public string levelFormat = "Lv: {0}";
        [Tooltip("Refine Level Format => {0} = {Refine Level}")]
        public string refineLevelFormat = "+{0}";
        [Tooltip("Title Refine Level Format => {0} = {Refine Level}")]
        public string titleRefineLevelFormat = " (+{0})";
        [Tooltip("Sell Price Format => {0} = {Sell price}")]
        public string sellPriceFormat = "{0}";
        [Tooltip("Stack Format => {0} = {Amount}, {1} = {Max stack}")]
        public string stackFormat = "{0}/{1}";
        [Tooltip("Durability Format => {0} = {Durability}, {1} = {Max durability}")]
        public string durabilityFormat = "{0}/{1}";
        [Tooltip("Weight Format => {0} = {Weight}")]
        public string weightFormat = "{0}";
        [Tooltip("Item Type Format => {0} = {Item Type title}")]
        public string itemTypeFormat = "Item Type: {0}";
        [Tooltip("Junk Item Type")]
        public string junkItemType = "Junk";
        [Tooltip("Shield Item Type")]
        public string shieldItemType = "Shield";
        [Tooltip("Potion Item Type")]
        public string potionItemType = "Potion";
        [Tooltip("Ammo Item Type")]
        public string ammoItemType = "Ammo";
        [Tooltip("Building Item Type")]
        public string buildingItemType = "Building";

        [Header("Input Dialog Settings")]
        public string dropInputTitle = "Drop Item";
        public string dropInputDescription = "";
        public string sellInputTitle = "Sell Item";
        public string sellInputDescription = "";
        public string setDealingInputTitle = "Offer Item";
        public string setDealingInputDescription = "";

        [Header("UI Elements")]
        public Text textTitle;
        public TextWrapper uiTextTitle;
        public Text textDescription;
        public TextWrapper uiTextDescription;
        public Text textLevel;
        public TextWrapper uiTextLevel;
        public Image imageIcon;
        public Text textItemType;
        public TextWrapper uiTextItemType;
        public Text textSellPrice;
        public TextWrapper uiTextSellPrice;
        public Text textStack;
        public TextWrapper uiTextStack;
        public Text textDurability;
        public TextWrapper uiTextDurability;
        public Text textWeight;
        public TextWrapper uiTextWeight;

        [Header("Equipment - UI Elements")]
        public UIEquipmentItemRequirement uiRequirement;
        public UICharacterStats uiStats;
        public UIAttributeAmounts uiIncreaseAttributes;
        public UIResistanceAmounts uiIncreaseResistances;
        public UIDamageElementAmounts uiIncreaseDamageAmounts;

        [Header("Weapon - UI Elements")]
        public UIDamageElementAmount uiDamageAmounts;

        [Header("Events")]
        public UnityEvent onSetLevelZeroData;
        public UnityEvent onSetNonLevelZeroData;
        public UnityEvent onSetEquippedData;
        public UnityEvent onSetUnEquippedData;
        public UnityEvent onSetUnEquippableData;
        public UnityEvent onNpcSellItemDialogAppear;
        public UnityEvent onNpcSellItemDialogDisappear;
        public UnityEvent onRefineItemAppear;
        public UnityEvent onRefineItemDisappear;
        public UnityEvent onEnterDealingState;
        public UnityEvent onExitDealingState;

        [Header("Options")]
        public UICharacterItem uiNextLevelItem;
        public bool showAmountWhenMaxIsOne;
        public bool showLevelAsDefault;
        public bool dontAppendRefineLevelToTitle;

        private bool isSellItemDialogAppeared;
        private bool isRefineItemAppeared;
        private bool isDealingStateEntered;

        protected override void UpdateUI()
        {
            Profiler.BeginSample("UICharacterItem - Update UI");
            if (!IsOwningCharacter() || !IsVisible())
                return;

            UpdateShopUIVisibility(false);
            UpdateRefineUIVisibility(false);
            UpdateDealingState(false);
            Profiler.EndSample();
        }

        protected override void UpdateData()
        {
            MigrateUIComponents();

            if (level <= 0)
                onSetLevelZeroData.Invoke();
            else
                onSetNonLevelZeroData.Invoke();

            if (equipmentItem != null)
            {
                if (!string.IsNullOrEmpty(equipPosition))
                    onSetEquippedData.Invoke();
                else
                    onSetUnEquippedData.Invoke();
            }
            else
                onSetUnEquippableData.Invoke();

            if (uiTextTitle != null)
            {
                var str = string.Format(titleFormat, item == null ? "Unknow" : item.title);
                if (!dontAppendRefineLevelToTitle)
                    str += string.Format(titleRefineLevelFormat, (level - 1).ToString("N0"));
                uiTextTitle.text = str;
            }

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, item == null ? "N/A" : item.description);

            if (uiTextLevel != null)
            {
                if (showLevelAsDefault)
                    uiTextLevel.text = string.Format(levelFormat, level.ToString("N0"));
                else
                    uiTextLevel.text = string.Format(refineLevelFormat, (level - 1).ToString("N0"));
            }

            if (imageIcon != null)
            {
                var iconSprite = item == null ? null : item.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (uiTextItemType != null)
            {
                switch (item.itemType)
                {
                    case ItemType.Junk:
                        uiTextItemType.text = string.Format(itemTypeFormat, junkItemType);
                        break;
                    case ItemType.Armor:
                        uiTextItemType.text = string.Format(itemTypeFormat, armorItem.ArmorType.title);
                        break;
                    case ItemType.Weapon:
                        uiTextItemType.text = string.Format(itemTypeFormat, weaponItem.WeaponType.title);
                        break;
                    case ItemType.Shield:
                        uiTextItemType.text = string.Format(itemTypeFormat, shieldItemType);
                        break;
                    case ItemType.Potion:
                        uiTextItemType.text = string.Format(itemTypeFormat, potionItemType);
                        break;
                    case ItemType.Ammo:
                        uiTextItemType.text = string.Format(itemTypeFormat, ammoItemType);
                        break;
                    case ItemType.Building:
                        uiTextItemType.text = string.Format(itemTypeFormat, buildingItemType);
                        break;
                }
            }

            if (uiTextSellPrice != null)
                uiTextSellPrice.text = string.Format(sellPriceFormat, item == null ? "0" : item.sellPrice.ToString("N0"));

            if (uiTextStack != null)
            {
                var stackString = "";
                if (item == null)
                    stackString = string.Format(stackFormat, "0", "0");
                else
                    stackString = string.Format(stackFormat, characterItem.amount.ToString("N0"), item.maxStack);
                uiTextStack.text = stackString;
                uiTextStack.gameObject.SetActive(showAmountWhenMaxIsOne || item.maxStack > 1);
            }

            if (uiTextDurability != null)
            {
                var durabilityString = "";
                if (item == null)
                    durabilityString = string.Format(durabilityFormat, "0", "0");
                else
                    durabilityString = string.Format(durabilityFormat, characterItem.durability.ToString("N0"), item.maxDurability);
                uiTextDurability.text = durabilityString;
                uiTextDurability.gameObject.SetActive(equipmentItem != null && item.maxDurability > 0);
            }

            if (uiTextWeight != null)
                uiTextWeight.text = string.Format(weightFormat, item == null ? "0" : item.weight.ToString("N2"));

            if (uiRequirement != null)
            {
                if (equipmentItem == null || (equipmentItem.requirement.level == 0 && equipmentItem.requirement.character == null && equipmentItem.CacheRequireAttributeAmounts.Count == 0))
                    uiRequirement.Hide();
                else
                {
                    uiRequirement.Show();
                    uiRequirement.Data = equipmentItem;
                }
            }

            if (uiStats != null)
            {
                var stats = equipmentItem.GetIncreaseStats(level, characterItem.GetEquipmentBonusRate());
                if (equipmentItem == null || stats.IsEmpty())
                    uiStats.Hide();
                else
                {
                    uiStats.Show();
                    uiStats.Data = stats;
                }
            }

            if (uiIncreaseAttributes != null)
            {
                var attributes = equipmentItem.GetIncreaseAttributes(level, characterItem.GetEquipmentBonusRate());
                if (equipmentItem == null || attributes == null || attributes.Count == 0)
                    uiIncreaseAttributes.Hide();
                else
                {
                    uiIncreaseAttributes.Show();
                    uiIncreaseAttributes.Data = attributes;
                }
            }

            if (uiIncreaseResistances != null)
            {
                var resistances = equipmentItem.GetIncreaseResistances(level, characterItem.GetEquipmentBonusRate());
                if (equipmentItem == null || resistances == null || resistances.Count == 0)
                    uiIncreaseResistances.Hide();
                else
                {
                    uiIncreaseResistances.Show();
                    uiIncreaseResistances.Data = resistances;
                }
            }

            if (uiIncreaseDamageAmounts != null)
            {
                var damageAmounts = equipmentItem.GetIncreaseDamages(level, characterItem.GetEquipmentBonusRate());
                if (equipmentItem == null || damageAmounts == null || damageAmounts.Count == 0)
                    uiIncreaseDamageAmounts.Hide();
                else
                {
                    uiIncreaseDamageAmounts.Show();
                    uiIncreaseDamageAmounts.Data = damageAmounts;
                }
            }

            if (uiDamageAmounts != null)
            {
                if (weaponItem == null)
                    uiDamageAmounts.Hide();
                else
                {
                    uiDamageAmounts.Show();
                    var keyValuePair = weaponItem.GetDamageAmount(level, characterItem.GetEquipmentBonusRate(), null);
                    uiDamageAmounts.Data = new DamageElementAmountTuple(keyValuePair.Key, keyValuePair.Value);
                }
            }

            if (uiNextLevelItem != null)
            {
                if (level + 1 > item.MaxLevel)
                    uiNextLevelItem.Hide();
                else
                {
                    uiNextLevelItem.Setup(new CharacterItemTuple(characterItem, (short)(level + 1), equipPosition), character, indexOfData);
                    uiNextLevelItem.Show();
                }
            }
            UpdateShopUIVisibility(true);
            UpdateRefineUIVisibility(true);
            UpdateDealingState(true);
        }

        private void UpdateShopUIVisibility(bool initData)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
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
            var uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiNpcDialog != null)
            {
                if (uiGameplay.uiNpcDialog.IsVisible() &&
                    uiGameplay.uiNpcDialog.Data != null &&
                    uiGameplay.uiNpcDialog.Data.type == NpcDialogType.Shop &&
                    string.IsNullOrEmpty(equipPosition))
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
        }

        private void UpdateRefineUIVisibility(bool initData)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
            {
                if (initData || isRefineItemAppeared)
                {
                    isRefineItemAppeared = false;
                    if (onRefineItemDisappear != null)
                        onRefineItemDisappear.Invoke();
                }
                return;
            }
            // Check visible item dialog
            var uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiRefineItem != null)
            {
                if (uiGameplay.uiRefineItem.IsVisible() &&
                    Data.characterItem != null &&
                    Data.characterItem.GetEquipmentItem() != null &&
                    string.IsNullOrEmpty(equipPosition))
                {
                    if (initData || !isRefineItemAppeared)
                    {
                        isRefineItemAppeared = true;
                        if (onRefineItemAppear != null)
                            onRefineItemAppear.Invoke();
                    }
                }
                else
                {
                    if (initData || isRefineItemAppeared)
                    {
                        isRefineItemAppeared = false;
                        if (onRefineItemDisappear != null)
                            onRefineItemDisappear.Invoke();
                    }
                }
            }
        }

        private void UpdateDealingState(bool initData)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
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
            var uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiDealing != null)
            {
                if (uiGameplay.uiDealing.IsVisible() &&
                    uiGameplay.uiDealing.dealingState == DealingState.Dealing &&
                    string.IsNullOrEmpty(equipPosition))
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
        }

        public void OnClickEquip()
        {
            // Only unequpped equipment can be equipped
            if (!IsOwningCharacter() || !string.IsNullOrEmpty(equipPosition))
                return;

            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();

            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestEquipItem(indexOfData);
        }

        public void OnClickUnEquip()
        {
            // Only equipped equipment can be unequipped
            if (!IsOwningCharacter() || string.IsNullOrEmpty(equipPosition))
                return;

            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();

            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestUnEquipItem(equipPosition);
        }

        public void OnClickDrop()
        {
            // Only unequipped equipment can be dropped
            if (!IsOwningCharacter() || !string.IsNullOrEmpty(equipPosition))
                return;
            
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (characterItem.amount == 1)
            {
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
                if (owningCharacter != null)
                    owningCharacter.RequestDropItem(indexOfData, 1);
            }
            else
                UISceneGlobal.Singleton.ShowInputDialog(dropInputTitle, dropInputDescription, OnDropAmountConfirmed, 1, characterItem.amount, characterItem.amount);
        }

        private void OnDropAmountConfirmed(int amount)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();
            if (owningCharacter != null)
                owningCharacter.RequestDropItem(indexOfData, (short)amount);
        }

        public void OnClickSell()
        {
            // Only unequipped equipment can be sell
            if (!IsOwningCharacter() || !string.IsNullOrEmpty(equipPosition))
                return;
            
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (characterItem.amount == 1)
            {
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
                if (owningCharacter != null)
                    owningCharacter.RequestSellItem(indexOfData, 1);
            }
            else
                UISceneGlobal.Singleton.ShowInputDialog(sellInputTitle, sellInputDescription, OnSellItemAmountConfirmed, 1, characterItem.amount, characterItem.amount);
        }

        private void OnSellItemAmountConfirmed(int amount)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();
            if (owningCharacter != null)
                owningCharacter.RequestSellItem(indexOfData, (short)amount);
        }

        public void OnClickSetDealingItem()
        {
            // Only unequipped equipment can be sell
            if (!IsOwningCharacter() || !string.IsNullOrEmpty(equipPosition))
                return;
            
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (characterItem.amount == 1)
            {
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
                if (owningCharacter != null)
                    owningCharacter.RequestSetDealingItem(indexOfData, 1);
            }
            else
                UISceneGlobal.Singleton.ShowInputDialog(setDealingInputTitle, setDealingInputDescription, OnSetDealingItemAmountConfirmed, 1, characterItem.amount, characterItem.amount);
        }

        private void OnSetDealingItemAmountConfirmed(int amount)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();
            if (owningCharacter != null)
                owningCharacter.RequestSetDealingItem(indexOfData, (short)amount);
        }

        public void OnClickSetRefineItem()
        {
            // Only unequipped equipment can refining
            if (!IsOwningCharacter() || !string.IsNullOrEmpty(equipPosition))
                return;
            
            var uiGameplay = UISceneGameplay.Singleton;
            if (uiGameplay.uiRefineItem != null &&
                characterItem.GetEquipmentItem() != null &&
                string.IsNullOrEmpty(equipPosition))
            {
                uiGameplay.uiRefineItem.Data = indexOfData;
                uiGameplay.uiRefineItem.Show();
                if (selectionManager != null)
                    selectionManager.DeselectSelectedUI();
            }
        }

        [ContextMenu("Migrate UI Components")]
        public void MigrateUIComponents()
        {
            uiTextTitle = MigrateUIHelpers.SetWrapperToText(textTitle, uiTextTitle);
            uiTextDescription = MigrateUIHelpers.SetWrapperToText(textDescription, uiTextDescription);
            uiTextLevel = MigrateUIHelpers.SetWrapperToText(textLevel, uiTextLevel);
            uiTextItemType = MigrateUIHelpers.SetWrapperToText(textItemType, uiTextItemType);
            uiTextSellPrice = MigrateUIHelpers.SetWrapperToText(textSellPrice, uiTextSellPrice);
            uiTextStack = MigrateUIHelpers.SetWrapperToText(textStack, uiTextStack);
            uiTextDurability = MigrateUIHelpers.SetWrapperToText(textDurability, uiTextDurability);
            uiTextWeight = MigrateUIHelpers.SetWrapperToText(textWeight, uiTextWeight);
        }
    }
}
