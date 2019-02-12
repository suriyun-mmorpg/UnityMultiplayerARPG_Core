using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UINonEquipItems : UIBase
    {
        public ICharacterData character { get; protected set; }
        public UICharacterItem uiItemDialog;
        public UICharacterItem uiCharacterItemPrefab;
        public List<ItemType> filterItemTypes;
        public Transform uiCharacterItemContainer;

        private UIList cacheNonEquipItemList;
        public UIList CacheNonEquipItemList
        {
            get
            {
                if (cacheNonEquipItemList == null)
                {
                    cacheNonEquipItemList = gameObject.AddComponent<UIList>();
                    cacheNonEquipItemList.uiPrefab = uiCharacterItemPrefab.gameObject;
                    cacheNonEquipItemList.uiContainer = uiCharacterItemContainer;
                }
                return cacheNonEquipItemList;
            }
        }

        private UICharacterItemSelectionManager cacheNonEquipItemSelectionManager;
        public UICharacterItemSelectionManager CacheNonEquipItemSelectionManager
        {
            get
            {
                if (cacheNonEquipItemSelectionManager == null)
                    cacheNonEquipItemSelectionManager = GetComponent<UICharacterItemSelectionManager>();
                if (cacheNonEquipItemSelectionManager == null)
                    cacheNonEquipItemSelectionManager = gameObject.AddComponent<UICharacterItemSelectionManager>();
                cacheNonEquipItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheNonEquipItemSelectionManager;
            }
        }

        public override void Show()
        {
            CacheNonEquipItemSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
            CacheNonEquipItemSelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
            CacheNonEquipItemSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterItem);
            CacheNonEquipItemSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterItem);
            base.Show();
        }

        public override void Hide()
        {
            CacheNonEquipItemSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            if (uiItemDialog != null && ui.Data.characterItem.IsValid())
            {
                uiItemDialog.selectionManager = CacheNonEquipItemSelectionManager;
                uiItemDialog.Setup(ui.Data, character, ui.IndexOfData);
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
            int selectedIdx = CacheNonEquipItemSelectionManager.SelectedUI != null ? CacheNonEquipItemSelectionManager.IndexOf(CacheNonEquipItemSelectionManager.SelectedUI) : -1;
            CacheNonEquipItemSelectionManager.DeselectSelectedUI();
            CacheNonEquipItemSelectionManager.Clear();

            if (character == null)
            {
                CacheNonEquipItemList.HideAll();
                return;
            }

            IList<CharacterItem> nonEquipItems = character.NonEquipItems;
            if (filterItemTypes != null && filterItemTypes.Count > 0)
            {
                // Filter items to show by specific item types
                IList<CharacterItem> filteredItems = new List<CharacterItem>();
                foreach (CharacterItem nonEquipItem in nonEquipItems)
                {
                    if (nonEquipItem.GetItem() == null) continue;
                    if (filterItemTypes.Contains(nonEquipItem.GetItem().itemType))
                        filteredItems.Add(nonEquipItem);
                }
                nonEquipItems = filteredItems;
            }
            CacheNonEquipItemList.Generate(nonEquipItems, (index, characterItem, ui) =>
            {
                UICharacterItem uiCharacterItem = ui.GetComponent<UICharacterItem>();
                uiCharacterItem.Setup(new CharacterItemTuple(characterItem, characterItem.level, InventoryType.NonEquipItems), this.character, index);
                uiCharacterItem.Show();
                UICharacterItemDragHandler dragHandler = uiCharacterItem.GetComponentInChildren<UICharacterItemDragHandler>();
                if (dragHandler != null)
                    dragHandler.SetupForNonEquipItems(uiCharacterItem);
                CacheNonEquipItemSelectionManager.Add(uiCharacterItem);
                if (selectedIdx == index)
                    uiCharacterItem.OnClickSelect();
            });
        }
    }
}
