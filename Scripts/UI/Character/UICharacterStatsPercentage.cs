using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterStatsPercentage : UISelectionEntry<CharacterStatsPercentage>
{
    [Header("Format")]
    [Tooltip("Hp Stats Percentage Format => {0} = {Amount}")]
    public string hpStatsPercentageFormat = "Hp: {0}%";
    [Tooltip("Mp Stats Percentage Format => {0} = {Amount}")]
    public string mpStatsPercentageFormat = "Mp: {0}%";
    [Tooltip("Atk Rate Stats Percentage Format => {0} = {Amount}")]
    public string atkRateStatsPercentageFormat = "Atk Rate: {0}%";
    [Tooltip("Def Stats Percentage Format => {0} = {Amount}")]
    public string defStatsPercentageFormat = "Def: {0}%";

    [Header("UI Elements")]
    public Text textStatsPercentage;

    private void Update()
    {
        if (textStatsPercentage != null)
        {
            var statsPercentageString = "";
            if (data.hp != 0)
                statsPercentageString += string.Format(hpStatsPercentageFormat, data.hp) + "\n";
            if (data.mp != 0)
                statsPercentageString += string.Format(mpStatsPercentageFormat, data.mp) + "\n";
            if (data.atkRate != 0)
                statsPercentageString += string.Format(atkRateStatsPercentageFormat, data.atkRate) + "\n";
            if (data.def != 0)
                statsPercentageString += string.Format(defStatsPercentageFormat, data.def) + "\n";
            textStatsPercentage.gameObject.SetActive(!string.IsNullOrEmpty(statsPercentageString));
            textStatsPercentage.text = statsPercentageString;
        }
    }
}
