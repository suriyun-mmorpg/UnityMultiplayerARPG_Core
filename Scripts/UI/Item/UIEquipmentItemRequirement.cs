using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIEquipmentItemRequirement : UISelectionEntry<Item>
    {
        /// <summary>
        /// Format => {0} = {Require Level Label}, {1} = {Level}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Require Level Label}, {1} = {Level}")]
        public string formatRequireLevel = "{0}: {1}";
        /// <summary>
        /// Format => {0} = {Require Class Label}, {1} = {Class Title}
        /// </summary>
        [Tooltip("Format => {0} = {Require Class Label}, {1} = {Class Title}")]
        public string formatRequireClass = "{0}: {1}";

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
                        formatRequireLevel,
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REQUIRE_LEVEL.ToString()),
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
                        formatRequireClass,
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REQUIRE_CLASS.ToString()),
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
                    uiRequireAttributeAmounts.showAsRequirement = true;
                    uiRequireAttributeAmounts.Show();
                    uiRequireAttributeAmounts.Data = equipmentItem.CacheRequireAttributeAmounts;
                }
            }
        }
    }
}
