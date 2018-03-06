using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterAttributeLevel : UISelectionEntry<CharacterAttributeLevel>
{
    public CharacterAttribute attribute;

    [Header("Generic Info Format")]
    [Tooltip("Title Format => {0} = {Title}")]
    public string titleFormat = "{0}";
    [Tooltip("Description Format => {0} = {Description}")]
    public string descriptionFormat = "{0}";
    [Tooltip("Level Format => {0} = {Level}")]
    public string levelFormat = "{0}";

    [Header("UI Elements")]
    public Text textTitle;
    public Text textDescription;
    public Text textLevel;
    public Image imageIcon;
    public UICharacterStats uiCharacterStats;
    public UICharacterStatsPercentage uiCharacterStatsPercentage;

    private void Update()
    {
        var attributeData = data.GetAttribute();

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, attributeData == null ? "Unknow" : attributeData.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, attributeData == null ? "N/A" : attributeData.description);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, data == null ? "N/A" : data.level.ToString("N0"));

        if (imageIcon != null)
            imageIcon.sprite = attributeData == null ? null : attributeData.icon;

        var stats = data.GetStats();
        if (uiCharacterStats != null)
            uiCharacterStats.data = stats;

        var statsPercentage = data.GetStatsPercentage();
        if (uiCharacterStatsPercentage != null)
            uiCharacterStatsPercentage.data = statsPercentage;
    }

    public void OnClickAdd()
    {
        var uiSceneGameplay = UISceneGameplay.Singleton;
        if (uiSceneGameplay != null)
            uiSceneGameplay.OwningCharacterEntity.AddAttributeLevel(uiSceneGameplay.OwningCharacterEntity.attributeLevels.IndexOf(data));
    }
}
