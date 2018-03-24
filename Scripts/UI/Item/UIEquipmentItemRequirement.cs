using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEquipmentItemRequirement : UISelectionEntry<KeyValuePair<Item, int>>
{
    [Header("Requirement Format")]
    [Tooltip("Require Level Format => {0} = {Level}")]
    public string requireLevelFormat = "Require Level: {0}";
    [Tooltip("Require Class Format => {0} = {Class title}")]
    public string requireClassFormat = "Require Class: {0}";

    [Header("UI Elements")]
    public Text textRequireLevel;
    public Text textRequireClass;
    public UIAttributeAmounts uiRequireAttributeAmounts;

    protected override void UpdateData()
    {
        var equipmentItem = Data.Key;

        if (textRequireLevel != null)
        {
            if (equipmentItem == null || equipmentItem.requirement.characterLevel <= 0)
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
    }
}
