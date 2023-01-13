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

        public System.Action<int, CharacterItem> onGenerateEntry = null;
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
            if (uiDialog != null && (CacheSelectionManager.selectionMode == UISelectionMode.SelectSingle ||
                CacheSelectionManager.selectionMode == UISelectionMode.Toggle))
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.Setup(ui.Data, Character, ui.IndexOfData);
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICharacterItem ui)
        {
            if (uiDialog != null && (CacheSelectionManager.selectionMode == UISelectionMode.SelectSingle ||
                CacheSelectionManager.selectionMode == UISelectionMode.Toggle))
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public void UpdateData(ICharacterData character, IDictionary<BaseItem, int> items)
        {
            Character = character;
            LoadedList.Clear();
            foreach (KeyValuePair<BaseItem, int> item in items)
            {
                LoadedList.Add(CharacterItem.Create(item.Key, 1, item.Value));
            }
            GenerateList();
        }

        public void UpdateData(ICharacterData character, IDictionary<int, int> items)
        {
            Character = character;
            LoadedList.Clear();
            BaseItem tempItem;
            foreach (KeyValuePair<int, int> item in items)
            {
                if (GameInstance.Items.TryGetValue(item.Key, out tempItem))
                    LoadedList.Add(CharacterItem.Create(tempItem, 1, item.Value));
            }
            GenerateList();
        }

        public void UpdateData(ICharacterData character, IList<ItemAmount> items)
        {
            Character = character;
            LoadedList.Clear();
            foreach (ItemAmount item in items)
            {
                LoadedList.Add(CharacterItem.Create(item.item, 1, item.amount));
            }
            GenerateList();
        }

        public void UpdateData(ICharacterData character, IList<RewardedItem> items)
        {
            Character = character;
            LoadedList.Clear();
            foreach (RewardedItem item in items)
            {
                LoadedList.Add(CharacterItem.Create(item.item, item.level, item.amount, item.randomSeed));
            }
            GenerateList();
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
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            List<KeyValuePair<int, CharacterItem>> filteredList = UICharacterItemsUtils.GetFilteredList(LoadedList, filterCategories, filterItemTypes, doNotShowEmptySlots);
            if (Character == null || filteredList.Count == 0)
            {
                if (uiDialog != null)
                    uiDialog.Hide();
                CacheList.HideAll();
                if (listEmptyObject != null)
                    listEmptyObject.SetActive(true);
                return;
            }

            if (listEmptyObject != null)
                listEmptyObject.SetActive(false);

            UICharacterItem selectedUI = null;
            UICharacterItem tempUI;
            CacheList.Generate(filteredList, (index, data, ui) =>
            {
                if (onGenerateEntry != null)
                    onGenerateEntry.Invoke(data.Key, data.Value);
                tempUI = ui.GetComponent<UICharacterItem>();
                tempUI.Setup(new UICharacterItemData(data.Value, inventoryType), Character, data.Key);
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
                if (!string.IsNullOrEmpty(selectedId) && selectedId.Equals(data.Value.id))
                    selectedUI = tempUI;
            });

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
