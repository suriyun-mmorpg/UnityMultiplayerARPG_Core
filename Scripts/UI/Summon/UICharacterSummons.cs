using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterSummons : UIBase
    {
        [FormerlySerializedAs("uiSummonDialog")]
        public UICharacterSummon uiDialog;
        [FormerlySerializedAs("uiCharacterSummonPrefab")]
        public UICharacterSummon uiPrefab;
        [FormerlySerializedAs("uiCharacterSummonContainer")]
        public Transform uiContainer;

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

        public ICharacterData Character { get; protected set; }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelect.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelect.AddListener(OnSelect);
            CacheSelectionManager.eventOnDeselect.RemoveListener(OnDeselect);
            CacheSelectionManager.eventOnDeselect.AddListener(OnDeselect);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnDialogHide);
            UpdateOwningCharacterData();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onSummonsOperation += OnSummonsOperation;
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnDialogHide);
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
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.Setup(ui.Data, Character, ui.IndexOfData);
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICharacterSummon ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public virtual void UpdateData(ICharacterData character)
        {
            Character = character;
            uint selectedSummonObjectId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.CharacterSummon.objectId : 0;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            Dictionary<int, UICharacterSummon> stackingSkillSummons = new Dictionary<int, UICharacterSummon>();
            UICharacterSummon tempUI;
            CacheList.Generate(character.Summons, (index, data, ui) =>
            {
                if (data.type == SummonType.Skill && stackingSkillSummons.ContainsKey(data.dataId))
                {
                    stackingSkillSummons[data.dataId].AddStackingEntry(data);
                    ui.gameObject.SetActive(false);
                }
                else
                {
                    tempUI = ui.GetComponent<UICharacterSummon>();
                    tempUI.Setup(data, character, index);
                    tempUI.Show();
                    switch (data.type)
                    {
                        case SummonType.Skill:
                            stackingSkillSummons.Add(data.dataId, tempUI);
                            break;
                        case SummonType.PetItem:
                            ui.transform.SetAsFirstSibling();
                            break;
                    }
                    CacheSelectionManager.Add(tempUI);
                    if (selectedSummonObjectId == data.objectId)
                        tempUI.OnClickSelect();
                }
            });
        }
    }
}
