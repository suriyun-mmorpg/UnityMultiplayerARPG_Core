using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIPlayerTitleEditing : UIBase
    {
        [Header("UI Elements")]
        public GameObject listEmptyObject;
        public UIPlayerTitle uiPrefab;
        public Transform uiContainer;
        public UIPlayerTitle[] selectedTitles;

        [Header("Options")]
        public bool updateSelectedTitleOnSelect;


        private List<int> availableTitleIds = new List<int>();
        private List<PlayerTitle> list = new List<PlayerTitle>();

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

        private UIPlayerTitleSelectionManager cacheSelectionManager;
        public UIPlayerTitleSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UIPlayerTitleSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSelectionManager;
            }
        }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelect);
            LoadAvailableTitles();
        }

        public virtual void LoadAvailableTitles()
        {
            GameInstance.ClientCharacterHandlers.RequestAvailableTitles(ResponseAvailableTitles);
        }

        protected virtual void ResponseAvailableTitles(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseAvailableTitlesMessage response)
        {
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            availableTitleIds.Clear();
            availableTitleIds.AddRange(response.titleIds);
            list.Clear();
            List<PlayerTitle> availableTitles = new List<PlayerTitle>();
            List<PlayerTitle> unavailableTitles = new List<PlayerTitle>();
            foreach (PlayerTitle title in GameInstance.PlayerTitles.Values)
            {
                if (availableTitleIds.Contains(title.DataId))
                    availableTitles.Add(title);
                else
                    unavailableTitles.Add(title);
            }
            list.AddRange(availableTitles);
            list.AddRange(unavailableTitles);
            UpdateData(GameInstance.PlayingCharacter.TitleDataId);
        }

        protected virtual void OnSelect(UIPlayerTitle ui)
        {
            UpdateSelectedTitles();
            if (updateSelectedTitleOnSelect)
                UpdateSelectedTitle();
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

            UIPlayerTitle tempUI;
            CacheList.Generate(list, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UIPlayerTitle>();
                tempUI.Data = data;
                tempUI.SetIsLocked(availableTitleIds.Contains(data.DataId));
                tempUI.Show();
                CacheSelectionManager.Add(tempUI);
                if ((selectedDataId == 0 && availableTitleIds.Contains(data.DataId)) || selectedDataId == data.DataId)
                {
                    selectedDataId = data.DataId;
                    tempUI.OnClickSelect();
                }
            });
        }

        public virtual void UpdateSelectedTitles()
        {
            PlayerTitle playerTitle = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data : null;
            if (selectedTitles != null && selectedTitles.Length > 0)
            {
                foreach (UIPlayerTitle selectedTitle in selectedTitles)
                {
                    selectedTitle.Data = playerTitle;
                }
            }
        }

        public virtual void UpdateSelectedTitle()
        {
            PlayerTitle playerTitle = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data : null;
            GameInstance.ClientCharacterHandlers.RequestSetTitle(new RequestSetTitleMessage()
            {
                dataId = playerTitle.DataId,
            }, ResponseSelectedTitle);
        }

        protected virtual void ResponseSelectedTitle(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSetTitleMessage response)
        {
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
        }
    }
}
