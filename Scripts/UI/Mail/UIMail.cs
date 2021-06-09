using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIMail : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Sender Name}")]
        public UILocaleKeySetting formatSenderName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MAIL_SENDER_NAME);
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MAIL_TITLE);
        [Tooltip("Format => {0} = {Content}")]
        public UILocaleKeySetting formatContent = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MAIL_CONTENT);
        [Tooltip("Format => {0} = {Gold}")]
        public UILocaleKeySetting formatGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD);
        [Tooltip("Format => {0} = {Sent Date}")]
        public UILocaleKeySetting formatSentDate = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MAIL_SENT_DATE);

        [Header("UI Elements")]
        public TextWrapper textSenderName;
        public TextWrapper textTitle;
        public TextWrapper textContent;
        public TextWrapper textGold;
        public UICharacterCurrencies uiCurrencies;
        public UICharacterItems uiItems;
        public TextWrapper textSentDate;
        public UIMailList uiMailList;
        public GameObject[] readObjects;
        public GameObject[] unreadObjects;
        public GameObject[] claimObjects;
        public GameObject[] unclaimObjects;

        private string mailId;
        public string MailId
        {
            get { return mailId; }
            set
            {
                if (mailId != value)
                {
                    mailId = value;
                    ReadMail();
                }
            }
        }

        private void ReadMail()
        {
            UpdateData(null);
            GameInstance.ClientMailHandlers.RequestReadMail(new RequestReadMailMessage()
            {
                id = MailId,
            }, ReadMailCallback);
        }

        private void ReadMailCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseReadMailMessage response)
        {
            ClientMailActions.ResponseReadMail(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            UpdateData(response.mail);
        }

        public void OnClickClaimItems()
        {
            GameInstance.ClientMailHandlers.RequestClaimMailItems(new RequestClaimMailItemsMessage()
            {
                id = MailId
            }, ClaimMailItemsCallback);
        }

        private void ClaimMailItemsCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseClaimMailItemsMessage response)
        {
            ClientMailActions.ResponseClaimMailItems(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_SUCCESS.ToString()), LanguageManager.GetText(UITextKeys.UI_MAIL_CLAIMED.ToString()));
            Hide();
            if (uiMailList)
                uiMailList.Refresh();
        }

        public void OnClickDelete()
        {
            GameInstance.ClientMailHandlers.RequestDeleteMail(new RequestDeleteMailMessage()
            {
                id = MailId
            }, DeleteMailCallback);
        }

        private void DeleteMailCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseDeleteMailMessage response)
        {
            ClientMailActions.ResponseDeleteMail(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_SUCCESS.ToString()), LanguageManager.GetText(UITextKeys.UI_MAIL_DELETED.ToString()));
            Hide();
            if (uiMailList)
                uiMailList.Refresh();
        }

        protected virtual void UpdateData(Mail mail)
        {
            if (textSenderName != null)
            {
                textSenderName.text = string.Format(
                    LanguageManager.GetText(formatSenderName),
                    mail == null ? LanguageManager.GetUnknowTitle() : mail.SenderName);
            }

            if (textTitle != null)
            {
                textTitle.text = string.Format(
                    LanguageManager.GetText(formatTitle),
                    mail == null ? LanguageManager.GetUnknowTitle() : mail.Title);
            }

            if (textContent != null)
            {
                textContent.text = string.Format(
                    LanguageManager.GetText(formatContent),
                    mail == null ? string.Empty : mail.Content);
            }

            if (textGold != null)
            {
                textGold.text = string.Format(
                    LanguageManager.GetText(formatGold),
                    mail == null ? "0" : mail.Gold.ToString("N0"));
                textGold.gameObject.SetActive(mail != null && mail.Gold != 0);
            }

            if (uiCurrencies != null)
            {
                List<CharacterCurrency> increasingCurrencies = new List<CharacterCurrency>();
                if (mail != null)
                {
                    foreach (KeyValuePair<int, int> mailCurrency in mail.Currencies)
                    {
                        increasingCurrencies.Add(CharacterCurrency.Create(mailCurrency.Key, amount: mailCurrency.Value));
                    }
                }
                if (increasingCurrencies.Count > 0)
                {
                    uiCurrencies.UpdateData(GameInstance.PlayingCharacter, increasingCurrencies);
                    uiCurrencies.Show();
                }
                else
                {
                    uiCurrencies.Hide();
                }
            }

            if (uiItems != null)
            {
                List<CharacterItem> increasingItems = new List<CharacterItem>();
                if (mail != null)
                {
                    foreach (KeyValuePair<int, short> mailItem in mail.Items)
                    {
                        increasingItems.Add(CharacterItem.Create(mailItem.Key, amount: mailItem.Value));
                    }
                }
                if (increasingItems.Count > 0)
                {
                    uiItems.UpdateData(GameInstance.PlayingCharacter, increasingItems);
                    uiItems.Show();
                }
                else
                {
                    uiItems.Hide();
                }
            }

            if (textSentDate != null)
            {
                System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                if (mail != null)
                    dateTime = dateTime.AddSeconds(mail.SentTimestamp).ToLocalTime();
                textSentDate.text = string.Format(
                    LanguageManager.GetText(formatSentDate),
                    dateTime.GetPrettyDate());
            }

            if (readObjects != null && readObjects.Length > 0)
            {
                for (int i = 0; i < readObjects.Length; ++i)
                {
                    readObjects[i].SetActive(mail != null && mail.IsRead);
                }
            }

            if (unreadObjects != null && unreadObjects.Length > 0)
            {
                for (int i = 0; i < unreadObjects.Length; ++i)
                {
                    unreadObjects[i].SetActive(mail == null || !mail.IsRead);
                }
            }

            if (claimObjects != null && claimObjects.Length > 0)
            {
                for (int i = 0; i < claimObjects.Length; ++i)
                {
                    claimObjects[i].SetActive(mail != null && mail.HasItemsToClaim() && mail.IsClaim);
                }
            }

            if (unclaimObjects != null && unclaimObjects.Length > 0)
            {
                for (int i = 0; i < unclaimObjects.Length; ++i)
                {
                    unclaimObjects[i].SetActive(mail != null && mail.HasItemsToClaim() && !mail.IsClaim);
                }
            }
        }
    }
}
