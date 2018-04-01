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
    [Tooltip("Armor Stats Format => {0} = {Amount}")]
    public string armorStatsFormat = "Armor: {0}";
    [Tooltip("Accuracy Stats Format => {0} = {Amount}")]
    public string accuracyStatsFormat = "Acc: {0}";
    [Tooltip("Evasion Format => {0} = {Amount}")]
    public string evasionStatsFormat = "Eva: {0}";
    [Tooltip("Cri Rate Stats Format => {0} = {Amount}")]
    public string criRateStatsFormat = "Critical: {0}%";
    [Tooltip("Cri Dmg Rate Stats Format => {0} = {Amount}")]
    public string criDmgRateStatsFormat = "Cri Dmg: {0}%";
    [Tooltip("Block Rate Stats Format => {0} = {Amount}")]
    public string blockRateStatsFormat = "Block: {0}%";
    [Tooltip("Block Dmg Rate Stats Format => {0} = {Amount}")]
    public string blockDmgRateStatsFormat = "Block Dmg: {0}%";
    [Tooltip("Weight Limit Stats Format => {0} = {Weight Limit}")]
    public string weightLimitStatsFormat = "Weight Limit: {0}";

    [Header("UI Elements")]
    public Text textStats;
    public Text textHp;
    public Text textMp;
    public Text textArmor;
    public Text textAccuracy;
    public Text textEvasion;
    public Text textCriRate;
    public Text textCriDmgRate;
    public Text textBlockRate;
    public Text textBlockDmgRate;
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

        // Armor
        if (!string.IsNullOrEmpty(statsString))
            statsString += "\n";
        statsStringPart = string.Format(armorStatsFormat, Data.armor.ToString("N0"));
        if (Data.armor != 0)
            statsString += statsStringPart;
        if (textArmor != null)
            textArmor.text = statsStringPart;

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

        // Cri Rate
        if (!string.IsNullOrEmpty(statsString))
            statsString += "\n";
        statsStringPart = string.Format(criRateStatsFormat, (Data.criRate * 100).ToString("N2"));
        if (Data.criRate != 0)
            statsString += statsStringPart;
        if (textCriRate != null)
            textCriRate.text = statsStringPart;

        // Cri Dmg Rate
        if (!string.IsNullOrEmpty(statsString))
            statsString += "\n";
        statsStringPart = string.Format(criDmgRateStatsFormat, (Data.criDmgRate * 100).ToString("N2"));
        if (Data.criDmgRate != 0)
            statsString += statsStringPart;
        if (textCriDmgRate != null)
            textCriDmgRate.text = statsStringPart;
        
        // Block Rate
        if (!string.IsNullOrEmpty(statsString))
            statsString += "\n";
        statsStringPart = string.Format(blockRateStatsFormat, (Data.blockRate * 100).ToString("N2"));
        if (Data.blockRate != 0)
            statsString += statsStringPart;
        if (textBlockRate != null)
            textBlockRate.text = statsStringPart;

        // Block Dmg Rate
        if (!string.IsNullOrEmpty(statsString))
            statsString += "\n";
        statsStringPart = string.Format(blockDmgRateStatsFormat, (Data.blockDmgRate * 100).ToString("N2"));
        if (Data.blockDmgRate != 0)
            statsString += statsStringPart;
        if (textBlockDmgRate != null)
            textBlockDmgRate.text = statsStringPart;

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
