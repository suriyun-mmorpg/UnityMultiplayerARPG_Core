using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIEquipmentItemRequirement : UISelectionEntry<Item>
    {
        [Header("Requirement Format")]
        [Tooltip("Require Level Format => {0} = {Level}, {1} = {Require Level Label}")]
        public string requireLevelFormat = "{1}: {0}";
        [Tooltip("Require Class Format => {0} = {Class title}, {1} = {Require Class Label}")]
        public string requireClassFormat = "{1}: {0}";

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
                        requireLevelFormat,
                        equipmentItem.requirement.level.ToString("N0"),
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REQUIRE_LEVEL.ToString()));
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
                        requireClassFormat,
                        equipmentItem.requirement.character.Title,
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REQUIRE_CLASS.ToString()));
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
                    uiRequireAttributeAmounts.showAsRequirement = true;
                    uiRequireAttributeAmounts.Show();
                    uiRequireAttributeAmounts.Data = equipmentItem.CacheRequireAttributeAmounts;
                }
            }
        }
    }
}
