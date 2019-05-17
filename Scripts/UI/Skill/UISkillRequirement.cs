using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UISkillRequirement : UISelectionEntry<CharacterSkillTuple>
    {
        [Header("Requirement Format")]
        [Tooltip("Require Level Format => {0} = {Level}, {1} = {Require Level Label}")]
        public string requireLevelFormat = "{1}: {0}";

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
                {
                    uiTextRequireLevel.gameObject.SetActive(false);
                }
                else
                {
                    uiTextRequireLevel.gameObject.SetActive(true);
                    uiTextRequireLevel.text = string.Format(
                        requireLevelFormat,
                        skill.GetRequireCharacterLevel(level).ToString("N0"),
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REQUIRE_LEVEL.ToString()));
                }
            }

            if (uiRequireAttributeAmounts != null)
            {
                if (skill == null)
                {
                    uiRequireAttributeAmounts.Hide();
                }
                else
                {
                    uiRequireAttributeAmounts.showAsRequirement = true;
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
                    uiRequireSkillLevels.showAsRequirement = true;
                    uiRequireSkillLevels.Show();
                    uiRequireSkillLevels.Data = skill.CacheRequireSkillLevels;
                }
            }
        }
    }
}
