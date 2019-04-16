using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterQuests : UIBase
    {
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
                    UpdateData();
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
                uiQuestDialog.Setup(ui.Data, BasePlayerCharacterController.OwningCharacter, ui.IndexOfData);
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

        public void UpdateData()
        {
            int selectedQuestId = CacheQuestSelectionManager.SelectedUI != null ? CacheQuestSelectionManager.SelectedUI.Data.dataId : 0;
            CacheQuestSelectionManager.DeselectSelectedUI();
            CacheQuestSelectionManager.Clear();

            List<CharacterQuest> characterQuests = new List<CharacterQuest>();
            foreach (CharacterQuest characterQuest in BasePlayerCharacterController.OwningCharacter.Quests)
            {
                if (HideCompleteQuest || characterQuest.isComplete)
                    continue;
                characterQuests.Add(characterQuest);
            }
            CacheQuestList.Generate(characterQuests, (index, characterQuest, ui) =>
            {
                UICharacterQuest uiCharacterQuest = ui.GetComponent<UICharacterQuest>();
                uiCharacterQuest.Setup(characterQuest, BasePlayerCharacterController.OwningCharacter, index);
                uiCharacterQuest.Show();
                CacheQuestSelectionManager.Add(uiCharacterQuest);
                if (selectedQuestId == characterQuest.dataId)
                    uiCharacterQuest.OnClickSelect();
            });
        }
    }
}
