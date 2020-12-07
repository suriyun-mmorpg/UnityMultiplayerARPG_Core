using LiteNetLib.Utils;
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
            BaseGameNetworkManager.Singleton.RequestReadMail(MailId, ReadMailCallback);
        }

        private void ReadMailCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, INetSerializable response)
        {
            if (responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                return;
            }
            ResponseReadMailMessage castedResponse = response as ResponseReadMailMessage;
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (castedResponse.error)
                    {
                        case ResponseReadMailMessage.Error.NotAvailable:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE.ToString());
                            break;
                        case ResponseReadMailMessage.Error.NotAllowed:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_MAIL_READ_NOT_ALLOWED.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), errorMessage);
                    break;
                default:
                    UpdateData(castedResponse.mail);
                    break;
            }
        }

        public void OnClickClaimItems()
        {
            BaseGameNetworkManager.Singleton.RequestClaimMailItems(MailId, ClaimMailItemsCallback);
        }

        private void ClaimMailItemsCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, INetSerializable response)
        {
            if (responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                return;
            }
            ResponseClaimMailItemsMessage castedResponse = response as ResponseClaimMailItemsMessage;
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (castedResponse.error)
                    {
                        case ResponseClaimMailItemsMessage.Error.NotAvailable:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE.ToString());
                            break;
                        case ResponseClaimMailItemsMessage.Error.NotAllowed:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_MAIL_CLAIM_NOT_ALLOWED.ToString());
                            break;
                        case ResponseClaimMailItemsMessage.Error.AlreadyClaimed:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_MAIL_CLAIM_ALREADY_CLAIMED.ToString());
                            break;
                        case ResponseClaimMailItemsMessage.Error.CannotCarry:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_MAIL_CLAIM_CANNOT_CARRY.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), errorMessage);
                    break;
                default:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_SUCCESS.ToString()), LanguageManager.GetText(UITextKeys.UI_MAIL_CLAIM_SUCCESS.ToString()));
                    Hide();
                    if (uiMailList)
                        uiMailList.Refresh();
                    break;
            }
        }

        public void OnClickDelete()
        {
            BaseGameNetworkManager.Singleton.RequestReadMail(MailId, DeleteMailCallback);
        }

        private void DeleteMailCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, INetSerializable response)
        {
            if (responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                return;
            }
            ResponseDeleteMailMessage castedResponse = response as ResponseDeleteMailMessage;
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (castedResponse.error)
                    {
                        case ResponseDeleteMailMessage.Error.NotAvailable:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE.ToString());
                            break;
                        case ResponseDeleteMailMessage.Error.NotAllowed:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_MAIL_DELETE_NOT_ALLOWED.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), errorMessage);
                    break;
                default:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_SUCCESS.ToString()), LanguageManager.GetText(UITextKeys.UI_MAIL_DELETE_SUCCESS.ToString()));
                    Hide();
                    if (uiMailList)
                        uiMailList.Refresh();
                    break;
            }
        }

        private void UpdateData(Mail mail)
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
                    uiCurrencies.UpdateData(BasePlayerCharacterController.OwningCharacter, increasingCurrencies);
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
                    uiItems.UpdateData(BasePlayerCharacterController.OwningCharacter, increasingItems);
                    uiItems.Show();
                }
                else
                {
                    uiItems.Hide();
                }
            }

            if (textSentDate != null)
            {
                System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                if (mail != null)
                    dateTime = dateTime.AddSeconds(mail.SentTimestamp);
                textSentDate.text = string.Format(
                    LanguageManager.GetText(formatSentDate),
                    dateTime.GetPrettyDate());
            }
        }
    }
}
