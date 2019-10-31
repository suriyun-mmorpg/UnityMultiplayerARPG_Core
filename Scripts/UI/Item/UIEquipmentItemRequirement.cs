using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIEquipmentItemRequirement : UISelectionEntry<Item>
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
            Item equipmentItem = Data;

            if (uiTextRequireLevel != null)
            {
                if (equipmentItem == null || equipmentItem.requirement.level <= 0)
                {
                    // Hide require level label when require level <= 0
                    uiTextRequireLevel.gameObject.SetActive(false);
                }
                else
                {
                    uiTextRequireLevel.gameObject.SetActive(true);
                    uiTextRequireLevel.text = string.Format(
                        LanguageManager.GetText(formatKeyRequireLevel),
                        equipmentItem.requirement.level.ToString("N0"));
                }
            }

            if (uiTextRequireClass != null)
            {
                if (equipmentItem == null || equipmentItem.requirement.character == null)
                {
                    // Hide require class label when require character is null
                    uiTextRequireClass.gameObject.SetActive(false);
                }
                else
                {
                    uiTextRequireClass.gameObject.SetActive(true);
                    uiTextRequireClass.text = string.Format(
                        LanguageManager.GetText(formatKeyRequireClass),
                        equipmentItem.requirement.character.Title);
                }
            }

            if (uiRequireAttributeAmounts != null)
            {
                if (equipmentItem == null)
                {
                    // Hide attribute amounts when item data is empty
                    uiRequireAttributeAmounts.Hide();
                }
                else
                {
                    uiRequireAttributeAmounts.displayType = UIAttributeAmounts.DisplayType.Requirement;
                    uiRequireAttributeAmounts.isBonus = false;
                    uiRequireAttributeAmounts.Show();
                    uiRequireAttributeAmounts.Data = equipmentItem.CacheRequireAttributeAmounts;
                }
            }
        }
    }
}
