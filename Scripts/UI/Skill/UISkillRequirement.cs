using UnityEngine;
using UnityEngine.UI;

public class UISkillRequirement : UISelectionEntry<SkillLevelTuple>
{
    [Header("Requirement Format")]
    [Tooltip("Require Level Format => {0} = {Level}")]
    public string requireLevelFormat = "Require Level: {0}";

    [Header("UI Elements")]
    public Text textRequireLevel;
    public UISkillLevels uiRequireSkillLevels;

    protected override void UpdateData()
    {
        var skill = Data.skill;
        var level = Data.targetLevel;

        if (textRequireLevel != null)
        {
            if (skill == null)
                textRequireLevel.gameObject.SetActive(false);
            else
            {
                textRequireLevel.gameObject.SetActive(true);
                textRequireLevel.text = string.Format(requireLevelFormat, skill.GetRequireCharacterLevel(level).ToString("N0"));
            }
        }

        if (uiRequireSkillLevels != null)
        {
            if (skill == null)
                uiRequireSkillLevels.Hide();
            else
            {
                uiRequireSkillLevels.Show();
                uiRequireSkillLevels.Data = skill.CacheRequireSkillLevels;
            }
        }
    }
}
