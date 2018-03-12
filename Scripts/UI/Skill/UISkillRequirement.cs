using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillRequirement : UISelectionEntry<KeyValuePair<Skill, int>>
{
    [Header("Requirement Format")]
    [Tooltip("Require Level Format => {0} = {Level}")]
    public string requireLevelFormat = "Require Level: {0}";

    [Header("UI Elements")]
    public Text textRequireLevel;
    public UISkillLevels uiRequireSkillLevels;

    protected override void UpdateData()
    {
        var skill = Data.Key;
        var level = Data.Value;

        if (textRequireLevel != null)
        {
            if (skill == null)
                textRequireLevel.gameObject.SetActive(false);
            else
            {
                textRequireLevel.text = string.Format(requireLevelFormat, skill.GetRequireCharacterLevel(level).ToString("N0"));
                textRequireLevel.gameObject.SetActive(true);
            }
        }

        if (uiRequireSkillLevels != null)
        {
            if (skill == null)
                uiRequireSkillLevels.gameObject.SetActive(false);
            else
            {
                uiRequireSkillLevels.Data = skill.CacheRequireSkillLevels;
                uiRequireSkillLevels.gameObject.SetActive(true);
            }
        }
    }
}
