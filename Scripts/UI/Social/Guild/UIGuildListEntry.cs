using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildListEntry : UISelectionEntry<GuildListEntry>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Guild Name}")]
        public UILocaleKeySetting formatKeyGuildName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);

        [Header("UI Elements")]
        public TextWrapper textGuildName;
        public TextWrapper textLevel;

        protected override void UpdateData()
        {
            if (textGuildName != null)
            {
                textGuildName.text = string.Format(
                    LanguageManager.GetText(formatKeyGuildName),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.GuildName);
            }

            if (textLevel != null)
            {
                textLevel.text = string.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    Data == null ? 0.ToString("N0") : Data.Level.ToString("N0"));
            }
        }
    }
}
