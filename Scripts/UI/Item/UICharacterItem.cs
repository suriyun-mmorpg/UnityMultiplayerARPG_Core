using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UICharacterItem : UIDataForCharacter<CharacterItemLevelTuple>
    {
        public string equipPosition { get; protected set; }

        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("Description Format => {0} = {Description}")]
        public string descriptionFormat = "{0}";
        [Tooltip("Level Format => {0} = {Level}")]
        public string levelFormat = "Lv: {0}";
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
        public Text textDescription;
        public Text textLevel;
        public Image imageIcon;
        public Text textItemType;
        public Text textSellPrice;
        public Text textStack;
        public Text textDurability;
        public Text textWeight;

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
        public UnityEvent onEnterDealingState;
        public UnityEvent onExitDealingState;

        [Header("Options")]
        public UICharacterItem uiNextLevelItem;
        public bool showAmountWhenMaxIsOne;

        private bool isSellItemDialogAppeared;
        private bool isDealingStateEntered;

        public void Setup(CharacterItemLevelTuple data, ICharacterData character, int indexOfData, string equipPosition)
        {
            this.equipPosition = equipPosition;
            Setup(data, character, indexOfData);
        }

        private void Update()
        {
            if (!IsOwningCharacter() || !IsVisible())
                return;

            UpdateShopUIVisibility(false);
            UpdateDealingState(false);
        }

        protected override void UpdateData()
        {
            var characterItem = Data.characterItem;
            var level = Data.targetLevel;
            var item = characterItem.GetItem();
            var equipmentItem = characterItem.GetEquipmentItem();
            var armorItem = characterItem.GetArmorItem();
            var weaponItem = characterItem.GetWeaponItem();

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

            if (textTitle != null)
                textTitle.text = string.Format(titleFormat, item == null ? "Unknow" : item.title);

            if (textDescription != null)
                textDescription.text = string.Format(descriptionFormat, item == null ? "N/A" : item.description);

            if (textLevel != null)
                textLevel.text = string.Format(levelFormat, level.ToString("N0"));

            if (imageIcon != null)
            {
                var iconSprite = item == null ? null : item.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (textItemType != null)
            {
                switch (item.itemType)
                {
                    case ItemType.Junk:
                        textItemType.text = string.Format(itemTypeFormat, junkItemType);
                        break;
                    case ItemType.Armor:
                        textItemType.text = string.Format(itemTypeFormat, armorItem.ArmorType.title);
                        break;
                    case ItemType.Weapon:
                        textItemType.text = string.Format(itemTypeFormat, weaponItem.WeaponType.title);
                        break;
                    case ItemType.Shield:
                        textItemType.text = string.Format(itemTypeFormat, shieldItemType);
                        break;
                    case ItemType.Potion:
                        textItemType.text = string.Format(itemTypeFormat, potionItemType);
                        break;
                    case ItemType.Ammo:
                        textItemType.text = string.Format(itemTypeFormat, ammoItemType);
                        break;
                    case ItemType.Building:
                        textItemType.text = string.Format(itemTypeFormat, buildingItemType);
                        break;
                }
            }

            if (textSellPrice != null)
                textSellPrice.text = string.Format(sellPriceFormat, item == null ? "0" : item.sellPrice.ToString("N0"));

            if (textStack != null)
            {
                var stackString = "";
                if (item == null)
                    stackString = string.Format(stackFormat, "0", "0");
                else
                    stackString = string.Format(stackFormat, characterItem.amount.ToString("N0"), item.maxStack);
                textStack.text = stackString;
                textStack.gameObject.SetActive(showAmountWhenMaxIsOne || item.maxStack > 1);
            }

            if (textDurability != null)
            {
                var durabilityString = "";
                if (item == null)
                    durabilityString = string.Format(durabilityFormat, "0", "0");
                else
                    durabilityString = string.Format(durabilityFormat, characterItem.durability.ToString("N0"), item.maxDurability);
                textDurability.text = durabilityString;
                textDurability.gameObject.SetActive(equipmentItem != null && item.maxDurability > 0);
            }

            if (textWeight != null)
                textWeight.text = string.Format(weightFormat, item == null ? "0" : item.weight.ToString("N2"));

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
                if (level + 1 > item.maxLevel)
                    uiNextLevelItem.Hide();
                else
                {
                    uiNextLevelItem.Setup(new CharacterItemLevelTuple(characterItem, (short)(level + 1)), character, indexOfData, equipPosition);
                    uiNextLevelItem.Show();
                }
            }
            UpdateShopUIVisibility(true);
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

            var characterItem = Data.characterItem;
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

            var characterItem = Data.characterItem;
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

            var characterItem = Data.characterItem;
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
    }
}
