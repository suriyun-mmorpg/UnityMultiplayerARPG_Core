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

    protected override void UpdateData()
    {
        var itemData = Data.GetItem();
        var itemLevel = Data.level;
        var equipmentItem = Data.GetEquipmentItem();
        var defendItem = Data.GetDefendItem();
        var armorItem = Data.GetArmorItem();
        var weaponItem = Data.GetWeaponItem();
        var shieldItem = Data.GetShieldItem();

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
            if (itemData == null)
                stackString = string.Format(stackFormat, "0", "0");
            else
                stackString = string.Format(stackFormat, Data.amount.ToString("N0"), itemData.maxStack);
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
                uiRequirement.Data = new KeyValuePair<BaseEquipmentItem, int>(equipmentItem, itemLevel);
                uiRequirement.Show();
            }
        }

        if (uiStats != null)
        {
            if (equipmentItem == null)
                uiStats.Hide();
            else
            {
                uiStats.Data = equipmentItem.GetStats(itemLevel);
                uiStats.Show();
            }
        }

        if (uiIncreaseAttributes != null)
        {
            if (equipmentItem == null)
                uiIncreaseAttributes.Hide();
            else
            {
                uiIncreaseAttributes.Data = equipmentItem.GetIncreaseAttributes(itemLevel);
                uiIncreaseAttributes.Show();
            }
        }

        if (uiIncreaseResistances != null)
        {
            if (equipmentItem == null)
                uiIncreaseResistances.Hide();
            else
            {
                uiIncreaseResistances.Data = equipmentItem.GetIncreaseResistances(itemLevel);
                uiIncreaseResistances.Show();
            }
        }

        if (uiIncreaseDamageAttributes != null)
        {
            if (equipmentItem == null)
                uiIncreaseDamageAttributes.Hide();
            else
            {
                uiIncreaseDamageAttributes.Data = equipmentItem.GetIncreaseDamageAttributes(itemLevel);
                uiIncreaseDamageAttributes.Show();
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
                uiDamageAttribute.Data = weaponItem.GetDamageAttribute(itemLevel, 0f, 1f);
                uiDamageAttribute.Show();
            }
        }
    }
}

[System.Serializable]
public class UICharacterItemEvent : UnityEvent<UICharacterItem> { }
