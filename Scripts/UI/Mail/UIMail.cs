using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIMail : UIBase
    {
        public UIMailList uiMailList;

        private string mailId;
        public string MailId
        {
            get { return mailId; }
            set
            {
                mailId = value;
                ReadMail();
            }
        }

        private void ReadMail()
        {
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
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_SUCCESS.ToString()), LanguageManager.GetText(UITextKeys.UI_MAIL_SEND_SUCCESS.ToString()));
                    UpdateData(castedResponse.mail);
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

        }
    }
}
