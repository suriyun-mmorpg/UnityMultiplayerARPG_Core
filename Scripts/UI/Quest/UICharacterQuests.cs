using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterQuests : UIBase
    {
        public UICharacterQuest uiQuestDialog;
        public UICharacterQuest uiCharacterQuestPrefab;
        public Transform uiCharacterQuestContainer;

        private UIList cacheCharacterQuestList;
        public UIList CacheCharacterQuestList
        {
            get
            {
                if (cacheCharacterQuestList == null)
                {
                    cacheCharacterQuestList = gameObject.AddComponent<UIList>();
                    cacheCharacterQuestList.uiPrefab = uiCharacterQuestPrefab.gameObject;
                    cacheCharacterQuestList.uiContainer = uiCharacterQuestContainer;
                }
                return cacheCharacterQuestList;
            }
        }

        private UICharacterQuestSelectionManager cacheCharacterQuestSelectionManager;
        public UICharacterQuestSelectionManager CacheCharacterQuestSelectionManager
        {
            get
            {
                if (cacheCharacterQuestSelectionManager == null)
                    cacheCharacterQuestSelectionManager = GetComponent<UICharacterQuestSelectionManager>();
                if (cacheCharacterQuestSelectionManager == null)
                    cacheCharacterQuestSelectionManager = gameObject.AddComponent<UICharacterQuestSelectionManager>();
                cacheCharacterQuestSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheCharacterQuestSelectionManager;
            }
        }

        public override void Show()
        {
            CacheCharacterQuestSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterQuest);
            CacheCharacterQuestSelectionManager.eventOnSelect.AddListener(OnSelectCharacterQuest);
            CacheCharacterQuestSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterQuest);
            CacheCharacterQuestSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterQuest);
            if (CacheCharacterQuestSelectionManager.Count > 0)
                CacheCharacterQuestSelectionManager.Get(0).OnClickSelect();
            else if (uiQuestDialog != null)
                uiQuestDialog.Hide();
            base.Show();
        }

        public override void Hide()
        {
            CacheCharacterQuestSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterQuest(UICharacterQuest ui)
        {
            if (uiQuestDialog != null)
            {
                uiQuestDialog.selectionManager = CacheCharacterQuestSelectionManager;
                uiQuestDialog.Setup(ui.Data, BasePlayerCharacterController.OwningCharacter, ui.IndexOfData);
                uiQuestDialog.Show();
            }
        }

        protected void OnDeselectCharacterQuest(UICharacterQuest ui)
        {
            if (uiQuestDialog != null)
                uiQuestDialog.Hide();
        }

        public void UpdateData()
        {
            int selectedQuestId = CacheCharacterQuestSelectionManager.SelectedUI != null ? CacheCharacterQuestSelectionManager.SelectedUI.Data.dataId : 0;
            CacheCharacterQuestSelectionManager.DeselectSelectedUI();
            CacheCharacterQuestSelectionManager.Clear();

            IList<CharacterQuest> characterQuests = BasePlayerCharacterController.OwningCharacter.Quests;
            CacheCharacterQuestList.Generate(characterQuests, (index, characterQuest, ui) =>
            {
                UICharacterQuest uiCharacterQuest = ui.GetComponent<UICharacterQuest>();
                uiCharacterQuest.Setup(characterQuest, BasePlayerCharacterController.OwningCharacter, index);
                uiCharacterQuest.Show();
                CacheCharacterQuestSelectionManager.Add(uiCharacterQuest);
                if (selectedQuestId.Equals(characterQuest.dataId))
                    uiCharacterQuest.OnClickSelect();
            });
        }
    }
}
