using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterQuests : UIBase
    {
        public ICharacterData character { get; protected set; }
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
                uiQuestDialog.Setup(ui.Data, character, ui.IndexOfData);
                uiQuestDialog.Show();
            }
        }

        protected void OnDeselectCharacterQuest(UICharacterQuest ui)
        {
            if (uiQuestDialog != null)
                uiQuestDialog.Hide();
        }

        public void UpdateData(IPlayerCharacterData character)
        {
            this.character = character;
            int selectedQuestId = CacheCharacterQuestSelectionManager.SelectedUI != null ? CacheCharacterQuestSelectionManager.SelectedUI.Data.dataId : 0;
            CacheCharacterQuestSelectionManager.DeselectSelectedUI();
            CacheCharacterQuestSelectionManager.Clear();

            if (character == null)
            {
                CacheCharacterQuestList.HideAll();
                return;
            }

            IList<CharacterQuest> characterQuests = character.Quests;
            CacheCharacterQuestList.Generate(characterQuests, (index, characterQuest, ui) =>
            {
                UICharacterQuest uiCharacterQuest = ui.GetComponent<UICharacterQuest>();
                uiCharacterQuest.Setup(characterQuest, character, index);
                uiCharacterQuest.Show();
                CacheCharacterQuestSelectionManager.Add(uiCharacterQuest);
                if (selectedQuestId.Equals(characterQuest.dataId))
                    uiCharacterQuest.OnClickSelect();
            });
        }
    }
}
