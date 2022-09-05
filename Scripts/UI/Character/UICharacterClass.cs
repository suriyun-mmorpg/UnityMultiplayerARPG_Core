using Cysharp.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterClass : UISelectionEntry<BaseCharacter>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Description}")]
        public UILocaleKeySetting formatKeyDescription = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public Image imageIcon;
        public UICharacterStats uiStats;
        public UIAttributeAmounts uiAttributes;
        public UIResistanceAmounts uiResistances;
        public UISkillLevels uiSkills;

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
            {
                uiTextTitle.text = ZString.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.Title);
            }

            if (uiTextDescription != null)
            {
                uiTextDescription.text = ZString.Format(
                    LanguageManager.GetText(formatKeyDescription),
                    Data == null ? LanguageManager.GetUnknowDescription() : Data.Description);
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = Data == null ? null : Data.Icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
                imageIcon.preserveAspect = true;
            }

            if (uiStats != null)
            {
                uiStats.displayType = UICharacterStats.DisplayType.Simple;
                uiStats.isBonus = false;
                uiStats.Data = Data.GetCharacterStats(1);
            }

            if (uiAttributes != null)
            {
                uiAttributes.displayType = UIAttributeAmounts.DisplayType.Simple;
                uiAttributes.isBonus = false;
                uiAttributes.Data = Data.GetCharacterAttributes(1);
            }

            if (uiResistances != null)
            {
                uiResistances.isBonus = false;
                uiResistances.Data = Data.GetCharacterResistances(1);
            }

            if (uiSkills != null)
            {
                uiSkills.displayType = UISkillLevels.DisplayType.Simple;
                uiSkills.isBonus = false;
                uiSkills.Data = Data.CacheSkillLevels;
            }
        }
    }
}
