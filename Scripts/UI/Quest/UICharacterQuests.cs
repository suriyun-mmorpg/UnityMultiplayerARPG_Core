using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterQuests : UIBase
    {
        public IPlayerCharacterData character { get; protected set; }
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
                    UpdateData(character);
                }
            }
        }


        private UIList cacheQuestList;
        public UIList CacheQuestList
        {
            get
            {
                if (cacheQuestList == null)
                {
                    cacheQuestList = gameObject.AddComponent<UIList>();
                    cacheQuestList.uiPrefab = uiPrefab.gameObject;
                    cacheQuestList.uiContainer = uiContainer;
                }
                return cacheQuestList;
            }
        }

        private UICharacterQuestSelectionManager cacheQuestSelectionManager;
        public UICharacterQuestSelectionManager CacheQuestSelectionManager
        {
            get
            {
                if (cacheQuestSelectionManager == null)
                    cacheQuestSelectionManager = gameObject.GetOrAddComponent<UICharacterQuestSelectionManager>();
                cacheQuestSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheQuestSelectionManager;
            }
        }

        protected virtual void OnEnable()
        {
            CacheQuestSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterQuest);
            CacheQuestSelectionManager.eventOnSelect.AddListener(OnSelectCharacterQuest);
            CacheQuestSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterQuest);
            CacheQuestSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterQuest);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnQuestDialogHide);
            if (CacheQuestSelectionManager.Count > 0)
                CacheQuestSelectionManager.Get(0).OnClickSelect();
            else if (uiDialog != null)
                uiDialog.Hide();
            UpdateOwningCharacterData();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onQuestsOperation += OnQuestsOperation;
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnQuestDialogHide);
            CacheQuestSelectionManager.DeselectSelectedUI();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onQuestsOperation -= OnQuestsOperation;
        }

        private void OnQuestsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        public void UpdateOwningCharacterData()
        {
            if (GameInstance.PlayingCharacter == null) return;
            UpdateData(GameInstance.PlayingCharacter);
        }

        protected void OnQuestDialogHide()
        {
            CacheQuestSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCharacterQuest(UICharacterQuest ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheQuestSelectionManager;
                uiDialog.Setup(ui.Data, character, ui.IndexOfData);
                uiDialog.Show();
            }
        }

        protected void OnDeselectCharacterQuest(UICharacterQuest ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnQuestDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnQuestDialogHide);
            }
        }

        public void UpdateData(IPlayerCharacterData character)
        {
            this.character = character;
            int selectedQuestId = CacheQuestSelectionManager.SelectedUI != null ? CacheQuestSelectionManager.SelectedUI.Data.dataId : 0;
            CacheQuestSelectionManager.DeselectSelectedUI();
            CacheQuestSelectionManager.Clear();
            int showingCount = 0;
            UICharacterQuest tempUI;
            CacheQuestList.Generate(character.Quests, (index, characterQuest, ui) =>
            {
                tempUI = ui.GetComponent<UICharacterQuest>();
                if (GameInstance.Quests.ContainsKey(characterQuest.dataId) && 
                    (!HideCompleteQuest || !characterQuest.isComplete))
                {
                    tempUI.Setup(characterQuest, character, index);
                    tempUI.Show();
                    CacheQuestSelectionManager.Add(tempUI);
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
