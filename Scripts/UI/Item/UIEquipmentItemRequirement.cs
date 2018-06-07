using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEquipmentItemRequirement : UISelectionEntry<Item>
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
        var equipmentItem = Data;

        if (textRequireLevel != null)
        {
            if (equipmentItem == null || equipmentItem.requirement.level <= 0)
                textRequireLevel.gameObject.SetActive(false);
            else
            {
                textRequireLevel.gameObject.SetActive(true);
                textRequireLevel.text = string.Format(requireLevelFormat, equipmentItem.requirement.level.ToString("N0"));
            }
        }

        if (textRequireClass != null)
        {
            if (equipmentItem == null || equipmentItem.requirement.character == null)
                textRequireClass.gameObject.SetActive(false);
            else
            {
                textRequireClass.gameObject.SetActive(true);
                textRequireClass.text = string.Format(requireClassFormat, equipmentItem.requirement.character.title);
            }
        }

        if (uiRequireAttributeAmounts != null)
        {
            if (equipmentItem == null)
                uiRequireAttributeAmounts.Hide();
            else
            {
                uiRequireAttributeAmounts.Show();
                uiRequireAttributeAmounts.Data = equipmentItem.CacheRequireAttributeAmounts;
            }
        }
    }
}
