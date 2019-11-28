using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterQuests : UIBase
    {
        public IPlayerCharacterData character { get; protected set; }
        public UICharacterQuest uiQuestDialog;
        public UICharacterQuest uiCharacterQuestPrefab;
        public Transform uiCharacterQuestContainer;
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
                    cacheQuestList.uiPrefab = uiCharacterQuestPrefab.gameObject;
                    cacheQuestList.uiContainer = uiCharacterQuestContainer;
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
                    cacheQuestSelectionManager = GetComponent<UICharacterQuestSelectionManager>();
                if (cacheQuestSelectionManager == null)
                    cacheQuestSelectionManager = gameObject.AddComponent<UICharacterQuestSelectionManager>();
                cacheQuestSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheQuestSelectionManager;
            }
        }

        public override void Show()
        {
            CacheQuestSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterQuest);
            CacheQuestSelectionManager.eventOnSelect.AddListener(OnSelectCharacterQuest);
            CacheQuestSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterQuest);
            CacheQuestSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterQuest);
            if (uiQuestDialog != null)
                uiQuestDialog.onHide.AddListener(OnQuestDialogHide);
            if (CacheQuestSelectionManager.Count > 0)
                CacheQuestSelectionManager.Get(0).OnClickSelect();
            else if (uiQuestDialog != null)
                uiQuestDialog.Hide();
            base.Show();
        }

        public override void Hide()
        {
            if (uiQuestDialog != null)
                uiQuestDialog.onHide.RemoveListener(OnQuestDialogHide);
            CacheQuestSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnQuestDialogHide()
        {
            CacheQuestSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCharacterQuest(UICharacterQuest ui)
        {
            if (uiQuestDialog != null)
            {
                uiQuestDialog.selectionManager = CacheQuestSelectionManager;
                uiQuestDialog.Setup(ui.Data, character, ui.IndexOfData);
                uiQuestDialog.Show();
            }
        }

        protected void OnDeselectCharacterQuest(UICharacterQuest ui)
        {
            if (uiQuestDialog != null)
            {
                uiQuestDialog.onHide.RemoveListener(OnQuestDialogHide);
                uiQuestDialog.Hide();
                uiQuestDialog.onHide.AddListener(OnQuestDialogHide);
            }
        }

        public void UpdateData(IPlayerCharacterData character)
        {
            this.character = character;
            int selectedQuestId = CacheQuestSelectionManager.SelectedUI != null ? CacheQuestSelectionManager.SelectedUI.Data.dataId : 0;
            CacheQuestSelectionManager.DeselectSelectedUI();
            CacheQuestSelectionManager.Clear();

            UICharacterQuest tempUiCharacterQuest;
            CacheQuestList.Generate(character.Quests, (index, characterQuest, ui) =>
            {
                tempUiCharacterQuest = ui.GetComponent<UICharacterQuest>();
                if (!HideCompleteQuest || !characterQuest.isComplete)
                {
                    tempUiCharacterQuest.Setup(characterQuest, character, index);
                    tempUiCharacterQuest.Show();
                    CacheQuestSelectionManager.Add(tempUiCharacterQuest);
                    if (selectedQuestId == characterQuest.dataId)
                        tempUiCharacterQuest.OnClickSelect();
                }
                else
                {
                    tempUiCharacterQuest.Hide();
                }
            });
        }
    }
}
