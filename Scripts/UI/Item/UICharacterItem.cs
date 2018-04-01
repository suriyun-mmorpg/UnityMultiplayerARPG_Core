using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterItem : UISelectionEntry<CharacterItem>
{
    public int indexOfData { get; protected set; }
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
    [Tooltip("Weight Format => {0} = {Weight}")]
    public string weightFormat = "{0}";
    [Tooltip("Item Type Format => {0} = {Item Type title}")]
    public string itemTypeFormat = "Item Type: {0}";
    [Tooltip("General Item Type")]
    public string generalItemType = "General Item";
    [Tooltip("Shield Item Type")]
    public string shieldItemType = "Shield";

    [Header("Input Dialog Settings")]
    public string dropInputTitle = "Drop Item";
    public string dropInputDescription = "";

    [Header("UI Elements")]
    public Text textTitle;
    public Text textDescription;
    public Text textLevel;
    public Image imageIcon;
    public Text textSellPrice;
    public Text textStack;
    public Text textWeight;
    public Text textItemType;

    [Header("Equipment - UI Elements")]
    public UIEquipmentItemRequirement uiRequirement;
    public UICharacterStats uiStats;
    public UIAttributeAmounts uiIncreaseAttributes;
    public UIResistanceAmounts uiIncreaseResistances;
    public UIDamageElementAmounts uiIncreaseDamageAttributes;

    [Header("Weapon - UI Elements")]
    public UIDamageElementAmount uiDamageAttribute;

    [Header("Action Buttons")]
    public Button buttonEquip;
    public Button buttonUnEquip;
    public Button buttonDrop;

    [Header("Options")]
    public bool hideAmountWhenMaxIsOne;

    public void Setup(CharacterItem data, int indexOfData, string equipPosition)
    {
        this.indexOfData = indexOfData;
        this.equipPosition = equipPosition;
        Data = data;
    }

    protected override void UpdateData()
    {
        var itemData = Data.GetItem();
        var itemLevel = Data.level;
        var equipmentItem = Data.GetEquipmentItem();
        var armorItem = Data.GetArmorItem();
        var weaponItem = Data.GetWeaponItem();
        var shieldItem = Data.GetShieldItem();

        if (buttonEquip != null)
        {
            buttonEquip.gameObject.SetActive(string.IsNullOrEmpty(equipPosition));
            buttonEquip.onClick.RemoveListener(OnClickEquip);
            buttonEquip.onClick.AddListener(OnClickEquip);
        }

        if (buttonUnEquip != null)
        {
            buttonUnEquip.gameObject.SetActive(!string.IsNullOrEmpty(equipPosition));
            buttonUnEquip.onClick.RemoveListener(OnClickUnEquip);
            buttonUnEquip.onClick.AddListener(OnClickUnEquip);
        }

        if (buttonDrop != null)
        {
            buttonDrop.gameObject.SetActive(string.IsNullOrEmpty(equipPosition));
            buttonDrop.onClick.RemoveListener(OnClickDrop);
            buttonDrop.onClick.AddListener(OnClickDrop);
        }

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, itemData == null ? "Unknow" : itemData.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, itemData == null ? "N/A" : itemData.description);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, itemLevel.ToString("N0"));

        if (imageIcon != null)
        {
            imageIcon.sprite = itemData == null ? null : itemData.icon;
            imageIcon.gameObject.SetActive(itemData != null);
        }

        if (textSellPrice != null)
            textSellPrice.text = string.Format(sellPriceFormat, itemData == null ? "0" : itemData.sellPrice.ToString("N0"));

        if (textStack != null)
        {
            var stackString = "";
            if (!hideAmountWhenMaxIsOne)
            {
                if (itemData == null)
                    stackString = string.Format(stackFormat, "0", "0");
                else
                    stackString = string.Format(stackFormat, Data.amount.ToString("N0"), itemData.maxStack);
            }
            textStack.text = stackString;
        }

        if (textWeight != null)
            textWeight.text = string.Format(weightFormat, itemData == null ? "0" : itemData.weight.ToString("N2"));

        if (textItemType != null)
        {
            if (armorItem != null)
                textItemType.text = string.Format(itemTypeFormat, armorItem.ArmorType.title);
            else if (weaponItem != null)
                textItemType.text = string.Format(itemTypeFormat, weaponItem.WeaponType.title);
            else if (shieldItem != null)
                textItemType.text = string.Format(itemTypeFormat, shieldItemType);
            else
                textItemType.text = string.Format(itemTypeFormat, generalItemType);
        }

        if (uiRequirement != null)
        {
            if (equipmentItem == null || (equipmentItem.requirement.level == 0 && equipmentItem.requirement.character == null && equipmentItem.CacheRequireAttributeAmounts.Count == 0))
                uiRequirement.Hide();
            else
            {
                uiRequirement.Show();
                uiRequirement.Data = new KeyValuePair<Item, int>(equipmentItem, itemLevel);
            }
        }

        if (uiStats != null)
        {
            var stats = equipmentItem.GetStats(itemLevel);
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
            var attributes = equipmentItem.GetIncreaseAttributes(itemLevel);
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
            var resistances = equipmentItem.GetIncreaseResistances(itemLevel);
            if (equipmentItem == null || resistances == null || resistances.Count == 0)
                uiIncreaseResistances.Hide();
            else
            {
                uiIncreaseResistances.Show();
                uiIncreaseResistances.Data = resistances;
            }
        }

        if (uiIncreaseDamageAttributes != null)
        {
            var damageAttributes = equipmentItem.GetIncreaseDamageAttributes(itemLevel);
            if (equipmentItem == null || damageAttributes == null || damageAttributes.Count == 0)
                uiIncreaseDamageAttributes.Hide();
            else
            {
                uiIncreaseDamageAttributes.Show();
                uiIncreaseDamageAttributes.Data = damageAttributes;
            }
        }

        if (uiDamageAttribute != null)
        {
            if (weaponItem == null)
                uiDamageAttribute.Hide();
            else
            {
                uiDamageAttribute.Show();
                uiDamageAttribute.Data = weaponItem.GetDamageAttribute(itemLevel, 0f, 1f);
            }
        }
    }

    private void OnClickEquip()
    {
        // Only unequpped equipment can be equipped
        if (!string.IsNullOrEmpty(equipPosition))
            return;
        
        if (selectionManager != null)
            selectionManager.DeselectSelectedUI();

        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        if (owningCharacter != null)
        {
            var armorItem = Data.GetArmorItem();
            var weaponItem = Data.GetWeaponItem();
            var shieldItem = Data.GetShieldItem();
            if (weaponItem != null)
            {
                if (weaponItem.EquipType == WeaponItemEquipType.OneHandCanDual)
                {
                    var equipWeapons = owningCharacter.EquipWeapons;
                    var rightWeapon = equipWeapons.rightHand.GetWeaponItem();
                    if (rightWeapon != null && rightWeapon.EquipType == WeaponItemEquipType.OneHandCanDual)
                        owningCharacter.EquipItem(indexOfData, GameDataConst.EQUIP_POSITION_LEFT_HAND);
                    else
                        owningCharacter.EquipItem(indexOfData, GameDataConst.EQUIP_POSITION_RIGHT_HAND);
                }
                else
                    owningCharacter.EquipItem(indexOfData, GameDataConst.EQUIP_POSITION_RIGHT_HAND);
            }
            else if (shieldItem != null)
                owningCharacter.EquipItem(indexOfData, GameDataConst.EQUIP_POSITION_LEFT_HAND);
            else if (armorItem != null)
                owningCharacter.EquipItem(indexOfData, armorItem.EquipPosition);
        }
    }

    private void OnClickUnEquip()
    {
        // Only equipped equipment can be unequipped
        if (string.IsNullOrEmpty(equipPosition))
            return;

        if (selectionManager != null)
            selectionManager.DeselectSelectedUI();

        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.UnEquipItem(equipPosition);
    }

    private void OnClickDrop()
    {
        // Only unequpped equipment can be dropped
        if (!string.IsNullOrEmpty(equipPosition))
            return;

        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        if (Data.amount == 1)
        {
            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();
            if (owningCharacter != null)
                owningCharacter.DropItem(indexOfData, 1);
        }
        else
            UISceneGlobal.Singleton.ShowInputDialog(dropInputTitle, dropInputDescription, OnDropAmountConfirmed, 1, Data.amount, Data.amount);
    }

    private void OnDropAmountConfirmed(int amount)
    {
        var owningCharacter = PlayerCharacterEntity.OwningCharacter;
        if (selectionManager != null)
            selectionManager.DeselectSelectedUI();
        if (owningCharacter != null)
            owningCharacter.DropItem(indexOfData, amount);
    }
}

[System.Serializable]
public class UICharacterItemEvent : UnityEvent<UICharacterItem> { }
