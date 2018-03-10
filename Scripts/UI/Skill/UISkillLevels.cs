using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillLevels : UISelectionEntry<Dictionary<Skill, int>>
{
    [Tooltip("Skill Level Format => {0} = {Skill title}, {1} = {Level}")]
    public string levelFormat = "{0}: {1}";

    [Header("UI Elements")]
    public Text textAllLevels;
    public UISkillTextPair[] textLevels;

    private Dictionary<Skill, Text> tempTextLevels;
    public Dictionary<Skill, Text> TempTextLevels
    {
        get
        {
            if (tempTextLevels == null)
            {
                tempTextLevels = new Dictionary<Skill, Text>();
                foreach (var textLevel in textLevels)
                {
                    if (textLevel.skill == null || textLevel.text == null)
                        continue;
                    var key = textLevel.skill;
                    var textComp = textLevel.text;
                    textComp.text = string.Format(levelFormat, key.title, "0", "0");
                    tempTextLevels[key] = textComp;
                }
            }
            return tempTextLevels;
        }
    }

    protected override void UpdateData()
    {
        if (textAllLevels != null)
        {
            if (Data == null || Data.Count == 0)
            {
                textAllLevels.gameObject.SetActive(false);
                foreach (var textLevel in TempTextLevels)
                {
                    var element = textLevel.Key;
                    textLevel.Value.text = string.Format(levelFormat, element.title, "0", "0");
                }
            }
            else
            {
                var text = "";
                foreach (var dataEntry in Data)
                {
                    if (dataEntry.Key == null || dataEntry.Value == 0)
                        continue;
                    var amountText = string.Format(levelFormat, dataEntry.Key.title, dataEntry.Value.ToString("N0"));
                    text += amountText + "\n";
                    if (TempTextLevels.ContainsKey(dataEntry.Key))
                        TempTextLevels[dataEntry.Key].text = amountText;
                }
                textAllLevels.gameObject.SetActive(!string.IsNullOrEmpty(text));
                textAllLevels.text = text;
            }
        }
    }
}
