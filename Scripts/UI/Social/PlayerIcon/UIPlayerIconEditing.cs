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

        private List<int> _availableIconIds = new List<int>();
        private List<PlayerIcon> _list = new List<PlayerIcon>();

        private UIList _cacheList;
        public UIList CacheList
        {
            get
            {
                if (_cacheList == null)
                {
                    _cacheList = gameObject.AddComponent<UIList>();
                    _cacheList.uiPrefab = uiPrefab.gameObject;
                    _cacheList.uiContainer = uiContainer;
                }
                return _cacheList;
            }
        }

        private UIPlayerIconSelectionManager _cacheSelectionManager;
        public UIPlayerIconSelectionManager CacheSelectionManager
        {
            get
            {
                if (_cacheSelectionManager == null)
                    _cacheSelectionManager = gameObject.GetOrAddComponent<UIPlayerIconSelectionManager>();
                _cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return _cacheSelectionManager;
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
            GameInstance.ClientUserContentHandlers.RequestAvailableContents(new RequestAvailableContentsMessage()
            {
                type = UnlockableContentType.Frame,
            }, ResponseAvailableContents);
        }

        protected virtual void ResponseAvailableContents(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseAvailableContentsMessage response)
        {
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            _availableIconIds.Clear();
            for (int i = 0; i < response.contents.Length; ++i)
            {
                if (response.contents[i].unlocked)
                    _availableIconIds.Add(response.contents[i].dataId);
            }
            _list.Clear();
            List<PlayerIcon> availableIcons = new List<PlayerIcon>();
            List<PlayerIcon> unavailableIcons = new List<PlayerIcon>();
            foreach (PlayerIcon icon in GameInstance.PlayerIcons.Values)
            {
                if (_availableIconIds.Contains(icon.DataId))
                    availableIcons.Add(icon);
                else
                    unavailableIcons.Add(icon);
            }
            _list.AddRange(availableIcons);
            _list.AddRange(unavailableIcons);
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

            if (_list.Count == 0)
            {
                CacheList.HideAll();
                if (listEmptyObject != null)
                    listEmptyObject.SetActive(true);
                return;
            }

            if (listEmptyObject != null)
                listEmptyObject.SetActive(false);

            UIPlayerIcon tempUI;
            CacheList.Generate(_list, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UIPlayerIcon>();
                tempUI.Data = data;
                tempUI.SetIsLocked(_availableIconIds.Contains(data.DataId));
                tempUI.Show();
                CacheSelectionManager.Add(tempUI);
                if ((selectedDataId == 0 && _availableIconIds.Contains(data.DataId)) || selectedDataId == data.DataId)
                {
                    selectedDataId = data.DataId;
                    tempUI.SelectByManager();
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
