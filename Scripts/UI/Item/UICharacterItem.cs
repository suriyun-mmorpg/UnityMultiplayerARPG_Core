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
    [Tooltip("Sell Price Format => {0} = {Sell price}")]
    public string sellPriceFormat = "{0}";
    [Tooltip("Stack Format => {0} = {Amount}, {1} = {Max stack}")]
    public string stackFormat = "{0}/{1}";
    [Tooltip("Weight Format => {0} = {Weight}")]
    public string weightFormat = "{0}";

    [Header("Requirement Format")]
    [Tooltip("Require Level Format => {0} = {Level}")]
    public string requireLevelFormat = "Require Level: {0}";
    [Tooltip("Require Class Format => {0} = {Class title}")]
    public string requireClassFormat = "Require Class: {0}";

    [Header("Weapon Info Format")]
    [Tooltip("Weapon Type Format => {0} = {Weapon Type title}")]
    public string weaponTypeFormat = "Weapon Type: {0}";

    [Header("UI Elements")]
    public Text textTitle;
    public Text textDescription;
    public Image imageIcon;
    public Text textSellPrice;
    public Text textStack;
    public Text textWeight;

    [Header("Equipment - UI Elements")]
    public UICharacterStats uiStats;
    public UIAttributeAmounts uiIncreaseAttributes;
    public UIResistanceAmounts uiIncreaseResistances;
    public UIDamageElementAmounts uiIncreaseDamageAttributes;

    [Header("Equipment Requirement - UI Elements")]
    public Text textRequireLevel;
    public Text textRequireClass;
    public UIAttributeAmounts uiRequireAttributeAmounts;

    [Header("Weapon - UI Elements")]
    public Text textWeaponType;
    public UIDamageElementAmount uiDamageAttribute;

    protected override void UpdateData()
    {
        var itemData = Data.GetItem();

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, itemData == null ? "Unknow" : itemData.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, itemData == null ? "N/A" : itemData.description);

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
            textWeight.text = string.Format(weightFormat, itemData == null ? "0" : itemData.weight.ToString("N0"));

        var equipmentItem = Data.GetEquipmentItem();

        if (textRequireLevel != null)
        {
            if (equipmentItem == null)
                textRequireLevel.gameObject.SetActive(false);
            else
            {
                textRequireLevel.text = string.Format(requireLevelFormat, equipmentItem.requirement.characterLevel.ToString("N0"));
                textRequireLevel.gameObject.SetActive(true);
            }
        }

        if (textRequireClass != null)
        {
            if (equipmentItem == null || equipmentItem.requirement.characterClass == null)
                textRequireClass.gameObject.SetActive(false);
            else
            {
                textRequireClass.text = string.Format(requireClassFormat, equipmentItem.requirement.characterClass.title);
                textRequireClass.gameObject.SetActive(true);
            }
        }

        if (uiRequireAttributeAmounts != null)
        {
            if (equipmentItem == null)
                uiRequireAttributeAmounts.gameObject.SetActive(false);
            else
            {
                uiRequireAttributeAmounts.Data = equipmentItem.CacheRequireAttributeAmounts;
                uiRequireAttributeAmounts.gameObject.SetActive(true);
            }
        }

        if (uiStats != null)
        {
            if (equipmentItem == null)
                uiStats.Hide();
            else
            {
                uiStats.Data = Data.GetStats();
                uiStats.Show();
            }
        }

        if (uiIncreaseAttributes != null)
        {
            if (equipmentItem == null)
                uiIncreaseAttributes.Hide();
            else
            {
                uiIncreaseAttributes.Data = equipmentItem.GetIncreaseAttributes(Data.level);
                uiIncreaseAttributes.Show();
            }
        }

        if (uiIncreaseResistances != null)
        {
            if (equipmentItem == null)
                uiIncreaseResistances.Hide();
            else
            {
                uiIncreaseResistances.Data = equipmentItem.GetIncreaseResistances(Data.level);
                uiIncreaseResistances.Show();
            }
        }

        if (uiIncreaseDamageAttributes != null)
        {
            if (equipmentItem == null)
                uiIncreaseDamageAttributes.Hide();
            else
            {
                uiIncreaseDamageAttributes.Data = equipmentItem.GetIncreaseDamageAttributes(Data.level);
                uiIncreaseDamageAttributes.Show();
            }
        }

        var weaponItem = Data.GetWeaponItem();

        if (textWeaponType != null)
        {
            if (weaponItem == null)
                textWeaponType.gameObject.SetActive(false);
            else
            {
                textWeaponType.text = string.Format(weaponTypeFormat, weaponItem.WeaponType.title);
                textWeaponType.gameObject.SetActive(true);
            }
        }

        if (uiDamageAttribute != null)
        {
            if (weaponItem == null)
                uiDamageAttribute.Hide();
            else
            {
                uiDamageAttribute.Data = weaponItem.GetDamageAttribute(Data.level, 1f);
                uiDamageAttribute.Show();
            }
        }
    }
}

[System.Serializable]
public class UICharacterItemEvent : UnityEvent<UICharacterItem> { }
