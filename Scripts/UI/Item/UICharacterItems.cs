using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterItems : UIBase
    {
        [Header("Filter")]
        public List<string> filterCategories = new List<string>();
        public List<ItemType> filterItemTypes = new List<ItemType>();
        public bool doNotShowEmptySlots;

        [Header("UI Elements")]
        public GameObject listEmptyObject;
        [FormerlySerializedAs("uiItemDialog")]
        public UICharacterItem uiDialog;
        [FormerlySerializedAs("uiCharacterItemPrefab")]
        public UICharacterItem uiPrefab;
        [FormerlySerializedAs("uiCharacterItemContainer")]
        public Transform uiContainer;
        public InventoryType inventoryType = InventoryType.NonEquipItems;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiPrefab.gameObject;
                    cacheList.uiContainer = uiContainer;
                }
                return cacheList;
            }
        }

        private UICharacterItemSelectionManager cacheSelectionManager;
        public UICharacterItemSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UICharacterItemSelectionManager>();
                return cacheSelectionManager;
            }
        }

        public virtual ICharacterData Character { get; protected set; }
        public List<CharacterItem> LoadedList { get; private set; } = new List<CharacterItem>();

        private UISelectionMode dirtySelectionMode;

        protected virtual void OnEnable()
        {
            CacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelect);
            CacheSelectionManager.eventOnDeselected.RemoveListener(OnDeselect);
            CacheSelectionManager.eventOnDeselected.AddListener(OnDeselect);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnDialogHide);
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnSelect(UICharacterItem ui)
        {
            if (ui.Data.characterItem.IsEmptySlot())
            {
                CacheSelectionManager.DeselectSelectedUI();
                return;
            }
            if (uiDialog != null && CacheSelectionManager.selectionMode == UISelectionMode.SelectSingle)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.Setup(ui.Data, Character, ui.IndexOfData);
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICharacterItem ui)
        {
            if (uiDialog != null && CacheSelectionManager.selectionMode == UISelectionMode.SelectSingle)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public virtual void UpdateData(ICharacterData character, IList<CharacterItem> characterItems)
        {
            Character = character;
            LoadedList.Clear();
            if (characterItems != null && characterItems.Count > 0)
                LoadedList.AddRange(characterItems);
            GenerateList();
        }

        public virtual void GenerateList()
        {
            string selectedId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.CharacterItem.id : string.Empty;
            CacheSelectionManager.Clear();
            ConvertFilterCategoriesToTrimedLowerChar();

            if (Character == null || LoadedList.Count == 0)
            {
                if (uiDialog != null)
                    uiDialog.Hide();
                CacheList.HideAll();
                if (listEmptyObject != null)
                    listEmptyObject.SetActive(true);
                return;
            }

            int showingCount = 0;
            UICharacterItem selectedUI = null;
            UICharacterItem tempUI;
            BaseItem tempItem;
            CacheList.Generate(LoadedList, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UICharacterItem>();
                tempItem = data.GetItem();

                if (!GameInstance.Singleton.IsLimitInventorySlot || doNotShowEmptySlots ||
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
                    (filterCategories.Count == 0 || (!string.IsNullOrEmpty(tempItem.Category) &&
                    filterCategories.Contains(tempItem.Category.Trim().ToLower()))))
                {
                    if (filterItemTypes.Count == 0 ||
                        filterItemTypes.Contains(tempItem.ItemType))
                    {
                        tempUI.Setup(new UICharacterItemData(data, inventoryType), Character, index);
                        tempUI.Show();
                        UICharacterItemDragHandler dragHandler = tempUI.GetComponentInChildren<UICharacterItemDragHandler>();
                        if (dragHandler != null)
                        {
                            switch (inventoryType)
                            {
                                case InventoryType.NonEquipItems:
                                    dragHandler.SetupForNonEquipItems(tempUI);
                                    break;
                                case InventoryType.EquipItems:
                                case InventoryType.EquipWeaponRight:
                                case InventoryType.EquipWeaponLeft:
                                    dragHandler.SetupForEquipItems(tempUI);
                                    break;
                                case InventoryType.StorageItems:
                                    dragHandler.SetupForStorageItems(tempUI);
                                    break;
                                case InventoryType.Unknow:
                                    dragHandler.SetupForUnknow(tempUI);
                                    break;
                            }
                        }
                        CacheSelectionManager.Add(tempUI);
                        if (!string.IsNullOrEmpty(selectedId) && selectedId.Equals(data.id))
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
                CacheSelectionManager.DeselectSelectedUI();
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

        protected void ConvertFilterCategoriesToTrimedLowerChar()
        {
            for (int i = 0; i < filterCategories.Count; ++i)
            {
                filterCategories[i] = filterCategories[i].Trim().ToLower();
            }
        }

        protected virtual void Update()
        {
            if (CacheSelectionManager.selectionMode != dirtySelectionMode)
            {
                CacheSelectionManager.DeselectAll();
                dirtySelectionMode = CacheSelectionManager.selectionMode;
                if (uiDialog != null)
                {
                    uiDialog.onHide.RemoveListener(OnDialogHide);
                    uiDialog.Hide();
                    uiDialog.onHide.AddListener(OnDialogHide);
                }
            }
        }
    }
}
