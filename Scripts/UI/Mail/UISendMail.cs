using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UISendMail : UIBase
    {
        public InputFieldWrapper inputReceiverName;
        public InputFieldWrapper inputTitle;
        public InputFieldWrapper inputContent;
        public InputFieldWrapper inputGold;

        public string ReceiverName
        {
            get { return inputReceiverName == null ? string.Empty : inputReceiverName.text; }
        }
        public string Title
        {
            get { return inputTitle == null ? string.Empty : inputTitle.text; }
        }
        public string Content
        {
            get { return inputContent == null ? string.Empty : inputContent.text; }
        }
        public int Gold
        {
            get
            {
                try
                {
                    return int.Parse(inputGold.text);
                }
                catch
                {
                    return 0;
                }
            }
        }

        private void OnEnable()
        {
            if (inputReceiverName != null)
                inputReceiverName.text = string.Empty;
            if (inputTitle != null)
                inputTitle.text = string.Empty;
            if (inputContent != null)
                inputContent.text = string.Empty;
            if (inputGold != null)
            {
                inputGold.text = "0";
                inputGold.contentType = InputField.ContentType.IntegerNumber;
            }
        }

        public void OnClickSend()
        {
            if (inputReceiverName != null)
                inputReceiverName.interactable = false;
            if (inputTitle != null)
                inputTitle.interactable = false;
            if (inputContent != null)
                inputContent.interactable = false;
            if (inputGold != null)
                inputGold.interactable = false;
            BaseGameNetworkManager.Singleton.RequestSendMail(
                ReceiverName,
                Title,
                Content,
                Gold,
                MailSendCallback);
        }

        private async UniTaskVoid MailSendCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSendMailMessage response)
        {
            await UniTask.Yield();
            if (inputReceiverName != null)
                inputReceiverName.interactable = true;
            if (inputTitle != null)
                inputTitle.interactable = true;
            if (inputContent != null)
                inputContent.interactable = true;
            if (inputGold != null)
                inputGold.interactable = true;
            if (responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                return;
            }
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (response.error)
                    {
                        case ResponseSendMailMessage.Error.NotAvailable:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE.ToString());
                            break;
                        case ResponseSendMailMessage.Error.NotAllowed:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_MAIL_SEND_NOT_ALLOWED.ToString());
                            break;
                        case ResponseSendMailMessage.Error.NoReceiver:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_MAIL_SEND_NO_RECEIVER.ToString());
                            break;
                        case ResponseSendMailMessage.Error.NotEnoughGold:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), errorMessage);
                    break;
                default:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_SUCCESS.ToString()), LanguageManager.GetText(UITextKeys.UI_MAIL_SEND_SUCCESS.ToString()));
                    Hide();
                    break;
            }
        }
    }
}
