using LiteNetLibManager;
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
            int selectedQuestId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.dataId : 0;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();
            int showingCount = 0;
            UICharacterQuest tempUI;
            bool hasTrackingQuest = false;
            for (int i = 0; i < GameInstance.PlayingCharacter.Quests.Count; ++i)
            {
                if (GameInstance.PlayingCharacter.Quests[i].isTracking)
                {
                    hasTrackingQuest = true;
                    break;
                }
            }
            CacheList.Generate(GameInstance.PlayingCharacter.Quests, (index, characterQuest, ui) =>
            {
                tempUI = ui.GetComponent<UICharacterQuest>();
                if (GameInstance.Quests.ContainsKey(characterQuest.dataId) &&
                    (!ShowOnlyTrackingQuests || characterQuest.isTracking || !hasTrackingQuest) &&
                    (!HideCompleteQuest || !characterQuest.isComplete))
                {
                    tempUI.Setup(characterQuest, GameInstance.PlayingCharacter, index);
                    tempUI.Show();
                    CacheSelectionManager.Add(tempUI);
                    if (selectedQuestId == characterQuest.dataId)
                        tempUI.OnClickSelect();
                    showingCount++;
                }
                else
                {
                    tempUI.Hide();
                }
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(showingCount == 0);
        }
    }
}
