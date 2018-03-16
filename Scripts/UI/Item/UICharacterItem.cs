using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterItem : UISelectionEntry<CharacterItem>
{
    [System.NonSerialized]
    public int indexOfData;
    [System.NonSerialized]
    public string equipPosition;

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
    [Tooltip("Armor Format => {0} = {Armor}")]
    public string armorFormat = "Armor: {0}";

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

    [Header("Armor/Shield - UI Elements")]
    public Text textArmor;

    [Header("Weapon - UI Elements")]
    public UIDamageElementAmount uiDamageAttribute;

    [Header("Action Buttons")]
    public Button buttonEquip;
    public Button buttonUnEquip;
    public Button buttonDrop;

    [Header("Options")]
    public bool hideAmountWhenMaxIsOne;

    protected override void UpdateData()
    {
        var itemData = Data.GetItem();
        var itemLevel = Data.level;
        var equipmentItem = Data.GetEquipmentItem();
        var defendItem = Data.GetDefendItem();
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
                textItemType.text = string.Format(itemTypeFormat, armorItem.armorType.title);
            else if (weaponItem != null)
                textItemType.text = string.Format(itemTypeFormat, weaponItem.WeaponType.title);
            else if (shieldItem != null)
                textItemType.text = string.Format(itemTypeFormat, shieldItemType);
            else
                textItemType.text = string.Format(itemTypeFormat, generalItemType);
        }

        if (uiRequirement != null)
        {
            if (equipmentItem == null || (equipmentItem.requirement.characterLevel == 0 && equipmentItem.requirement.characterClass == null && equipmentItem.CacheRequireAttributeAmounts.Count == 0))
                uiRequirement.Hide();
            else
            {
                uiRequirement.Show();
                uiRequirement.Data = new KeyValuePair<BaseEquipmentItem, int>(equipmentItem, itemLevel);
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

        if (textArmor != null)
        {
            if (defendItem == null)
                textArmor.gameObject.SetActive(false);
            else
            {
                textArmor.text = string.Format(armorFormat, defendItem.GetArmor(itemLevel));
                textArmor.gameObject.SetActive(true);
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

        var owningCharacter = CharacterEntity.OwningCharacter;
        /*
        if (owningCharacter != null)
            owningCharacter.EquipItem(indexOfData, targetEquipPosition);*/
    }

    private void OnClickUnEquip()
    {
        // Only equipped equipment can be unequipped
        if (string.IsNullOrEmpty(equipPosition))
            return;
        
        var owningCharacter = CharacterEntity.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.UnEquipItem(equipPosition);
    }

    private void OnClickDrop()
    {
        // Only unequpped equipment can be dropped
        if (!string.IsNullOrEmpty(equipPosition))
            return;

        UISceneGlobal.Singleton.ShowInputDialog(dropInputTitle, dropInputDescription, OnDropAmountConfirmed);
    }

    private void OnDropAmountConfirmed(int amount)
    {
        var owningCharacter = CharacterEntity.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.DropItem(indexOfData, amount);
    }
}

[System.Serializable]
public class UICharacterItemEvent : UnityEvent<UICharacterItem> { }
