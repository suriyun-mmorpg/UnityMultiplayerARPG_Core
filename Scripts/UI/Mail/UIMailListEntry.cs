using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIMailListEntry : UISelectionEntry<MailListEntry>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Sender Name}")]
        public UILocaleKeySetting formatSenderName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MAIL_SENDER_NAME);
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MAIL_TITLE);
        [Tooltip("Format => {0} = {Sent Date}")]
        public UILocaleKeySetting formatSentDate = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MAIL_SENT_DATE);

        [Header("UI Elements")]
        public TextWrapper textSenderName;
        public TextWrapper textTitle;
        public TextWrapper textSentDate;
        public GameObject[] readObjects;
        public GameObject[] unreadObjects;

        protected override void UpdateData()
        {
            if (textSenderName != null)
            {
                textSenderName.text = ZString.Format(
                    LanguageManager.GetText(formatSenderName),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.SenderName);
            }

            if (textTitle != null)
            {
                textTitle.text = ZString.Format(
                    LanguageManager.GetText(formatTitle),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.Title);
            }

            if (textSentDate != null)
            {
                System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                if (Data != null)
                    dateTime = dateTime.AddSeconds(Data.SentTimestamp).ToLocalTime();
                textSentDate.text = ZString.Format(
                    LanguageManager.GetText(formatSentDate),
                    (System.DateTime.Now - new System.DateTime(dateTime.Ticks)).GetPrettyDate());
            }

            if (readObjects != null && readObjects.Length > 0)
            {
                for (int i = 0; i < readObjects.Length; ++i)
                {
                    readObjects[i].SetActive(Data != null && Data.IsRead);
                }
            }

            if (unreadObjects != null && unreadObjects.Length > 0)
            {
                for (int i = 0; i < unreadObjects.Length; ++i)
                {
                    unreadObjects[i].SetActive(Data == null || !Data.IsRead);
                }
            }
        }
    }
}
