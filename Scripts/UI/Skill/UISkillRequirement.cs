using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UISkillRequirement : UISelectionEntry<CharacterSkillTuple>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Level}")]
        public string formatKeyRequireLevel = UILocaleKeys.UI_FORMAT_REQUIRE_LEVEL.ToString();

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
                        LanguageManager.GetText(formatKeyRequireLevel),
                        skill.GetRequireCharacterLevel(level).ToString("N0"));
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
