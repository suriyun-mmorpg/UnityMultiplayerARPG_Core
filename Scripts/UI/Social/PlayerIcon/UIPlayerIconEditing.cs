using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIPlayerIconEditing : UIBase
    {
        [Header("UI Elements")]
        public GameObject listEmptyObject;
        public UIPlayerIcon uiPrefab;
        public Transform uiContainer;
        public UIPlayerIcon[] selectedIcons;

        [Header("Options")]
        public bool updateSelectedIconOnSelect;


        private List<int> availableIconIds = new List<int>();
        private List<PlayerIcon> list = new List<PlayerIcon>();

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

        private UIPlayerIconSelectionManager cacheSelectionManager;
        public UIPlayerIconSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UIPlayerIconSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSelectionManager;
            }
        }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelect);
            LoadAvailableIcons();
        }

        public virtual void LoadAvailableIcons()
        {
            GameInstance.ClientCharacterHandlers.RequestAvailableIcons(ResponseAvailableIcons);
        }

        protected virtual void ResponseAvailableIcons(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseAvailableIconsMessage response)
        {
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            availableIconIds.Clear();
            availableIconIds.AddRange(response.iconIds);
            list.Clear();
            List<PlayerIcon> availableIcons = new List<PlayerIcon>();
            List<PlayerIcon> unavailableIcons = new List<PlayerIcon>();
            foreach (PlayerIcon icon in GameInstance.PlayerIcons.Values)
            {
                if (availableIconIds.Contains(icon.DataId))
                    availableIcons.Add(icon);
                else
                    unavailableIcons.Add(icon);
            }
            list.AddRange(availableIcons);
            list.AddRange(unavailableIcons);
            UpdateData(GameInstance.PlayingCharacter.IconDataId);
        }

        protected virtual void OnSelect(UIPlayerIcon ui)
        {
            UpdateSelectedIcons();
            if (updateSelectedIconOnSelect)
                UpdateSelectedIcon();
        }

        public void UpdateData()
        {
            int selectedDataId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.DataId : 0;
            UpdateData(selectedDataId);
        }

        public virtual void UpdateData(int selectedDataId)
        {
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            if (list.Count == 0)
            {
                CacheList.HideAll();
                if (listEmptyObject != null)
                    listEmptyObject.SetActive(true);
                return;
            }

            if (listEmptyObject != null)
                listEmptyObject.SetActive(false);

            UIPlayerIcon tempUI;
            CacheList.Generate(list, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UIPlayerIcon>();
                tempUI.Data = data;
                tempUI.SetIsLocked(availableIconIds.Contains(data.DataId));
                tempUI.Show();
                CacheSelectionManager.Add(tempUI);
                if ((selectedDataId == 0 && availableIconIds.Contains(data.DataId)) || selectedDataId == data.DataId)
                {
                    selectedDataId = data.DataId;
                    tempUI.OnClickSelect();
                }
            });
        }

        public virtual void UpdateSelectedIcons()
        {
            PlayerIcon playerIcon = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data : null;
            if (selectedIcons != null && selectedIcons.Length > 0)
            {
                foreach (UIPlayerIcon selectedIcon in selectedIcons)
                {
                    selectedIcon.Data = playerIcon;
                }
            }
        }

        public virtual void UpdateSelectedIcon()
        {
            PlayerIcon playerIcon = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data : null;
            GameInstance.ClientCharacterHandlers.RequestSetIcon(new RequestSetIconMessage()
            {
                dataId = playerIcon.DataId,
            }, ResponseSelectedIcon);
        }

        protected virtual void ResponseSelectedIcon(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSetIconMessage response)
        {
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
        }
    }
}
