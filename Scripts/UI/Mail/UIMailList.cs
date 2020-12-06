using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIMailList : UIBase
    {
        [Header("UI Elements")]
        public UIMail uiDialog;
        public UIMailListEntry uiPrefab;
        public Transform uiContainer;

        [Header("Options")]
        public bool onlyNewMails = true;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiPrefab.gameObject;
                    cacheList.uiContainer = uiContainer;
                }
                return cacheList;
            }
        }

        private UIMailListSelectionManager cacheSelectionManager;
        public UIMailListSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UIMailListSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSelectionManager;
            }
        }

        public void Refresh()
        {
            BaseGameNetworkManager.Singleton.RequestMailList(onlyNewMails, MailListCallback);
        }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelect.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelect.AddListener(OnSelect);
            CacheSelectionManager.eventOnDeselect.RemoveListener(OnDeselect);
            CacheSelectionManager.eventOnDeselect.AddListener(OnDeselect);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnDialogHide);
            Refresh();
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected void OnDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelect(UIMailListEntry ui)
        {
            if (uiDialog != null && ui.Data != null)
            {
                uiDialog.uiMailList = this;
                uiDialog.MailId = ui.Data.Id;
                uiDialog.Show();
            }
        }

        protected void OnDeselect(UIMailListEntry ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        private void MailListCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, INetSerializable response)
        {
            if (responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                return;
            }
            ResponseMailListMessage castedResponse = response as ResponseMailListMessage;
            //castedResponse.mails;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            UIMailListEntry tempUi;
            CacheList.Generate(castedResponse.mails, (index, mailListEntry, ui) =>
            {
                tempUi = ui.GetComponent<UIMailListEntry>();
                tempUi.Data = mailListEntry;
                tempUi.Show();
                CacheSelectionManager.Add(tempUi);
            });
        }
    }
}
