using LiteNetLibManager;
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
                    cacheItemSelectionManager = gameObject.GetOrAddComponent<UICharacterItemSelectionManager>();
                cacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheItemSelectionManager;
            }
        }

        protected virtual void OnEnable()
        {
            CacheItemSelectionManager.eventOnSelected.RemoveListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnSelected.AddListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselectCharacterItem);
            CacheItemSelectionManager.eventOnDeselected.AddListener(OnDeselectCharacterItem);
            if (uiItemDialog != null)
                uiItemDialog.onHide.AddListener(OnItemDialogHide);
            UpdateOwningCharacterData();
            if (!BasePlayerCharacterController.OwningCharacter) return;
            BasePlayerCharacterController.OwningCharacter.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        protected virtual void OnDisable()
        {
            if (uiItemDialog != null)
                uiItemDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
            if (!BasePlayerCharacterController.OwningCharacter) return;
            BasePlayerCharacterController.OwningCharacter.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
        }

        private void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        private void UpdateOwningCharacterData()
        {
            if (!BasePlayerCharacterController.OwningCharacter) return;
            UpdateData(BasePlayerCharacterController.OwningCharacter);
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
            string selectedId = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.CharacterItem.id : string.Empty;
            CacheItemSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.Clear();

            if (character == null)
            {
                CacheItemList.HideAll();
                return;
            }
            
            // Filter items to show by specific item types
            BaseItem tempItem;
            UICharacterItem tempUiCharacterItem;
            CacheItemList.Generate(character.NonEquipItems, (index, nonEquipItem, ui) =>
            {
                tempUiCharacterItem = ui.GetComponent<UICharacterItem>();
                tempItem = nonEquipItem.GetItem();
                if (!GameInstance.Singleton.IsLimitInventorySlot ||
                    (filterCategories != null && filterCategories.Count > 0) ||
                    (filterItemTypes != null && filterItemTypes.Count > 0))
                {
                    // If inventory type isn't limit inventory slot, hide empty slot
                    if (tempItem == null)
                    {
                        tempUiCharacterItem.Hide();
                        return;
                    }
                }

                if (tempItem == null ||
                    string.IsNullOrEmpty(tempItem.category) ||
                    filterCategories == null || filterCategories.Count == 0 ||
                    filterCategories.Contains(tempItem.category))
                {
                    if (filterItemTypes == null || filterItemTypes.Count == 0 ||
                        filterItemTypes.Contains(tempItem.ItemType))
                    {
                        tempUiCharacterItem.Setup(new UICharacterItemData(nonEquipItem, InventoryType.NonEquipItems), this.character, index);
                        tempUiCharacterItem.Show();
                        UICharacterItemDragHandler dragHandler = tempUiCharacterItem.GetComponentInChildren<UICharacterItemDragHandler>();
                        if (dragHandler != null)
                            dragHandler.SetupForNonEquipItems(tempUiCharacterItem);
                        CacheItemSelectionManager.Add(tempUiCharacterItem);
                        if (!string.IsNullOrEmpty(selectedId) &&  selectedId.Equals(nonEquipItem.id))
                            tempUiCharacterItem.OnClickSelect();
                    }
                    else
                    {
                        tempUiCharacterItem.Hide();
                    }
                }
                else
                {
                    tempUiCharacterItem.Hide();
                }
            });
        }
    }
}
