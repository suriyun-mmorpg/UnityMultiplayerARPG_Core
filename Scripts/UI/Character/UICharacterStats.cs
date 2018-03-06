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
    [Tooltip("Atk Rate Stats Format => {0} = {Amount}")]
    public string atkRateStatsFormat = "Atk Rate: {0}";
    [Tooltip("Def Stats Format => {0} = {Amount}")]
    public string defStatsFormat = "Def: {0}";
    [Tooltip("Cri Hit Rate Stats Format => {0} = {Amount}")]
    public string criHitRateStatsFormat = "Cri Hit: {0}%";
    [Tooltip("Cri Dmg Rate Stats Format => {0} = {Amount}")]
    public string criDmgRateStatsFormat = "Cri Dmg: {0}%";
    [Tooltip("Weight Limit Stats Format => {0} = {Weight Limit}")]
    public string weightLimitStatsFormat = "Weight Limit: {0}";

    [Header("UI Elements")]
    public Text textStats;

    private void Update()
    {
        if (textStats != null)
        {
            var statsString = "";
            if (data.hp != 0)
                statsString += string.Format(hpStatsFormat, data.hp) + "\n";
            if (data.mp != 0)
                statsString += string.Format(mpStatsFormat, data.mp) + "\n";
            if (data.atkRate != 0)
                statsString += string.Format(atkRateStatsFormat, data.atkRate) + "\n";
            if (data.def != 0)
                statsString += string.Format(defStatsFormat, data.def) + "\n";
            if (data.criHitRate != 0)
                statsString += string.Format(criHitRateStatsFormat, (data.criHitRate * 100).ToString("N2")) + "\n";
            if (data.criDmgRate != 0)
                statsString += string.Format(criDmgRateStatsFormat, (data.criDmgRate * 100).ToString("N2")) + "\n";
            if (data.weightLimit != 0)
                statsString += string.Format(weightLimitStatsFormat, data.weightLimit.ToString("N2")) + "\n";
            textStats.gameObject.SetActive(!string.IsNullOrEmpty(statsString));
            textStats.text = statsString;
        }
    }
}
