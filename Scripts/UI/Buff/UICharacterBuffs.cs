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

        private UIList cacheCharacterBuffList;
        public UIList CacheCharacterBuffList
        {
            get
            {
                if (cacheCharacterBuffList == null)
                {
                    cacheCharacterBuffList = gameObject.AddComponent<UIList>();
                    cacheCharacterBuffList.uiPrefab = uiCharacterBuffPrefab.gameObject;
                    cacheCharacterBuffList.uiContainer = uiCharacterBuffContainer;
                }
                return cacheCharacterBuffList;
            }
        }

        private UICharacterBuffSelectionManager cacheCharacterBuffSelectionManager;
        public UICharacterBuffSelectionManager CacheCharacterBuffSelectionManager
        {
            get
            {
                if (cacheCharacterBuffSelectionManager == null)
                    cacheCharacterBuffSelectionManager = GetComponent<UICharacterBuffSelectionManager>();
                if (cacheCharacterBuffSelectionManager == null)
                    cacheCharacterBuffSelectionManager = gameObject.AddComponent<UICharacterBuffSelectionManager>();
                cacheCharacterBuffSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheCharacterBuffSelectionManager;
            }
        }

        public override void Show()
        {
            CacheCharacterBuffSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterBuff);
            CacheCharacterBuffSelectionManager.eventOnSelect.AddListener(OnSelectCharacterBuff);
            CacheCharacterBuffSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterBuff);
            CacheCharacterBuffSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterBuff);
            base.Show();
        }

        public override void Hide()
        {
            CacheCharacterBuffSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterBuff(UICharacterBuff ui)
        {
            if (uiBuffDialog != null)
            {
                uiBuffDialog.selectionManager = CacheCharacterBuffSelectionManager;
                uiBuffDialog.Setup(ui.Data, character, ui.indexOfData);
                uiBuffDialog.Show();
            }
        }

        protected void OnDeselectCharacterBuff(UICharacterBuff ui)
        {
            if (uiBuffDialog != null)
                uiBuffDialog.Hide();
        }

        public void UpdateData(ICharacterData character)
        {
            this.character = character;
            
            string selectedBuffKey = CacheCharacterBuffSelectionManager.SelectedUI != null ? CacheCharacterBuffSelectionManager.SelectedUI.CharacterBuff.GetKey() : string.Empty;
            CacheCharacterBuffSelectionManager.DeselectSelectedUI();
            CacheCharacterBuffSelectionManager.Clear();

            if (character == null)
            {
                CacheCharacterBuffList.HideAll();
                return;
            }

            IList<CharacterBuff> buffs = character.Buffs;
            CacheCharacterBuffList.Generate(buffs, (index, characterBuff, ui) =>
            {
                UICharacterBuff uiCharacterBuff = ui.GetComponent<UICharacterBuff>();
                uiCharacterBuff.Setup(characterBuff, character, index);
                uiCharacterBuff.Show();
                CacheCharacterBuffSelectionManager.Add(uiCharacterBuff);
                if (selectedBuffKey.Equals(characterBuff.GetKey()))
                    uiCharacterBuff.OnClickSelect();
            });
        }
    }
}
