using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillLevels : UISelectionEntry<Dictionary<Skill, short>>
{
    [Tooltip("Skill Level Format => {0} = {Skill title}, {1} = {Current Level}, {2} = {Target Level}")]
    public string levelFormat = "{0}: {1}/{2}";
    [Tooltip("Skill Level Format => {0} = {Skill title}, {1} = {Current Level}, {2} = {Target Level}")]
    public string levelNotReachTargetFormat = "{0}: <color=red>{1}/{2}</color>";

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
        var owningCharacter = BasePlayerCharacterController.OwningCharacter;
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
                var skill = dataEntry.Key;
                var targetLevel = dataEntry.Value;
                if (skill == null || targetLevel == 0)
                    continue;
                if (!string.IsNullOrEmpty(text))
                    text += "\n";
                short currentLevel = 0;
                if (owningCharacter != null)
                    owningCharacter.CacheSkills.TryGetValue(skill, out currentLevel);
                var format = currentLevel >= targetLevel ? levelFormat : levelNotReachTargetFormat;
                var amountText = string.Format(format, skill.title, currentLevel.ToString("N0"), targetLevel.ToString("N0"));
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
