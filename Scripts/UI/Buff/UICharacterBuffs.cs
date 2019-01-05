using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterBuffSelectionManager))]
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

        private UICharacterBuffSelectionManager selectionManager;
        public UICharacterBuffSelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UICharacterBuffSelectionManager>();
                selectionManager.selectionMode = UISelectionMode.SelectSingle;
                return selectionManager;
            }
        }

        public override void Show()
        {
            SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterBuff);
            SelectionManager.eventOnSelect.AddListener(OnSelectCharacterBuff);
            SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterBuff);
            SelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterBuff);
            base.Show();
        }

        public override void Hide()
        {
            SelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterBuff(UICharacterBuff ui)
        {
            if (uiBuffDialog != null)
            {
                uiBuffDialog.selectionManager = SelectionManager;
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
            
            string selectedBuffKey = SelectionManager.SelectedUI != null ? SelectionManager.SelectedUI.CharacterBuff.GetKey() : string.Empty;
            SelectionManager.DeselectSelectedUI();
            SelectionManager.Clear();

            if (character == null)
            {
                CacheList.HideAll();
                return;
            }

            IList<CharacterBuff> buffs = character.Buffs;
            CacheList.Generate(buffs, (index, characterBuff, ui) =>
            {
                UICharacterBuff uiCharacterBuff = ui.GetComponent<UICharacterBuff>();
                uiCharacterBuff.Setup(characterBuff, character, index);
                uiCharacterBuff.Show();
                SelectionManager.Add(uiCharacterBuff);
                if (selectedBuffKey.Equals(characterBuff.GetKey()))
                    uiCharacterBuff.OnClickSelect();
            });
        }
    }
}
