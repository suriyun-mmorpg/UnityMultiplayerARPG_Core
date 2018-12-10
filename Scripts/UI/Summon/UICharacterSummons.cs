using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterSummonSelectionManager))]
    public partial class UICharacterSummons : UIBase
    {
        public ICharacterData character { get; protected set; }
        public UICharacterSummon uiSummonDialog;
        public UICharacterSummon uiCharacterSummonPrefab;
        public Transform uiCharacterSummonContainer;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiCharacterSummonPrefab.gameObject;
                    cacheList.uiContainer = uiCharacterSummonContainer;
                }
                return cacheList;
            }
        }

        private UICharacterSummonSelectionManager selectionManager;
        public UICharacterSummonSelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UICharacterSummonSelectionManager>();
                selectionManager.selectionMode = UISelectionMode.SelectSingle;
                return selectionManager;
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
            SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterSummon);
            SelectionManager.eventOnSelect.AddListener(OnSelectCharacterSummon);
            SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterSummon);
            SelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterSummon);
            base.Show();
        }

        public override void Hide()
        {
            SelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterSummon(UICharacterSummon ui)
        {
            if (uiSummonDialog != null)
            {
                uiSummonDialog.selectionManager = SelectionManager;
                uiSummonDialog.Setup(ui.Data, character, ui.indexOfData);
                uiSummonDialog.Show();
            }
        }

        protected void OnDeselectCharacterSummon(UICharacterSummon ui)
        {
            if (uiSummonDialog != null)
                uiSummonDialog.Hide();
        }

        public void UpdateData(ICharacterData character)
        {
            this.character = character;

            var selectedSummonObjectId = SelectionManager.SelectedUI != null ? SelectionManager.SelectedUI.CharacterSummon.objectId : 0;
            SelectionManager.DeselectSelectedUI();
            SelectionManager.Clear();

            if (character == null)
            {
                CacheList.HideAll();
                return;
            }

            var stackingSkillSummons = new Dictionary<int, UICharacterSummon>();
            var summons = character.Summons;
            CacheList.Generate(summons, (index, characterSummon, ui) =>
            {
                if (characterSummon.type == SummonType.Skill && stackingSkillSummons.ContainsKey(characterSummon.dataId))
                {
                    stackingSkillSummons[characterSummon.dataId].AddStackingEntry(characterSummon);
                    ui.gameObject.SetActive(false);
                }
                else
                {
                    var uiCharacterSummon = ui.GetComponent<UICharacterSummon>();
                    uiCharacterSummon.Setup(characterSummon, character, index);
                    uiCharacterSummon.Show();
                    switch (characterSummon.type)
                    {
                        case SummonType.Skill:
                            stackingSkillSummons.Add(characterSummon.dataId, uiCharacterSummon);
                            break;
                        case SummonType.Pet:
                            ui.transform.SetAsFirstSibling();
                            break;
                    }
                    SelectionManager.Add(uiCharacterSummon);
                    if (selectedSummonObjectId == characterSummon.objectId)
                        uiCharacterSummon.OnClickSelect();
                }
            });
        }
    }
}
