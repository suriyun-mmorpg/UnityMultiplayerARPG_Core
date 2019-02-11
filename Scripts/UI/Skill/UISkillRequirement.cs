using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UISkillRequirement : UISelectionEntry<CharacterSkillTuple>
    {
        [Header("Requirement Format")]
        [Tooltip("Require Level Format => {0} = {Level}")]
        public string requireLevelFormat = "Require Level: {0}";

        [Header("UI Elements")]
        public TextWrapper uiTextRequireLevel;
        public UIAttributeAmounts uiRequireAttributeAmounts;
        public UISkillLevels uiRequireSkillLevels;

        protected override void UpdateData()
        {
            Skill skill = Data.characterSkill.GetSkill();
            short level = Data.targetLevel;

            if (uiTextRequireLevel != null)
            {
                if (skill == null)
                    uiTextRequireLevel.gameObject.SetActive(false);
                else
                {
                    uiTextRequireLevel.gameObject.SetActive(true);
                    uiTextRequireLevel.text = string.Format(requireLevelFormat, skill.GetRequireCharacterLevel(level).ToString("N0"));
                }
            }

            if (uiRequireAttributeAmounts != null)
            {
                if (skill == null)
                    uiRequireAttributeAmounts.Hide();
                else
                {
                    uiRequireAttributeAmounts.Show();
                    uiRequireAttributeAmounts.Data = skill.CacheRequireAttributeAmounts;
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
}
