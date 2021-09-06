using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public class UIStorageItems : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Current Total Weights}, {1} = {Weight Limit}")]
        public UILocaleKeySetting formatKeyWeightLimit = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_WEIGHT);
        [Tooltip("Format => {0} = {Current Used Slots}, {1} = {Slot Limit}")]
        public UILocaleKeySetting formatKeySlotLimit = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_SLOT);

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
        public TextWrapper uiTextWeightLimit;
        public TextWrapper uiTextSlotLimit;

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
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSelectionManager;
            }
        }

        public StorageType StorageType { get; private set; }
        public string StorageOwnerId { get; private set; }
        public BaseGameEntity TargetEntity { get; private set; }
        public short WeightLimit { get; private set; }
        public short SlotLimit { get; private set; }
        public float TotalWeight { get; private set; }
        public short UsedSlots { get; private set; }
        public List<CharacterItem> LoadedList { get; private set; } = new List<CharacterItem>();

        private bool doNotCloseStorageOnDisable;

        protected virtual void OnEnable()
        {
            ClientStorageActions.onNotifyStorageItemsUpdated += UpdateData;
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelect);
            CacheSelectionManager.eventOnDeselected.RemoveListener(OnDeselect);
            CacheSelectionManager.eventOnDeselected.AddListener(OnDeselect);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnDialogHide);
        }

        protected virtual void OnDisable()
        {
            ClientStorageActions.onNotifyStorageItemsUpdated -= UpdateData;
            // Close storage
            if (!doNotCloseStorageOnDisable)
                GameInstance.ClientStorageHandlers.RequestCloseStorage(ClientStorageActions.ResponseCloseStorage);
            doNotCloseStorageOnDisable = false;
            // Clear data
            StorageType = StorageType.None;
            StorageOwnerId = string.Empty;
            TargetEntity = null;
            WeightLimit = 0;
            SlotLimit = 0;
            // Hide
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
        }

        public override void Hide()
        {
            doNotCloseStorageOnDisable = false;
            base.Hide();
        }

        public void Hide(bool doNotCloseStorageOnDisable)
        {
            this.doNotCloseStorageOnDisable = doNotCloseStorageOnDisable;
            base.Hide();
        }

        protected virtual void Update()
        {
            if (uiTextWeightLimit != null)
            {
                if (WeightLimit <= 0)
                    uiTextWeightLimit.text = LanguageManager.GetText(UITextKeys.UI_LABEL_UNLIMIT_WEIGHT.ToString());
                else
                    uiTextWeightLimit.text = string.Format(LanguageManager.GetText(formatKeyWeightLimit), TotalWeight.ToString("N2"), WeightLimit.ToString("N2"));
            }

            if (uiTextSlotLimit != null)
            {
                if (SlotLimit <= 0)
                    uiTextSlotLimit.text = LanguageManager.GetText(UITextKeys.UI_LABEL_UNLIMIT_SLOT.ToString());
                else
                    uiTextSlotLimit.text = string.Format(LanguageManager.GetText(formatKeySlotLimit), UsedSlots.ToString("N0"), SlotLimit.ToString("N0"));
            }
        }

        public void Show(StorageType storageType, string storageOwnerId, BaseGameEntity targetEntity, short weightLimit, short slotLimit)
        {
            StorageType = storageType;
            StorageOwnerId = storageOwnerId;
            TargetEntity = targetEntity;
            WeightLimit = weightLimit;
            SlotLimit = slotLimit;
            Show();
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
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.Setup(ui.Data, GameInstance.PlayingCharacter, ui.IndexOfData);
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICharacterItem ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public virtual void UpdateData(IList<CharacterItem> characterItems)
        {
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

            TotalWeight = 0;
            UsedSlots = 0;

            if (LoadedList.Count == 0)
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

                if (data.NotEmptySlot())
                {
                    // Increase total weight and used slots, if data is not empty slot
                    TotalWeight += data.GetItem().Weight * data.amount;
                    UsedSlots++;
                }

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
                    if (filterItemTypes == null || filterItemTypes.Count == 0 ||
                        filterItemTypes.Contains(tempItem.ItemType))
                    {
                        tempUI.Setup(new UICharacterItemData(data, InventoryType.StorageItems), GameInstance.PlayingCharacter, index);
                        tempUI.Show();
                        UICharacterItemDragHandler dragHandler = tempUI.GetComponentInChildren<UICharacterItemDragHandler>();
                        if (dragHandler != null)
                            dragHandler.SetupForStorageItems(tempUI);
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
    }
}
