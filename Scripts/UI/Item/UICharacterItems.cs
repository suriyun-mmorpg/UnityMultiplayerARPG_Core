using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterItems : UIBase
    {
        [Header("Filter")]
        public List<string> filterCategories;
        public List<ItemType> filterItemTypes;

        [Header("UI Elements")]
        public GameObject listEmptyObject;
        [FormerlySerializedAs("uiItemDialog")]
        public UICharacterItem uiDialog;
        [FormerlySerializedAs("uiCharacterItemPrefab")]
        public UICharacterItem uiPrefab;
        [FormerlySerializedAs("uiCharacterItemContainer")]
        public Transform uiContainer;
        public bool isUnknowSource;

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

        public virtual ICharacterData Character { get; set; }

        private UISelectionMode dirtySelectionMode;

        protected virtual void OnEnable()
        {
            CacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
            CacheItemSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheItemSelectionManager.eventOnSelected.AddListener(OnSelect);
            CacheItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselect);
            CacheItemSelectionManager.eventOnDeselected.AddListener(OnDeselect);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnItemDialogHide);
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnItemDialogHide()
        {
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnSelect(UICharacterItem ui)
        {
            if (ui.Data.characterItem.IsEmptySlot())
            {
                CacheItemSelectionManager.DeselectSelectedUI();
                return;
            }
            if (uiDialog != null && CacheItemSelectionManager.selectionMode == UISelectionMode.SelectSingle)
            {
                uiDialog.selectionManager = CacheItemSelectionManager;
                uiDialog.Setup(ui.Data, Character, ui.IndexOfData);
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICharacterItem ui)
        {
            if (uiDialog != null && CacheItemSelectionManager.selectionMode == UISelectionMode.SelectSingle)
            {
                uiDialog.onHide.RemoveListener(OnItemDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnItemDialogHide);
            }
        }

        public virtual void UpdateData(ICharacterData character, IList<CharacterItem> characterItems)
        {
            Character = character;
            string selectedId = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.CharacterItem.id : string.Empty;
            CacheItemSelectionManager.Clear();

            if (character == null || characterItems == null || characterItems.Count == 0)
            {
                if (uiDialog != null)
                    uiDialog.Hide();
                CacheItemList.HideAll();
                if (listEmptyObject != null)
                    listEmptyObject.SetActive(false);
                return;
            }

            int showingCount = 0;
            UICharacterItem selectedUI = null;
            UICharacterItem tempUI;
            BaseItem tempItem;
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
                        showingCount++;
                    }
                    else
                    {
                        // Hide because item's type not matches in the filter list
                        tempUI.Hide();
                    }
                }
                else
                {
                    // Hide because item's category not matches in the filter list
                    tempUI.Hide();
                }
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(showingCount == 0);
            if (selectedUI == null)
            {
                CacheItemSelectionManager.DeselectSelectedUI();
            }
            else
            {
                bool defaultDontShowComparingEquipments = uiDialog != null ? uiDialog.dontShowComparingEquipments : false;
                if (uiDialog != null)
                    uiDialog.dontShowComparingEquipments = true;
                selectedUI.OnClickSelect();
                if (uiDialog != null)
                    uiDialog.dontShowComparingEquipments = defaultDontShowComparingEquipments;
            }
        }

        protected virtual void Update()
        {
            if (CacheItemSelectionManager.selectionMode != dirtySelectionMode)
            {
                CacheItemSelectionManager.DeselectAll();
                dirtySelectionMode = CacheItemSelectionManager.selectionMode;
                if (uiDialog != null)
                {
                    uiDialog.onHide.RemoveListener(OnItemDialogHide);
                    uiDialog.Hide();
                    uiDialog.onHide.AddListener(OnItemDialogHide);
                }
            }
        }
    }
}
