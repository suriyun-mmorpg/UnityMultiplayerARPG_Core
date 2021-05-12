using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIEquipmentItemRequirement : UISelectionEntry<IEquipmentItem>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Require Level}")]
        public UILocaleKeySetting formatKeyRequireLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_LEVEL);
        [Tooltip("Format => {0} = {Require Class Title}")]
        public UILocaleKeySetting formatKeyRequireClass = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_CLASS);

        [Header("UI Elements")]
        public TextWrapper uiTextRequireLevel;
        public TextWrapper uiTextRequireClass;
        public UIAttributeAmounts uiRequireAttributeAmounts;

        protected override void UpdateData()
        {
            if (uiTextRequireLevel != null)
            {
                if (Data == null || Data.Requirement.level <= 0)
                {
                    // Hide require level label when require level <= 0
                    uiTextRequireLevel.SetGameObjectActive(false);
                }
                else
                {
                    uiTextRequireLevel.SetGameObjectActive(true);
                    uiTextRequireLevel.text = string.Format(
                        LanguageManager.GetText(formatKeyRequireLevel),
                        Data.Requirement.level.ToString("N0"));
                }
            }

            if (uiTextRequireClass != null)
            {
                if (Data == null || Data.Requirement.character == null)
                {
                    // Hide require class label when require character is null
                    uiTextRequireClass.SetGameObjectActive(false);
                }
                else
                {
                    uiTextRequireClass.SetGameObjectActive(true);
                    uiTextRequireClass.text = string.Format(
                        LanguageManager.GetText(formatKeyRequireClass),
                        Data.Requirement.character.Title);
                }
            }

            if (uiRequireAttributeAmounts != null)
            {
                if (Data == null)
                {
                    // Hide attribute amounts when item data is empty
                    uiRequireAttributeAmounts.Hide();
                }
                else
                {
                    uiRequireAttributeAmounts.displayType = UIAttributeAmounts.DisplayType.Requirement;
                    uiRequireAttributeAmounts.includeEquipmentsForCurrentAmounts = true;
                    uiRequireAttributeAmounts.includeBuffsForCurrentAmounts = false;
                    uiRequireAttributeAmounts.includeSkillsForCurrentAmounts = true;
                    uiRequireAttributeAmounts.isBonus = false;
                    uiRequireAttributeAmounts.Show();
                    uiRequireAttributeAmounts.Data = Data.RequireAttributeAmounts;
                }
            }
        }
    }
}
