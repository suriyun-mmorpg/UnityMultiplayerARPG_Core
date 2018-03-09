using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterItem : UISelectionEntry<CharacterItem>
{
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

    [Header("Equipment Info Format")]
    [Tooltip("Require Class Format => {0} = {Class title}")]
    public string requireClassFormat = "Require Class: {0}";
    [Tooltip("Require Attribute Format => {0} = {Attribute title}, {1} = {Amount}")]
    public string requireAttributeFormat = "Require {0}: {1}";

    [Header("Weapon Damage Format")]
    [Tooltip("Damage Format => {0} = {Damage title}, {1} = {Min damage}, {2} = {Max damage}")]
    public string damageFormat = "{0}: {1}~{2}";
    public string defaultDamageTitle = "Damage";

    [Header("UI Elements")]
    public Text textTitle;
    public Text textDescription;
    public Image imageIcon;
    public Text textSellPrice;
    public Text textStack;
    public Text textWeight;
    public Text textRequireClass;
    public Text textRequireAttributes;
    public Text textDamage;
    public UICharacterStats uiCharacterStats;

    protected virtual void Update()
    {
        var itemData = data.GetItem();

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
                stackString = string.Format(stackFormat, data.amount.ToString("N0"), itemData.maxStack);
            textStack.text = stackString;
        }

        if (textWeight != null)
            textWeight.text = string.Format(weightFormat, itemData == null ? "0" : itemData.weight.ToString("N0"));

        var equipmentItem = data.GetEquipmentItem();

        if (textRequireClass != null)
        {
            if (equipmentItem == null || equipmentItem.requireClass == null)
                textRequireClass.gameObject.SetActive(false);
            else
            {
                textRequireClass.gameObject.SetActive(true);
                textRequireClass.text = string.Format(requireClassFormat, equipmentItem.requireClass.title);
            }
        }

        if (textRequireAttributes != null)
        {
            if (equipmentItem == null || equipmentItem.requireAttributes == null || equipmentItem.requireAttributes.Length == 0)
                textRequireAttributes.gameObject.SetActive(false);
            else
            {
                var requireAttributes = equipmentItem.requireAttributes;
                var requireAttributesText = "";
                foreach (var requireAttribute in requireAttributes)
                {
                    if (requireAttribute == null || requireAttribute.attribute == null || requireAttribute.amount <= 0)
                        continue;
                    requireAttributesText += string.Format(requireAttributeFormat, requireAttribute.attribute.title, requireAttribute.amount) + "\n";
                }
                textRequireAttributes.gameObject.SetActive(!string.IsNullOrEmpty(requireAttributesText));
                textRequireAttributes.text = requireAttributesText;
            }
        }

        var stats = data.GetStats();
        if (uiCharacterStats != null)
            uiCharacterStats.data = stats;

        var weaponItem = data.GetWeaponItem();

        if (textDamage != null)
        {
            var damageElementAmountPairs = data.GetAdditionalDamageAttributes();
            if (weaponItem == null || damageElementAmountPairs.Count == 0)
                textDamage.gameObject.SetActive(false);
            else
            {
                var damagesText = "";
                foreach (var damageElementAmountPair in damageElementAmountPairs)
                {
                    var element = damageElementAmountPair.Key;
                    var amount = damageElementAmountPair.Value;
                    damagesText += string.Format(damageFormat,
                        element.title,
                        amount.minDamage,
                        amount.maxDamage) + "\n";
                }
                textDamage.gameObject.SetActive(!string.IsNullOrEmpty(damagesText));
                textDamage.text = damagesText;
            }
        }
    }
}

[System.Serializable]
public class UICharacterItemEvent : UnityEvent<UICharacterItem> { }
