using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterItems : UIBase
    {
        public UICharacterItem uiItemDialog;
        public List<string> filterCategories;
        public List<ItemType> filterItemTypes;
        [FormerlySerializedAs("uiCharacterItemPrefab")]
        public UICharacterItem uiPrefab;
        [FormerlySerializedAs("uiCharacterItemContainer")]
        public Transform uiContainer;
        public bool isUnknowSource;

        public virtual ICharacterData Character { get; set; }

        private UIList cacheItemList;
        public UIList CacheItemList
        {
            get
            {
                if (cacheItemList == null)
                {
                    cacheItemList = gameObject.AddComponent<UIList>();
                    cacheItemList.uiPrefab = uiPrefab.gameObject;
                    cacheItemList.uiContainer = uiContainer;
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
                return cacheItemSelectionManager;
            }
        }

        private UISelectionMode dirtySelectionMode;

        protected virtual void OnEnable()
        {
            CacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
            CacheItemSelectionManager.eventOnSelected.RemoveListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnSelected.AddListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselectCharacterItem);
            CacheItemSelectionManager.eventOnDeselected.AddListener(OnDeselectCharacterItem);
            if (uiItemDialog != null)
                uiItemDialog.onHide.AddListener(OnItemDialogHide);
        }

        protected virtual void OnDisable()
        {
            if (uiItemDialog != null)
                uiItemDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnItemDialogHide()
        {
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnSelectCharacterItem(UICharacterItem ui)
        {
            if (ui.Data.characterItem.IsEmptySlot())
            {
                CacheItemSelectionManager.DeselectSelectedUI();
                return;
            }
            if (uiItemDialog != null && CacheItemSelectionManager.selectionMode == UISelectionMode.SelectSingle)
            {
                uiItemDialog.selectionManager = CacheItemSelectionManager;
                uiItemDialog.Setup(ui.Data, Character, ui.IndexOfData);
                uiItemDialog.Show();
            }
        }

        protected virtual void OnDeselectCharacterItem(UICharacterItem ui)
        {
            if (uiItemDialog != null && CacheItemSelectionManager.selectionMode == UISelectionMode.SelectSingle)
            {
                uiItemDialog.onHide.RemoveListener(OnItemDialogHide);
                uiItemDialog.Hide();
                uiItemDialog.onHide.AddListener(OnItemDialogHide);
            }
        }

        public virtual void UpdateData(ICharacterData character, IList<CharacterItem> characterItems)
        {
            Character = character;
            string selectedId = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.CharacterItem.id : string.Empty;
            CacheItemSelectionManager.Clear();

            if (character == null || characterItems == null || characterItems.Count == 0)
            {
                if (uiItemDialog != null)
                    uiItemDialog.Hide();
                CacheItemList.HideAll();
                return;
            }

            // Filter items to show by specific item types
            BaseItem tempItem;
            UICharacterItem selectedUI = null;
            UICharacterItem tempUI;
            CacheItemList.Generate(characterItems, (index, characterItem, ui) =>
            {
                tempUI = ui.GetComponent<UICharacterItem>();
                tempItem = characterItem.GetItem();
                if (!GameInstance.Singleton.IsLimitInventorySlot ||
                    (filterCategories != null && filterCategories.Count > 0) ||
                    (filterItemTypes != null && filterItemTypes.Count > 0))
                {
                    // If inventory type isn't limit inventory slot, hide empty slot
                    if (tempItem == null)
                    {
                        tempUI.Hide();
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
                        tempUI.Setup(new UICharacterItemData(characterItem, isUnknowSource ? InventoryType.Unknow : InventoryType.NonEquipItems), Character, index);
                        tempUI.Show();
                        UICharacterItemDragHandler dragHandler = tempUI.GetComponentInChildren<UICharacterItemDragHandler>();
                        if (dragHandler != null)
                        {
                            if (isUnknowSource)
                                dragHandler.SetupForUnknow(tempUI);
                            else
                                dragHandler.SetupForNonEquipItems(tempUI);
                        }
                        CacheItemSelectionManager.Add(tempUI);
                        if (!string.IsNullOrEmpty(selectedId) && selectedId.Equals(characterItem.id))
                            selectedUI = tempUI;
                    }
                    else
                    {
                        tempUI.Hide();
                    }
                }
                else
                {
                    tempUI.Hide();
                }
            });
            if (selectedUI == null)
            {
                CacheItemSelectionManager.DeselectSelectedUI();
            }
            else
            {
                bool defaultDontShowComparingEquipments = uiItemDialog != null ? uiItemDialog.dontShowComparingEquipments : false;
                if (uiItemDialog != null)
                    uiItemDialog.dontShowComparingEquipments = true;
                selectedUI.OnClickSelect();
                if (uiItemDialog != null)
                    uiItemDialog.dontShowComparingEquipments = defaultDontShowComparingEquipments;
            }
        }

        protected virtual void Update()
        {
            if (CacheItemSelectionManager.selectionMode != dirtySelectionMode)
            {
                CacheItemSelectionManager.DeselectAll();
                dirtySelectionMode = CacheItemSelectionManager.selectionMode;
                if (uiItemDialog != null)
                {
                    uiItemDialog.onHide.RemoveListener(OnItemDialogHide);
                    uiItemDialog.Hide();
                    uiItemDialog.onHide.AddListener(OnItemDialogHide);
                }
            }
        }
    }
}
