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


        private List<int> availableFrameIds = new List<int>();
        private List<PlayerFrame> list = new List<PlayerFrame>();

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

        private UIPlayerFrameSelectionManager cacheSelectionManager;
        public UIPlayerFrameSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UIPlayerFrameSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSelectionManager;
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
            GameInstance.ClientCharacterHandlers.RequestAvailableFrames(ResponseAvailableFrames);
        }

        protected virtual void ResponseAvailableFrames(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseAvailableFramesMessage response)
        {
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            availableFrameIds.Clear();
            availableFrameIds.AddRange(response.frameIds);
            list.Clear();
            List<PlayerFrame> availableFrames = new List<PlayerFrame>();
            List<PlayerFrame> unavailableFrames = new List<PlayerFrame>();
            foreach (PlayerFrame frame in GameInstance.PlayerFrames.Values)
            {
                if (availableFrameIds.Contains(frame.DataId))
                    availableFrames.Add(frame);
                else
                    unavailableFrames.Add(frame);
            }
            list.AddRange(availableFrames);
            list.AddRange(unavailableFrames);
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

            if (list.Count == 0)
            {
                CacheList.HideAll();
                if (listEmptyObject != null)
                    listEmptyObject.SetActive(true);
                return;
            }

            if (listEmptyObject != null)
                listEmptyObject.SetActive(false);

            UIPlayerFrame tempUI;
            CacheList.Generate(list, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UIPlayerFrame>();
                tempUI.Data = data;
                tempUI.SetIsLocked(availableFrameIds.Contains(data.DataId));
                tempUI.Show();
                CacheSelectionManager.Add(tempUI);
                if ((selectedDataId == 0 && availableFrameIds.Contains(data.DataId)) || selectedDataId == data.DataId)
                {
                    selectedDataId = data.DataId;
                    tempUI.OnClickSelect();
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
