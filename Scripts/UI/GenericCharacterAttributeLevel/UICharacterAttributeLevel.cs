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

    [Header("Attribute Stats Effectiveness Format")]
    [Tooltip("Hp Stats Format => {0} = {Amount}")]
    public string hpStatsFormat = "Hp: {0}";
    [Tooltip("Mp Stats Format => {0} = {Amount}")]
    public string mpStatsFormat = "Mp: {0}";
    [Tooltip("Atk Rate Stats Format => {0} = {Amount}")]
    public string atkRateStatsFormat = "Atk Rate: {0}";
    [Tooltip("Def Stats Format => {0} = {Amount}")]
    public string defStatsFormat = "Def: {0}";
    [Tooltip("Cri Hit Rate Stats Format => {0} = {Amount}")]
    public string criHitRateStatsFormat = "Cri Hit: {0}";
    [Tooltip("Cri Dmg Rate Stats Format => {0} = {Amount}")]
    public string criDmgRateStatsFormat = "Cri Dmg: {0}";

    [Header("Attribute Stats Effectiveness Percentage Format")]
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

    [Header("UI Elements")]
    public Text textTitle;
    public Text textDescription;
    public Text textLevel;
    public Image imageIcon;
    public Text textStats;
    public Text textStatsPercentage;

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
                statsString += string.Format(criHitRateStatsFormat, stats.criHitRate) + "\n";
            if (stats.criDmgRate != 0)
                statsString += string.Format(criDmgRateStatsFormat, stats.criDmgRate) + "\n";
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
                statsPercentageString += string.Format(criHitRateStatsPercentageFormat, statsPercentage.criHitRate) + "\n";
            if (statsPercentage.criDmgRate != 0)
                statsPercentageString += string.Format(criDmgRateStatsPercentageFormat, statsPercentage.criDmgRate) + "\n";
            textStatsPercentage.gameObject.SetActive(!string.IsNullOrEmpty(statsPercentageString));
            textStatsPercentage.text = statsPercentageString;
        }
    }

    public void OnClickAdd()
    {
        var uiSceneGameplay = UISceneGameplay.Singleton;
        if (uiSceneGameplay != null)
            uiSceneGameplay.OwningCharacterEntity.AddAttributeLevel(uiSceneGameplay.OwningCharacterEntity.attributeLevels.IndexOf(data));
    }
}
