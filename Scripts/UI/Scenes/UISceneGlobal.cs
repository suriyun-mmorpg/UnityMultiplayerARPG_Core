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
            var errorMessage = "Unknow";
            switch (disconnectInfo.Reason)
            {
                case DisconnectReason.DisconnectPeerCalled:
                    errorMessage = "You have been kicked from server";
                    break;
                case DisconnectReason.ConnectionFailed:
                    errorMessage = "Cannot connect to the server";
                    break;
                case DisconnectReason.RemoteConnectionClose:
                    errorMessage = "Server has been closed";
                    break;
                case DisconnectReason.SocketReceiveError:
                    errorMessage = "Cannot receive data";
                    break;
                case DisconnectReason.SocketSendError:
                    errorMessage = "Cannot send data";
                    break;
                case DisconnectReason.Timeout:
                    errorMessage = "Connection timeout";
                    break;
            }
            Singleton.ShowMessageDialog("Disconnected", errorMessage, true, false, false, false);
        }
    }
}
