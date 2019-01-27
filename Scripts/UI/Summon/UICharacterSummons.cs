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

        private UIList cacheCharacterSummonList;
        public UIList CacheCharacterSummonList
        {
            get
            {
                if (cacheCharacterSummonList == null)
                {
                    cacheCharacterSummonList = gameObject.AddComponent<UIList>();
                    cacheCharacterSummonList.uiPrefab = uiCharacterSummonPrefab.gameObject;
                    cacheCharacterSummonList.uiContainer = uiCharacterSummonContainer;
                }
                return cacheCharacterSummonList;
            }
        }

        private UICharacterSummonSelectionManager cacheCharacterSummonSelectionManager;
        public UICharacterSummonSelectionManager CacheCharacterSummonSelectionManager
        {
            get
            {
                if (cacheCharacterSummonSelectionManager == null)
                    cacheCharacterSummonSelectionManager = GetComponent<UICharacterSummonSelectionManager>();
                if (cacheCharacterSummonSelectionManager == null)
                    cacheCharacterSummonSelectionManager = gameObject.AddComponent<UICharacterSummonSelectionManager>();
                cacheCharacterSummonSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheCharacterSummonSelectionManager;
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
            CacheCharacterSummonSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterSummon);
            CacheCharacterSummonSelectionManager.eventOnSelect.AddListener(OnSelectCharacterSummon);
            CacheCharacterSummonSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterSummon);
            CacheCharacterSummonSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterSummon);
            base.Show();
        }

        public override void Hide()
        {
            CacheCharacterSummonSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterSummon(UICharacterSummon ui)
        {
            if (uiSummonDialog != null)
            {
                uiSummonDialog.selectionManager = CacheCharacterSummonSelectionManager;
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

            uint selectedSummonObjectId = CacheCharacterSummonSelectionManager.SelectedUI != null ? CacheCharacterSummonSelectionManager.SelectedUI.CharacterSummon.objectId : 0;
            CacheCharacterSummonSelectionManager.DeselectSelectedUI();
            CacheCharacterSummonSelectionManager.Clear();

            if (character == null)
            {
                CacheCharacterSummonList.HideAll();
                return;
            }

            Dictionary<int, UICharacterSummon> stackingSkillSummons = new Dictionary<int, UICharacterSummon>();
            IList<CharacterSummon> summons = character.Summons;
            CacheCharacterSummonList.Generate(summons, (index, characterSummon, ui) =>
            {
                if (characterSummon.type == SummonType.Skill && stackingSkillSummons.ContainsKey(characterSummon.dataId))
                {
                    stackingSkillSummons[characterSummon.dataId].AddStackingEntry(characterSummon);
                    ui.gameObject.SetActive(false);
                }
                else
                {
                    UICharacterSummon uiCharacterSummon = ui.GetComponent<UICharacterSummon>();
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
                    CacheCharacterSummonSelectionManager.Add(uiCharacterSummon);
                    if (selectedSummonObjectId == characterSummon.objectId)
                        uiCharacterSummon.OnClickSelect();
                }
            });
        }
    }
}
