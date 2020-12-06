using UnityEngine;

namespace MultiplayerARPG
{
    public class UIMailListEntry : UISelectionEntry<MailListEntry>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Sender Name}")]
        public UILocaleKeySetting formatSenderName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Sent Date}")]
        public UILocaleKeySetting formatSentDate = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper textSenderName;
        public TextWrapper textTitle;
        public TextWrapper textSentDate;

        protected override void UpdateData()
        {
            if (textSenderName != null)
            {
                textSenderName.text = string.Format(
                    LanguageManager.GetText(formatSenderName),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.SenderName);
            }

            if (textTitle != null)
            {
                textTitle.text = string.Format(
                    LanguageManager.GetText(formatTitle),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.Title);
            }

            if (textSentDate != null)
            {
                System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                if (Data != null)
                    dateTime = dateTime.AddSeconds(Data.SentTimestamp);
                textSentDate.text = string.Format(
                    LanguageManager.GetText(formatSentDate),
                    dateTime.ToShortDateString());
            }
        }
    }
}
