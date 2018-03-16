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

    private Dictionary<Skill, Text> cacheTextLevels;
    public Dictionary<Skill, Text> CacheTextLevels
    {
        get
        {
            if (cacheTextLevels == null)
            {
                cacheTextLevels = new Dictionary<Skill, Text>();
                foreach (var textLevel in textLevels)
                {
                    if (textLevel.skill == null || textLevel.text == null)
                        continue;
                    var key = textLevel.skill;
                    var textComp = textLevel.text;
                    textComp.text = string.Format(levelFormat, key.title, "0", "0");
                    cacheTextLevels[key] = textComp;
                }
            }
            return cacheTextLevels;
        }
    }

    protected override void UpdateData()
    {
        if (Data == null || Data.Count == 0)
        {
            if (textAllLevels != null)
                textAllLevels.gameObject.SetActive(false);

            foreach (var textLevel in CacheTextLevels)
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
                if (!string.IsNullOrEmpty(text))
                    text += "\n";
                var amountText = string.Format(levelFormat, dataEntry.Key.title, dataEntry.Value.ToString("N0"));
                text += amountText;
                Text cacheTextAmount;
                if (CacheTextLevels.TryGetValue(dataEntry.Key, out cacheTextAmount))
                    cacheTextAmount.text = amountText;
            }
            if (textAllLevels != null)
            {
                textAllLevels.gameObject.SetActive(!string.IsNullOrEmpty(text));
                textAllLevels.text = text;
            }
        }
    }
}
