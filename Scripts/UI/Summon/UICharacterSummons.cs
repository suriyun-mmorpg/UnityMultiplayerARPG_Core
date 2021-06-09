using LiteNetLibManager;
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

        private UICharacterSummonSelectionManager cacheSelectionManager;
        public UICharacterSummonSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UICharacterSummonSelectionManager>();
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
            if (uiSummonDialog != null)
                uiSummonDialog.onHide.AddListener(OnDialogHide);
            UpdateOwningCharacterData();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onSummonsOperation += OnSummonsOperation;
        }

        protected virtual void OnDisable()
        {
            if (uiSummonDialog != null)
                uiSummonDialog.onHide.RemoveListener(OnDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onSummonsOperation -= OnSummonsOperation;
        }

        private void OnSummonsOperation(LiteNetLibSyncList.Operation operation, int index)
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

        protected virtual void OnSelect(UICharacterSummon ui)
        {
            if (uiSummonDialog != null)
            {
                uiSummonDialog.selectionManager = CacheSelectionManager;
                uiSummonDialog.Setup(ui.Data, character, ui.IndexOfData);
                uiSummonDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICharacterSummon ui)
        {
            if (uiSummonDialog != null)
            {
                uiSummonDialog.onHide.RemoveListener(OnDialogHide);
                uiSummonDialog.Hide();
                uiSummonDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public virtual void UpdateData(ICharacterData character)
        {
            this.character = character;
            uint selectedSummonObjectId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.CharacterSummon.objectId : 0;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            Dictionary<int, UICharacterSummon> stackingSkillSummons = new Dictionary<int, UICharacterSummon>();
            UICharacterSummon tempUiCharacterSummon;
            CacheList.Generate(character.Summons, (index, characterSummon, ui) =>
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
                        case SummonType.PetItem:
                            ui.transform.SetAsFirstSibling();
                            break;
                    }
                    CacheSelectionManager.Add(tempUiCharacterSummon);
                    if (selectedSummonObjectId == characterSummon.objectId)
                        tempUiCharacterSummon.OnClickSelect();
                }
            });
        }
    }
}
