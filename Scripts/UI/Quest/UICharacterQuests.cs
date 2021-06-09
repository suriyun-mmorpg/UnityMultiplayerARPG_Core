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
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
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
            if (CacheSelectionManager.Count > 0)
                CacheSelectionManager.Get(0).OnClickSelect();
            else if (uiDialog != null)
                uiDialog.Hide();
            UpdateOwningCharacterData();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onQuestsOperation += OnQuestsOperation;
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
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

        protected virtual void OnDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnSelect(UICharacterQuest ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.Setup(ui.Data, character, ui.IndexOfData);
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICharacterQuest ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public void UpdateData(IPlayerCharacterData character)
        {
            this.character = character;
            int selectedQuestId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.dataId : 0;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();
            int showingCount = 0;
            UICharacterQuest tempUI;
            CacheList.Generate(character.Quests, (index, characterQuest, ui) =>
            {
                tempUI = ui.GetComponent<UICharacterQuest>();
                if (GameInstance.Quests.ContainsKey(characterQuest.dataId) && 
                    (!HideCompleteQuest || !characterQuest.isComplete))
                {
                    tempUI.Setup(characterQuest, character, index);
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
