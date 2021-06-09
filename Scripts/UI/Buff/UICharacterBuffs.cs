using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterBuffs : UIBase
    {
        public ICharacterData character { get; protected set; }
        public UICharacterBuff uiBuffDialog;
        public UICharacterBuff uiCharacterBuffPrefab;
        public Transform uiCharacterBuffContainer;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiCharacterBuffPrefab.gameObject;
                    cacheList.uiContainer = uiCharacterBuffContainer;
                }
                return cacheList;
            }
        }

        private UICharacterBuffSelectionManager cacheSelectionManager;
        public UICharacterBuffSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UICharacterBuffSelectionManager>();
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
            if (uiBuffDialog != null)
                uiBuffDialog.onHide.AddListener(OnDialogHide);
        }

        protected virtual void OnDisable()
        {
            if (uiBuffDialog != null)
                uiBuffDialog.onHide.RemoveListener(OnDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnSelect(UICharacterBuff ui)
        {
            if (uiBuffDialog != null)
            {
                uiBuffDialog.selectionManager = CacheSelectionManager;
                uiBuffDialog.Setup(ui.Data, character, ui.IndexOfData);
                uiBuffDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICharacterBuff ui)
        {
            if (uiBuffDialog != null)
            {
                uiBuffDialog.onHide.RemoveListener(OnDialogHide);
                uiBuffDialog.Hide();
                uiBuffDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public virtual void UpdateData(ICharacterData character)
        {
            this.character = character;
            string selectedBuffKey = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.CharacterBuff.GetKey() : string.Empty;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            if (character == null)
            {
                CacheList.HideAll();
                return;
            }

            UICharacterBuff tempUiCharacterBuff;
            CacheList.Generate(character.Buffs, (index, characterBuff, ui) =>
            {
                tempUiCharacterBuff = ui.GetComponent<UICharacterBuff>();
                if (characterBuff.buffRemainsDuration > 0)
                {
                    tempUiCharacterBuff.Setup(characterBuff, character, index);
                    tempUiCharacterBuff.Show();
                    CacheSelectionManager.Add(tempUiCharacterBuff);
                    if (selectedBuffKey.Equals(characterBuff.GetKey()))
                        tempUiCharacterBuff.OnClickSelect();
                }
                else
                {
                    tempUiCharacterBuff.Hide();
                }
            });
        }
    }
}
