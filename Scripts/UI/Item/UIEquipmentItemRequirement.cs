using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UIEquipmentItemRequirement : UISelectionEntry<IEquipmentItem>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Require Level}")]
        public UILocaleKeySetting formatKeyRequireLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_LEVEL);
        [Tooltip("Format => {0} = {Require Class Title}")]
        [FormerlySerializedAs("formatKeyRequireClasses")]
        public UILocaleKeySetting formatKeyRequireClasses = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_CLASS);

        [Header("UI Elements")]
        public TextWrapper uiTextRequireLevel;
        [FormerlySerializedAs("uiTextRequireClass")]
        public TextWrapper uiTextRequireClasses;
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

            if (uiTextRequireClasses != null)
            {
                if (Data == null || !Data.Requirement.HasAvailableClasses())
                {
                    // Hide require class label when require character is null
                    uiTextRequireClasses.SetGameObjectActive(false);
                }
                else
                {
                    StringBuilder str = new StringBuilder();
                    if (Data.Requirement.availableClass != null)
                    {
                        str.Append(Data.Requirement.availableClass.Title);
                    }
                    if (Data.Requirement.availableClasses != null &&
                        Data.Requirement.availableClasses.Length > 0)
                    {
                        foreach (PlayerCharacter characterClass in Data.Requirement.availableClasses)
                        {
                            if (characterClass == null)
                                continue;
                            if (str.Length > 0)
                                str.Append('/');
                            str.Append(characterClass.Title);
                        }
                    }
                    uiTextRequireClasses.SetGameObjectActive(true);
                    uiTextRequireClasses.text = string.Format(
                        LanguageManager.GetText(formatKeyRequireClasses),
                        str.ToString());
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
