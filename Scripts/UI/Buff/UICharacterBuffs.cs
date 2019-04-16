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

        private UIList cacheBuffList;
        public UIList CacheBuffList
        {
            get
            {
                if (cacheBuffList == null)
                {
                    cacheBuffList = gameObject.AddComponent<UIList>();
                    cacheBuffList.uiPrefab = uiCharacterBuffPrefab.gameObject;
                    cacheBuffList.uiContainer = uiCharacterBuffContainer;
                }
                return cacheBuffList;
            }
        }

        private UICharacterBuffSelectionManager cacheBuffSelectionManager;
        public UICharacterBuffSelectionManager CacheBuffSelectionManager
        {
            get
            {
                if (cacheBuffSelectionManager == null)
                    cacheBuffSelectionManager = GetComponent<UICharacterBuffSelectionManager>();
                if (cacheBuffSelectionManager == null)
                    cacheBuffSelectionManager = gameObject.AddComponent<UICharacterBuffSelectionManager>();
                cacheBuffSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheBuffSelectionManager;
            }
        }

        public override void Show()
        {
            CacheBuffSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterBuff);
            CacheBuffSelectionManager.eventOnSelect.AddListener(OnSelectCharacterBuff);
            CacheBuffSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterBuff);
            CacheBuffSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterBuff);
            if (uiBuffDialog != null)
                uiBuffDialog.onHide.AddListener(OnBuffDialogHide);
            base.Show();
        }

        public override void Hide()
        {
            if (uiBuffDialog != null)
                uiBuffDialog.onHide.RemoveListener(OnBuffDialogHide);
            CacheBuffSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnBuffDialogHide()
        {
            CacheBuffSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCharacterBuff(UICharacterBuff ui)
        {
            if (uiBuffDialog != null)
            {
                uiBuffDialog.selectionManager = CacheBuffSelectionManager;
                uiBuffDialog.Setup(ui.Data, character, ui.IndexOfData);
                uiBuffDialog.Show();
            }
        }

        protected void OnDeselectCharacterBuff(UICharacterBuff ui)
        {
            if (uiBuffDialog != null)
            {
                uiBuffDialog.onHide.RemoveListener(OnBuffDialogHide);
                uiBuffDialog.Hide();
                uiBuffDialog.onHide.AddListener(OnBuffDialogHide);
            }
        }

        public void UpdateData(ICharacterData character)
        {
            this.character = character;
            string selectedBuffKey = CacheBuffSelectionManager.SelectedUI != null ? CacheBuffSelectionManager.SelectedUI.CharacterBuff.GetKey() : string.Empty;
            CacheBuffSelectionManager.DeselectSelectedUI();
            CacheBuffSelectionManager.Clear();

            if (character == null)
            {
                CacheBuffList.HideAll();
                return;
            }

            IList<CharacterBuff> buffs = character.Buffs;
            CacheBuffList.Generate(buffs, (index, characterBuff, ui) =>
            {
                UICharacterBuff uiCharacterBuff = ui.GetComponent<UICharacterBuff>();
                uiCharacterBuff.Setup(characterBuff, character, index);
                uiCharacterBuff.Show();
                CacheBuffSelectionManager.Add(uiCharacterBuff);
                if (selectedBuffKey.Equals(characterBuff.GetKey()))
                    uiCharacterBuff.OnClickSelect();
            });
        }
    }
}
