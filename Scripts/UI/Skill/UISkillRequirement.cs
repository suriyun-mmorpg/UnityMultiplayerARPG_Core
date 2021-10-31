using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UISkillRequirement : UISelectionEntry<UICharacterSkillData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Require Level}")]
        public UILocaleKeySetting formatKeyRequireLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_LEVEL);
        [Tooltip("Format => {0} = {Current Level}, {1} = {Require Level}")]
        public UILocaleKeySetting formatKeyRequireLevelNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_LEVEL_NOT_ENOUGH);

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
                if (skill == null || skill.GetRequireCharacterLevel(level) <= 0)
                {
                    // Hide require level label when require level <= 0
                    uiTextRequireLevel.SetGameObjectActive(false);
                }
                else
                {
                    uiTextRequireLevel.SetGameObjectActive(true);
                    short characterLevel = (short)(GameInstance.PlayingCharacter != null ? GameInstance.PlayingCharacter.Level : 1);
                    short requireCharacterLevel = skill.GetRequireCharacterLevel(level);
                    if (characterLevel >= requireCharacterLevel)
                    {
                        uiTextRequireLevel.text = string.Format(
                            LanguageManager.GetText(formatKeyRequireLevel),
                            requireCharacterLevel.ToString("N0"));
                    }
                    else
                    {
                        uiTextRequireLevel.text = string.Format(
                            LanguageManager.GetText(formatKeyRequireLevelNotEnough),
                            characterLevel,
                            requireCharacterLevel.ToString("N0"));
                    }
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
                    uiRequireAttributeAmounts.includeEquipmentsForCurrentAmounts = false;
                    uiRequireAttributeAmounts.includeBuffsForCurrentAmounts = false;
                    uiRequireAttributeAmounts.includeSkillsForCurrentAmounts = true;
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
                    uiRequireSkillLevels.includeEquipmentsForCurrentLevels = false;
                    uiRequireSkillLevels.isBonus = false;
                    uiRequireSkillLevels.Show();
                    uiRequireSkillLevels.Data = skill.CacheRequireSkillLevels;
                }
            }
        }
    }
}
