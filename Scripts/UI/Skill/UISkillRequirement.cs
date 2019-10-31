using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UISkillRequirement : UISelectionEntry<UICharacterSkillData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyRequireLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_LEVEL);

        [Header("UI Elements")]
        public TextWrapper uiTextRequireLevel;
        public UIAttributeAmounts uiRequireAttributeAmounts;
        public UISkillLevels uiRequireSkillLevels;

        protected override void UpdateData()
        {
            BaseSkill skill = Data.characterSkill.GetSkill();
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
                    uiRequireAttributeAmounts.displayType = UIAttributeAmounts.DisplayType.Requirement;
                    uiRequireAttributeAmounts.isBonus = false;
                    uiRequireAttributeAmounts.Show();
                    uiRequireAttributeAmounts.Data = skill.CacheRequireAttributeAmounts;
                }
            }

            if (uiRequireSkillLevels != null)
            {
                if (skill == null)
                {
                    uiRequireSkillLevels.Hide();
                }
                else
                {
                    uiRequireSkillLevels.displayType = UISkillLevels.DisplayType.Requirement;
                    uiRequireSkillLevels.isBonus = false;
                    uiRequireSkillLevels.Show();
                    uiRequireSkillLevels.Data = skill.CacheRequireSkillLevels;
                }
            }
        }
    }
}
