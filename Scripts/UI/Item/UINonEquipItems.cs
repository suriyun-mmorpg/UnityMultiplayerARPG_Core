using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterItemSelectionManager))]
    public partial class UINonEquipItems : UIBase
    {
        public ICharacterData character { get; protected set; }
        public UICharacterItem uiItemDialog;
        public UICharacterItem uiCharacterItemPrefab;
        public Transform uiCharacterItemContainer;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiCharacterItemPrefab.gameObject;
                    cacheList.uiContainer = uiCharacterItemContainer;
                }
                return cacheList;
            }
        }

        private UICharacterItemSelectionManager selectionManager;
        public UICharacterItemSelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UICharacterItemSelectionManager>();
                selectionManager.selectionMode = UISelectionMode.SelectSingle;
                return selectionManager;
            }
        }

        public override void Show()
        {
            SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
            SelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
            SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterItem);
            SelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterItem);
            base.Show();
        }

        public override void Hide()
        {
            SelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            if (uiItemDialog != null && ui.Data.characterItem.IsValid())
            {
                uiItemDialog.selectionManager = SelectionManager;
                uiItemDialog.Setup(ui.Data, character, ui.indexOfData);
                uiItemDialog.Show();
            }
        }

        protected void OnDeselectCharacterItem(UICharacterItem ui)
        {
            if (uiItemDialog != null)
                uiItemDialog.Hide();
        }

        public void UpdateData(ICharacterData character)
        {
            this.character = character;
            int selectedIdx = SelectionManager.SelectedUI != null ? SelectionManager.IndexOf(SelectionManager.SelectedUI) : -1;
            SelectionManager.DeselectSelectedUI();
            SelectionManager.Clear();

            if (character == null)
            {
                CacheList.HideAll();
                return;
            }

            IList<CharacterItem> nonEquipItems = character.NonEquipItems;
            CacheList.Generate(nonEquipItems, (index, characterItem, ui) =>
            {
                UICharacterItem uiCharacterItem = ui.GetComponent<UICharacterItem>();
                uiCharacterItem.Setup(new CharacterItemTuple(characterItem, characterItem.level, InventoryType.NonEquipItems), this.character, index);
                uiCharacterItem.Show();
                SelectionManager.Add(uiCharacterItem);
                if (selectedIdx == index)
                    uiCharacterItem.OnClickSelect();
            });
        }
    }
}
