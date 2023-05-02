using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterQuests : UIBase
    {
        public GameObject listEmptyObject;
        [FormerlySerializedAs("uiQuestDialog")]
        public UICharacterQuest uiDialog;
        [FormerlySerializedAs("uiCharacterQuestPrefab")]
        public UICharacterQuest uiPrefab;
        [FormerlySerializedAs("uiCharacterQuestContainer")]
        public Transform uiContainer;

        [SerializeField]
        private bool hideCompleteQuest;
        public bool HideCompleteQuest
        {
            get { return hideCompleteQuest; }
            set
            {
                if (hideCompleteQuest != value)
                {
                    hideCompleteQuest = value;
                    UpdateData();
                }
            }
        }

        [SerializeField]
        private bool showOnlyTrackingQuests;
        public bool ShowOnlyTrackingQuests
        {
            get { return showOnlyTrackingQuests; }
            set
            {
                if (showOnlyTrackingQuests != value)
                {
                    showOnlyTrackingQuests = value;
                    UpdateData();
                }
            }
        }

        [SerializeField]
        private bool showAllWhenNoTrackedQuests;
        public bool ShowAllWhenNoTrackedQuests
        {
            get { return showAllWhenNoTrackedQuests; }
            set
            {
                if (showAllWhenNoTrackedQuests != value)
                {
                    showAllWhenNoTrackedQuests = value;
                    UpdateData();
                }
            }
        }

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

        private UICharacterQuestSelectionManager cacheSelectionManager;
        public UICharacterQuestSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UICharacterQuestSelectionManager>();
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
            UpdateData();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onQuestsOperation += OnQuestsOperation;
        }

        protected virtual void OnDisable()
        {
            CacheSelectionManager.DeselectSelectedUI();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onQuestsOperation -= OnQuestsOperation;
        }

        private void OnQuestsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData();
        }

        protected virtual void OnSelect(UICharacterQuest ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.Setup(ui.Data, GameInstance.PlayingCharacter, ui.IndexOfData);
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICharacterQuest ui)
        {
            if (uiDialog != null)
                uiDialog.Hide();
        }

        public void UpdateData()
        {
            int selectedDataId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.dataId : 0;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            List<CharacterQuest> filteredList = UICharacterQuestsUtils.GetFilteredList(GameInstance.PlayingCharacter.Quests, ShowOnlyTrackingQuests, ShowAllWhenNoTrackedQuests, HideCompleteQuest);
            if (filteredList.Count == 0)
            {
                if (uiDialog != null)
                    uiDialog.Hide();
                CacheList.HideAll();
                if (listEmptyObject != null)
                    listEmptyObject.SetActive(true);
                return;
            }

            if (listEmptyObject != null)
                listEmptyObject.SetActive(false);

            UICharacterQuest tempUI;
            CacheList.Generate(filteredList, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UICharacterQuest>();
                tempUI.Setup(data, GameInstance.PlayingCharacter, index);
                tempUI.Show();
                CacheSelectionManager.Add(tempUI);
                if (selectedDataId == data.dataId)
                    tempUI.OnClickSelect();
            });
        }
    }
}
