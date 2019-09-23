using LiteNetLib;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UISceneGlobal : MonoBehaviour
    {
        public static UISceneGlobal Singleton { get; private set; }
        public UIMessageDialog uiMessageDialog;
        public UIInputDialog uiInputDialog;

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ShowMessageDialog(string title,
            string description,
            bool showButtonOkay = true,
            bool showButtonYes = false,
            bool showButtonNo = false,
            bool showButtonCancel = false,
            System.Action onClickOkay = null,
            System.Action onClickYes = null,
            System.Action onClickNo = null,
            System.Action onClickCancel = null)
        {
            uiMessageDialog.Show(title,
                description,
                showButtonOkay,
                showButtonYes,
                showButtonNo,
                showButtonCancel,
                onClickOkay,
                onClickYes,
                onClickNo,
                onClickCancel);
        }

        public void ShowInputDialog(string title,
            string description,
            System.Action<string> onConfirmText,
            string defaultText = "")
        {
            uiInputDialog.Show(title,
                description,
                onConfirmText,
                defaultText);
        }

        public void ShowInputDialog(string title,
            string description,
            System.Action<int> onConfirmInteger,
            int? minAmount = null,
            int? maxAmount = null,
            int defaultAmount = 0)
        {
            uiInputDialog.Show(title,
                description,
                onConfirmInteger,
                minAmount,
                maxAmount,
                defaultAmount);
        }

        public void ShowInputDialog(string title,
            string description,
            System.Action<float> onConfirmDecimal,
            float? minAmount = null,
            float? maxAmount = null,
            float defaultAmount = 0f)
        {
            uiInputDialog.Show(title,
                description,
                onConfirmDecimal,
                minAmount,
                maxAmount,
                defaultAmount);
        }

        public void ShowDisconnectDialog(DisconnectInfo disconnectInfo)
        {
            string errorMessage = LanguageManager.GetUnknowTitle();
            switch (disconnectInfo.Reason)
            {
                case DisconnectReason.DisconnectPeerCalled:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_KICKED_FROM_SERVER.ToString());
                    break;
                case DisconnectReason.ConnectionFailed:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_FAILED.ToString());
                    break;
                case DisconnectReason.ConnectionRejected:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_REJECTED.ToString());
                    break;
                case DisconnectReason.RemoteConnectionClose:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_REMOTE_CONNECTION_CLOSE.ToString());
                    break;
                case DisconnectReason.InvalidProtocol:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_INVALID_PROTOCOL.ToString());
                    break;
                case DisconnectReason.HostUnreachable:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_HOST_UNREACHABLE.ToString());
                    break;
                case DisconnectReason.Timeout:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString());
                    break;
            }
            Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_DISCONNECTED.ToString()), errorMessage, true, false, false, false);
        }
    }
}
