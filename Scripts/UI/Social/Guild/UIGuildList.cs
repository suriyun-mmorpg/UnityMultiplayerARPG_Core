using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildList : UIBase
    {
        [Header("UI Elements")]
        public GameObject listEmptyObject;
        public UIGuildListEntry uiDialog;
        public UIGuildListEntry uiPrefab;
        public Transform uiContainer;

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

        private UIGuildListSelectionManager cacheSelectionManager;
        public UIGuildListSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UIGuildListSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.Toggle;
                return cacheSelectionManager;
            }
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

        protected void OnSelect(UIGuildListEntry ui)
        {
            if (uiDialog != null && ui.Data != null)
            {
                uiDialog.Data = ui.Data;
                uiDialog.Show();
            }
        }

        protected void OnDeselect(UIGuildListEntry ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public void Refresh()
        {
            GameInstance.ClientGuildHandlers.RequestFindGuilds(new RequestFindGuildsMessage()
            {
                guildName = string.Empty,
            }, GuildListCallback);
        }

        private void GuildListCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseGuildListMessage response)
        {
            ClientGuildActions.ResponseGuildList(requestHandler, responseCode, response);
            int selectedId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.Id : 0;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();
            if (listEmptyObject != null)
                listEmptyObject.SetActive(true);
            if (responseCode == AckResponseCode.Unimplemented ||
                responseCode == AckResponseCode.Timeout)
                return;
            UIGuildListEntry tempUi;
            CacheList.Generate(response.guilds, (index, guildListEntry, ui) =>
            {
                tempUi = ui.GetComponent<UIGuildListEntry>();
                tempUi.Data = guildListEntry;
                tempUi.Show();
                CacheSelectionManager.Add(tempUi);
                if (selectedId == guildListEntry.Id)
                    tempUi.OnClickSelect();
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(response.guilds.Length == 0);
        }
    }
}
