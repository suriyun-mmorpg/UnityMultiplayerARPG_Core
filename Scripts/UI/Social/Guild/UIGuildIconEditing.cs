using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIGuildIconEditing : UIBase
    {
        [Header("UI Elements")]
        public GameObject listEmptyObject;
        public UIGuildIcon uiPrefab;
        public Transform uiContainer;
        public UIGuildIcon[] selectedIcons;

        [Header("Options")]
        public bool updateGuildOptionsOnSelectIcon;

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

        private UIGuildIconEditingSelectionManager cacheSelectionManager;
        public UIGuildIconEditingSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                {
                    cacheSelectionManager = gameObject.GetOrAddComponent<UIGuildIconEditingSelectionManager>();
                    cacheSelectionManager.selectionMode = UISelectionMode.Toggle;
                }
                return cacheSelectionManager;
            }
        }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelect);
        }

        protected virtual void OnDisable()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnSelect(UIGuildIcon ui)
        {
            if (selectedIcons != null && selectedIcons.Length > 0)
            {
                foreach (UIGuildIcon selectedIcon in selectedIcons)
                {
                    selectedIcon.Data = ui.Data;
                }
                if (updateGuildOptionsOnSelectIcon)
                {
                    UpdateGuildOptions();
                }
            }
        }

        public void UpdateData()
        {
            int selectedId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.DataId : 0;
            UpdateData(selectedId);
        }

        public virtual void UpdateData(int selectedId)
        {
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            CacheList.Generate(GameInstance.GuildIcons.Values, (index, data, ui) =>
            {
                UIGuildIcon uiGuildIcon = ui.GetComponent<UIGuildIcon>();
                uiGuildIcon.Data = data;
                uiGuildIcon.Show();
                CacheSelectionManager.Add(uiGuildIcon);
                if (selectedId == data.DataId)
                    uiGuildIcon.OnClickSelect();
            });
        }

        public virtual void UpdateGuildOptions()
        {
            if (GameInstance.JoinedGuild == null)
            {
                // No joined guild data, so it can't update guild data
                return;
            }
            // Get current guild options before modify and save
            GuildOptions options = new GuildOptions();
            if (!string.IsNullOrEmpty(GameInstance.JoinedGuild.options))
                options = JsonUtility.FromJson<GuildOptions>(GameInstance.JoinedGuild.options);
            options.iconDataId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.DataId : 0;
            GameInstance.ClientGuildHandlers.RequestChangeGuildOptions(new RequestChangeGuildOptionsMessage()
            {
                options = JsonUtility.ToJson(options),
            }, ChangeGuildOptionsCallback);
        }

        private void ChangeGuildOptionsCallback(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseChangeGuildOptionsMessage response)
        {
            ClientGuildActions.ResponseChangeGuildOptions(requestHandler, responseCode, response);
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
        }
    }
}
