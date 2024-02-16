using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIPlayerFrameEditing : UIBase
    {
        [Header("UI Elements")]
        public GameObject listEmptyObject;
        public UIPlayerFrame uiPrefab;
        public Transform uiContainer;
        public UIPlayerFrame[] selectedFrames;

        [Header("Options")]
        public bool updateSelectedFrameOnSelect;

        private List<int> _availableFrameIds = new List<int>();
        private List<PlayerFrame> _list = new List<PlayerFrame>();

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

        private UIPlayerFrameSelectionManager _cacheSelectionManager;
        public UIPlayerFrameSelectionManager CacheSelectionManager
        {
            get
            {
                if (_cacheSelectionManager == null)
                    _cacheSelectionManager = gameObject.GetOrAddComponent<UIPlayerFrameSelectionManager>();
                _cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return _cacheSelectionManager;
            }
        }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelect);
            LoadAvailableFrames();
        }

        public virtual void LoadAvailableFrames()
        {
            GameInstance.ClientUserContentHandlers.RequestAvailableContents(new RequestAvailableContentsMessage()
            {
                type = UnlockableContentType.Frame,
            }, ResponseAvailableContents);
        }

        protected virtual void ResponseAvailableContents(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseAvailableContentsMessage response)
        {
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            _availableFrameIds.Clear();
            for (int i = 0; i < response.contents.Length; ++i)
            {
                if (response.contents[i].unlocked)
                    _availableFrameIds.Add(response.contents[i].dataId);
            }
            _list.Clear();
            List<PlayerFrame> availableFrames = new List<PlayerFrame>();
            List<PlayerFrame> unavailableFrames = new List<PlayerFrame>();
            foreach (PlayerFrame frame in GameInstance.PlayerFrames.Values)
            {
                if (_availableFrameIds.Contains(frame.DataId))
                    availableFrames.Add(frame);
                else
                    unavailableFrames.Add(frame);
            }
            _list.AddRange(availableFrames);
            _list.AddRange(unavailableFrames);
            UpdateData(GameInstance.PlayingCharacter.FrameDataId);
        }

        protected virtual void OnSelect(UIPlayerFrame ui)
        {
            UpdateSelectedFrames();
            if (updateSelectedFrameOnSelect)
                UpdateSelectedFrame();
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

            UIPlayerFrame tempUI;
            CacheList.Generate(_list, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UIPlayerFrame>();
                tempUI.Data = data;
                tempUI.SetIsLocked(_availableFrameIds.Contains(data.DataId));
                tempUI.Show();
                CacheSelectionManager.Add(tempUI);
                if ((selectedDataId == 0 && _availableFrameIds.Contains(data.DataId)) || selectedDataId == data.DataId)
                {
                    selectedDataId = data.DataId;
                    tempUI.SelectByManager();
                }
            });
        }

        public virtual void UpdateSelectedFrames()
        {
            PlayerFrame playerFrame = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data : null;
            if (selectedFrames != null && selectedFrames.Length > 0)
            {
                foreach (UIPlayerFrame selectedFrame in selectedFrames)
                {
                    selectedFrame.Data = playerFrame;
                }
            }
        }

        public virtual void UpdateSelectedFrame()
        {
            PlayerFrame playerFrame = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data : null;
            GameInstance.ClientCharacterHandlers.RequestSetFrame(new RequestSetFrameMessage()
            {
                dataId = playerFrame.DataId,
            }, ResponseSelectedFrame);
        }

        protected virtual void ResponseSelectedFrame(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSetFrameMessage response)
        {
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
        }
    }
}
