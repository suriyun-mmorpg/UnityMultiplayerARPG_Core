using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterStats : UISelectionEntry<CharacterStats>
{
    [Header("Format")]
    [Tooltip("Hp Stats Format => {0} = {Amount}")]
    public string hpStatsFormat = "Hp: {0}";
    [Tooltip("Mp Stats Format => {0} = {Amount}")]
    public string mpStatsFormat = "Mp: {0}";
    [Tooltip("Accuracy Stats Format => {0} = {Amount}")]
    public string accuracyStatsFormat = "Acc: {0}";
    [Tooltip("Evasion Format => {0} = {Amount}")]
    public string evasionStatsFormat = "Eva: {0}";
    [Tooltip("Cri Hit Rate Stats Format => {0} = {Amount}")]
    public string criHitRateStatsFormat = "Cri Hit: {0}%";
    [Tooltip("Cri Dmg Rate Stats Format => {0} = {Amount}")]
    public string criDmgRateStatsFormat = "Cri Dmg: {0}%";
    [Tooltip("Weight Limit Stats Format => {0} = {Weight Limit}")]
    public string weightLimitStatsFormat = "Weight Limit: {0}";

    [Header("UI Elements")]
    public Text textStats;
    public Text textHp;
    public Text textMp;
    public Text textAccuracy;
    public Text textEvasion;
    public Text textCriHitRate;
    public Text textCriDmgRate;
    public Text textWeightLimit;

    protected override void UpdateData()
    {
        var statsString = "";
        var statsStringPart = "";
        // Hp
        if (!string.IsNullOrEmpty(statsString))
            statsString += "\n";
        statsStringPart = string.Format(hpStatsFormat, Data.hp.ToString("N0"));
        if (Data.hp != 0)
            statsString += statsStringPart;
        if (textHp != null)
            textHp.text = statsStringPart;
        // Mp
        if (!string.IsNullOrEmpty(statsString))
            statsString += "\n";
        statsStringPart = string.Format(mpStatsFormat, Data.mp.ToString("N0"));
        if (Data.mp != 0)
            statsString += statsStringPart;
        if (textMp != null)
            textMp.text = statsStringPart;
        // Accuracy
        if (!string.IsNullOrEmpty(statsString))
            statsString += "\n";
        statsStringPart = string.Format(accuracyStatsFormat, Data.accuracy.ToString("N0"));
        if (Data.accuracy != 0)
            statsString += statsStringPart;
        if (textAccuracy != null)
            textAccuracy.text = statsStringPart;
        // Evasion
        if (!string.IsNullOrEmpty(statsString))
            statsString += "\n";
        statsStringPart = string.Format(evasionStatsFormat, Data.evasion.ToString("N0"));
        if (Data.evasion != 0)
            statsString += statsStringPart;
        if (textEvasion != null)
            textEvasion.text = statsStringPart;
        // Cri Hit Rate
        if (!string.IsNullOrEmpty(statsString))
            statsString += "\n";
        statsStringPart = string.Format(criHitRateStatsFormat, (Data.criHitRate * 100).ToString("N2"));
        if (Data.criHitRate != 0)
            statsString += statsStringPart;
        if (textCriHitRate != null)
            textCriHitRate.text = statsStringPart;
        // Cri Dmg Rate
        if (!string.IsNullOrEmpty(statsString))
            statsString += "\n";
        statsStringPart = string.Format(criDmgRateStatsFormat, (Data.criDmgRate * 100).ToString("N2"));
        if (Data.criDmgRate != 0)
            statsString += statsStringPart;
        if (textCriDmgRate != null)
            textCriDmgRate.text = statsStringPart;
        // Weight
        if (!string.IsNullOrEmpty(statsString))
            statsString += "\n";
        statsStringPart = string.Format(weightLimitStatsFormat, Data.weightLimit.ToString("N2"));
        if (Data.weightLimit != 0)
            statsString += statsStringPart;
        if (textWeightLimit != null)
            textWeightLimit.text = statsStringPart;
        // All stats text
        if (textStats != null)
        {
            textStats.gameObject.SetActive(!string.IsNullOrEmpty(statsString));
            textStats.text = statsString;
        }
    }
}
