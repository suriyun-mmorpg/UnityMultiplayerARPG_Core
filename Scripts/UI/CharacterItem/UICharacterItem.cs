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

    [Header("Equipment Info Format")]
    [Tooltip("Require Class Format => {0} = {Class title}")]
    public string requireClassFormat = "Require Class: {0}";
    [Tooltip("Require Attribute Format => {0} = {Attribute title}, {1} = {Amount}")]
    public string requireAttributeFormat = "Require {0}: {1}";

    [Header("Equipment Stats Format")]
    [Tooltip("Hp Stats Format => {0} = {Amount}")]
    public string hpStatsFormat = "Hp: {0}";
    [Tooltip("Mp Stats Format => {0} = {Amount}")]
    public string mpStatsFormat = "Mp: {0}";
    [Tooltip("Atk Rate Stats Format => {0} = {Amount}")]
    public string atkRateStatsFormat = "Atk Rate: {0}";
    [Tooltip("Def Stats Format => {0} = {Amount}")]
    public string defStatsFormat = "Def: {0}";
    [Tooltip("Cri Hit Rate Stats Format => {0} = {Amount}")]
    public string criHitRateStatsFormat = "Cri Hit: {0}%";
    [Tooltip("Cri Dmg Rate Stats Format => {0} = {Amount}")]
    public string criDmgRateStatsFormat = "Cri Dmg: {0}%";

    [Header("Equipment Stats Percentage Format")]
    [Tooltip("Hp Stats Percentage Format => {0} = {Amount}")]
    public string hpStatsPercentageFormat = "Hp: {0}%";
    [Tooltip("Mp Stats Percentage Format => {0} = {Amount}")]
    public string mpStatsPercentageFormat = "Mp: {0}%";
    [Tooltip("Atk Rate Stats Percentage Format => {0} = {Amount}")]
    public string atkRateStatsPercentageFormat = "Atk Rate: {0}%";
    [Tooltip("Def Stats Percentage Format => {0} = {Amount}")]
    public string defStatsPercentageFormat = "Def: {0}%";
    [Tooltip("Cri Hit Rate Stats Percentage Format => {0} = {Amount}")]
    public string criHitRateStatsPercentageFormat = "Cri Hit: {0}%";
    [Tooltip("Cri Dmg Rate Stats Percentage Format => {0} = {Amount}")]
    public string criDmgRateStatsPercentageFormat = "Cri Dmg: {0}%";

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
    public Text textRequireClass;
    public Text textRequireAttributes;
    public Text textStats;
    public Text textStatsPercentage;
    public Text textDamage;

    protected virtual void Update()
    {
        var itemData = data.GetItem();

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, itemData == null ? "Unknow" : itemData.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, itemData == null ? "N/A" : itemData.description);

        if (imageIcon != null)
            imageIcon.sprite = itemData == null ? null : itemData.icon;

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

        if (textStats != null)
        {
            var statsString = "";
            if (stats.hp != 0)
                statsString += string.Format(hpStatsFormat, stats.hp) + "\n";
            if (stats.mp != 0)
                statsString += string.Format(mpStatsFormat, stats.mp) + "\n";
            if (stats.atkRate != 0)
                statsString += string.Format(atkRateStatsFormat, stats.atkRate) + "\n";
            if (stats.def != 0)
                statsString += string.Format(defStatsFormat, stats.def) + "\n";
            if (stats.criHitRate != 0)
                statsString += string.Format(criHitRateStatsFormat, (stats.criHitRate * 100f).ToString("N2")) + "\n";
            if (stats.criDmgRate != 0)
                statsString += string.Format(criDmgRateStatsFormat, (stats.criDmgRate * 100f).ToString("N2")) + "\n";
            textStats.gameObject.SetActive(!string.IsNullOrEmpty(statsString));
            textStats.text = statsString;
        }

        var statsPercentage = data.GetStatsPercentage();

        if (textStatsPercentage != null)
        {
            var statsPercentageString = "";
            if (statsPercentage.hp != 0)
                statsPercentageString += string.Format(hpStatsPercentageFormat, statsPercentage.hp) + "\n";
            if (statsPercentage.mp != 0)
                statsPercentageString += string.Format(mpStatsPercentageFormat, statsPercentage.mp) + "\n";
            if (statsPercentage.atkRate != 0)
                statsPercentageString += string.Format(atkRateStatsPercentageFormat, statsPercentage.atkRate) + "\n";
            if (statsPercentage.def != 0)
                statsPercentageString += string.Format(defStatsPercentageFormat, statsPercentage.def) + "\n";
            if (statsPercentage.criHitRate != 0)
                statsPercentageString += string.Format(criHitRateStatsPercentageFormat, (statsPercentage.criHitRate * 100f).ToString("N2")) + "\n";
            if (statsPercentage.criDmgRate != 0)
                statsPercentageString += string.Format(criDmgRateStatsPercentageFormat, (statsPercentage.criDmgRate * 100f).ToString("N2")) + "\n";
            textStatsPercentage.gameObject.SetActive(!string.IsNullOrEmpty(statsPercentageString));
            textStatsPercentage.text = statsPercentageString;
        }

        var weaponItem = data.GetWeaponItem();

        if (textDamage != null)
        {
            if (weaponItem == null || weaponItem.TempDamageAmounts.Count == 0)
                textDamage.gameObject.SetActive(false);
            else
            {
                var damageAmounts = weaponItem.TempDamageAmounts.Values;
                var damagesText = "";
                foreach (var damageAmount in damageAmounts)
                {
                    damagesText += string.Format(damageFormat, 
                        damageAmount.damageElement == null ? defaultDamageTitle : damageAmount.damageElement.title, 
                        damageAmount.minDamage, 
                        damageAmount.maxDamage) + "\n";
                }
                textDamage.gameObject.SetActive(!string.IsNullOrEmpty(damagesText));
                textDamage.text = damagesText;
            }
        }
    }
}

[System.Serializable]
public class UICharacterItemEvent : UnityEvent<UICharacterItem> { }
