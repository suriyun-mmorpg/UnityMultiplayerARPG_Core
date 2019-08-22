using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UINonEquipItems : UIBase
    {
        public ICharacterData character { get; protected set; }
        public UICharacterItem uiItemDialog;
        public UICharacterItem uiCharacterItemPrefab;
        public List<string> filterCategories;
        public List<ItemType> filterItemTypes;
        public Transform uiCharacterItemContainer;

        private UIList cacheItemList;
        public UIList CacheItemList
        {
            get
            {
                if (cacheItemList == null)
                {
                    cacheItemList = gameObject.AddComponent<UIList>();
                    cacheItemList.uiPrefab = uiCharacterItemPrefab.gameObject;
                    cacheItemList.uiContainer = uiCharacterItemContainer;
                }
                return cacheItemList;
            }
        }

        private UICharacterItemSelectionManager cacheItemSelectionManager;
        public UICharacterItemSelectionManager CacheItemSelectionManager
        {
            get
            {
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = GetComponent<UICharacterItemSelectionManager>();
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = gameObject.AddComponent<UICharacterItemSelectionManager>();
                cacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheItemSelectionManager;
            }
        }

        public override void Show()
        {
            CacheItemSelectionManager.eventOnSelected.RemoveListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnSelected.AddListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselectCharacterItem);
            CacheItemSelectionManager.eventOnDeselected.AddListener(OnDeselectCharacterItem);
            if (uiItemDialog != null)
                uiItemDialog.onHide.AddListener(OnItemDialogHide);
            base.Show();
        }

        public override void Hide()
        {
            if (uiItemDialog != null)
                uiItemDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnItemDialogHide()
        {
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            if (ui.Data.characterItem.IsEmptySlot())
            {
                CacheItemSelectionManager.DeselectSelectedUI();
                return;
            }
            if (uiItemDialog != null)
            {
                uiItemDialog.selectionManager = CacheItemSelectionManager;
                uiItemDialog.Setup(ui.Data, character, ui.IndexOfData);
                uiItemDialog.Show();
            }
        }

        protected void OnDeselectCharacterItem(UICharacterItem ui)
        {
            if (uiItemDialog != null)
            {
                uiItemDialog.onHide.RemoveListener(OnItemDialogHide);
                uiItemDialog.Hide();
                uiItemDialog.onHide.AddListener(OnItemDialogHide);
            }
        }

        public void UpdateData(ICharacterData character)
        {
            this.character = character;
            int selectedIdx = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.IndexOf(CacheItemSelectionManager.SelectedUI) : -1;
            CacheItemSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.Clear();

            if (character == null)
            {
                CacheItemList.HideAll();
                return;
            }

            IList<CharacterItem> nonEquipItems = character.NonEquipItems;
            IList<CharacterItem> filteredItems = new List<CharacterItem>();
            List<int> filterIndexes = new List<int>();
            // Filter items to show by specific item types
            Item item;
            int counter = 0;
            foreach (CharacterItem nonEquipItem in nonEquipItems)
            {
                item = nonEquipItem.GetItem();
                if (!GameInstance.Singleton.IsLimitInventorySlot ||
                    (filterCategories != null && filterCategories.Count > 0) ||
                    (filterItemTypes != null && filterItemTypes.Count > 0))
                {
                    if (item == null)
                    {
                        ++counter;
                        continue;
                    }
                }

                if (item == null ||
                    string.IsNullOrEmpty(item.category) ||
                    filterCategories == null || filterCategories.Count == 0 ||
                    filterCategories.Contains(item.category))
                {
                    if (filterItemTypes == null || filterItemTypes.Count == 0 ||
                        filterItemTypes.Contains(item.itemType))
                    {
                        filteredItems.Add(nonEquipItem);
                        filterIndexes.Add(counter);
                    }
                }
                ++counter;
            }
            CacheItemList.Generate(filteredItems, (index, characterItem, ui) =>
            {
                UICharacterItem uiCharacterItem = ui.GetComponent<UICharacterItem>();
                uiCharacterItem.Setup(new CharacterItemTuple(characterItem, characterItem.level, InventoryType.NonEquipItems), this.character, filterIndexes[index]);
                uiCharacterItem.Show();
                UICharacterItemDragHandler dragHandler = uiCharacterItem.GetComponentInChildren<UICharacterItemDragHandler>();
                if (dragHandler != null)
                    dragHandler.SetupForNonEquipItems(uiCharacterItem);
                CacheItemSelectionManager.Add(uiCharacterItem);
                if (selectedIdx == index)
                    uiCharacterItem.OnClickSelect();
            });
        }
    }
}
