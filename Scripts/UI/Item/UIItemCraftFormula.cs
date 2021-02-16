using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIItemCraftFormula : UISelectionEntry<ItemCraftFormula>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Craft Duration}")]
        public UILocaleKeySetting formatKeyCraftDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRAFT_DURATION);
        [Tooltip("Format => {0} = {Craft Remains Duration}")]
        public UILocaleKeySetting formatKeyCraftRemainsDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public Image imageIcon;
        public TextWrapper uiTextDuration;
        public UIItemCraft uiCraftItem;

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
            {
                uiTextTitle.text = string.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.Title);
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = Data == null ? null : Data.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (uiTextDuration != null)
            {
                uiTextDuration.text = string.Format(
                    LanguageManager.GetText(formatKeyCraftDuration),
                    Data.CraftDuration.ToString("N0"));
            }

            if (uiCraftItem != null)
            {
                if (Data == null)
                {
                    uiCraftItem.Hide();
                }
                else
                {
                    uiCraftItem.Show();
                    uiCraftItem.Data = Data.ItemCraft;
                }
            }
        }
    }
}
