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
        if (textStats != null)
        {
            var statsString = "";
            if (Data.hp != 0)
                statsString += string.Format(hpStatsFormat, Data.hp) + "\n";
            if (Data.mp != 0)
                statsString += string.Format(mpStatsFormat, Data.mp) + "\n";
            if (Data.accuracy != 0)
                statsString += string.Format(accuracyStatsFormat, Data.accuracy) + "\n";
            if (Data.evasion != 0)
                statsString += string.Format(evasionStatsFormat, Data.evasion) + "\n";
            if (Data.criHitRate != 0)
                statsString += string.Format(criHitRateStatsFormat, (Data.criHitRate * 100).ToString("N2")) + "\n";
            if (Data.criDmgRate != 0)
                statsString += string.Format(criDmgRateStatsFormat, (Data.criDmgRate * 100).ToString("N2")) + "\n";
            if (Data.weightLimit != 0)
                statsString += string.Format(weightLimitStatsFormat, Data.weightLimit.ToString("N2")) + "\n";
            textStats.gameObject.SetActive(!string.IsNullOrEmpty(statsString));
            textStats.text = statsString;
        }
    }
}
