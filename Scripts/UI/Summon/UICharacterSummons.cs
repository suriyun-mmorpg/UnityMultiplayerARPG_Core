using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterSummons : UIBase
    {
        public ICharacterData character { get; protected set; }
        public UICharacterSummon uiSummonDialog;
        public UICharacterSummon uiCharacterSummonPrefab;
        public Transform uiCharacterSummonContainer;

        private UIList cacheSummonList;
        public UIList CacheSummonList
        {
            get
            {
                if (cacheSummonList == null)
                {
                    cacheSummonList = gameObject.AddComponent<UIList>();
                    cacheSummonList.uiPrefab = uiCharacterSummonPrefab.gameObject;
                    cacheSummonList.uiContainer = uiCharacterSummonContainer;
                }
                return cacheSummonList;
            }
        }

        private UICharacterSummonSelectionManager cacheSummonSelectionManager;
        public UICharacterSummonSelectionManager CacheSummonSelectionManager
        {
            get
            {
                if (cacheSummonSelectionManager == null)
                    cacheSummonSelectionManager = GetComponent<UICharacterSummonSelectionManager>();
                if (cacheSummonSelectionManager == null)
                    cacheSummonSelectionManager = gameObject.AddComponent<UICharacterSummonSelectionManager>();
                cacheSummonSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSummonSelectionManager;
            }
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }

        public override void Show()
        {
            CacheSummonSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterSummon);
            CacheSummonSelectionManager.eventOnSelect.AddListener(OnSelectCharacterSummon);
            CacheSummonSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterSummon);
            CacheSummonSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterSummon);
            if (uiSummonDialog != null)
                uiSummonDialog.onHide.AddListener(OnSummonDialogHide);
            base.Show();
        }

        public override void Hide()
        {
            if (uiSummonDialog != null)
                uiSummonDialog.onHide.RemoveListener(OnSummonDialogHide);
            CacheSummonSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSummonDialogHide()
        {
            CacheSummonSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCharacterSummon(UICharacterSummon ui)
        {
            if (uiSummonDialog != null)
            {
                uiSummonDialog.selectionManager = CacheSummonSelectionManager;
                uiSummonDialog.Setup(ui.Data, character, ui.IndexOfData);
                uiSummonDialog.Show();
            }
        }

        protected void OnDeselectCharacterSummon(UICharacterSummon ui)
        {
            if (uiSummonDialog != null)
            {
                uiSummonDialog.onHide.RemoveListener(OnSummonDialogHide);
                uiSummonDialog.Hide();
                uiSummonDialog.onHide.AddListener(OnSummonDialogHide);
            }
        }

        public void UpdateData(ICharacterData character)
        {
            this.character = character;
            uint selectedSummonObjectId = CacheSummonSelectionManager.SelectedUI != null ? CacheSummonSelectionManager.SelectedUI.CharacterSummon.objectId : 0;
            CacheSummonSelectionManager.DeselectSelectedUI();
            CacheSummonSelectionManager.Clear();

            Dictionary<int, UICharacterSummon> stackingSkillSummons = new Dictionary<int, UICharacterSummon>();
            UICharacterSummon tempUiCharacterSummon;
            CacheSummonList.Generate(character.Summons, (index, characterSummon, ui) =>
            {
                if (characterSummon.type == SummonType.Skill && stackingSkillSummons.ContainsKey(characterSummon.dataId))
                {
                    stackingSkillSummons[characterSummon.dataId].AddStackingEntry(characterSummon);
                    ui.gameObject.SetActive(false);
                }
                else
                {
                    tempUiCharacterSummon = ui.GetComponent<UICharacterSummon>();
                    tempUiCharacterSummon.Setup(characterSummon, character, index);
                    tempUiCharacterSummon.Show();
                    switch (characterSummon.type)
                    {
                        case SummonType.Skill:
                            stackingSkillSummons.Add(characterSummon.dataId, tempUiCharacterSummon);
                            break;
                        case SummonType.Pet:
                            ui.transform.SetAsFirstSibling();
                            break;
                    }
                    CacheSummonSelectionManager.Add(tempUiCharacterSummon);
                    if (selectedSummonObjectId == characterSummon.objectId)
                        tempUiCharacterSummon.OnClickSelect();
                }
            });
        }
    }
}
