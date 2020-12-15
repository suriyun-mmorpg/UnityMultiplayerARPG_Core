using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIStorageItems : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Current Total Weights}, {1} = {Weight Limit}")]
        public UILocaleKeySetting formatKeyWeightLimit = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_WEIGHT);
        [Tooltip("Format => {0} = {Current Used Slots}, {1} = {Slot Limit}")]
        public UILocaleKeySetting formatKeySlotLimit = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_SLOT);

        [Header("UI Elements")]
        public UICharacterItem uiItemDialog;
        public UICharacterItem uiCharacterItemPrefab;
        public Transform uiCharacterItemContainer;
        public TextWrapper uiTextWeightLimit;
        public TextWrapper uiTextSlotLimit;

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

        public StorageType StorageType { get; private set; }
        public string StorageOwnerId { get; private set; }
        public BaseGameEntity TargetEntity { get; private set; }
        public short WeightLimit { get; private set; }
        public short SlotLimit { get; private set; }
        public float TotalWeight { get; private set; }
        public short UsedSlots { get; private set; }

        protected virtual void OnEnable()
        {
            ClientStorageActions.onNotifyStorageItemsUpdated += UpdateData;
            CacheItemSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheItemSelectionManager.eventOnSelected.AddListener(OnSelect);
            CacheItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselect);
            CacheItemSelectionManager.eventOnDeselected.AddListener(OnDeselect);
            if (uiItemDialog != null)
                uiItemDialog.onHide.AddListener(OnDialogHide);
        }

        protected virtual void OnDisable()
        {
            ClientStorageActions.onNotifyStorageItemsUpdated -= UpdateData;
            // Close storage
            if (StorageType != StorageType.None && BasePlayerCharacterController.OwningCharacter)
                BasePlayerCharacterController.OwningCharacter.CallServerCloseStorage();
            // Clear data
            StorageType = StorageType.None;
            StorageOwnerId = string.Empty;
            TargetEntity = null;
            WeightLimit = 0;
            SlotLimit = 0;
            // Hide
            if (uiItemDialog != null)
                uiItemDialog.onHide.RemoveListener(OnDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
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

        protected void OnDialogHide()
        {
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelect(UICharacterItem ui)
        {
            if (ui.Data.characterItem.IsEmptySlot())
            {
                CacheItemSelectionManager.DeselectSelectedUI();
                return;
            }
            if (uiItemDialog != null)
            {
                uiItemDialog.selectionManager = CacheItemSelectionManager;
                uiItemDialog.Setup(ui.Data, BasePlayerCharacterController.OwningCharacter, ui.IndexOfData);
                uiItemDialog.Show();
            }
        }

        protected void OnDeselect(UICharacterItem ui)
        {
            if (uiItemDialog != null)
            {
                uiItemDialog.onHide.RemoveListener(OnDialogHide);
                uiItemDialog.Hide();
                uiItemDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public void UpdateData(IList<CharacterItem> characterItems)
        {
            string selectedId = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.CharacterItem.id : string.Empty;
            CacheItemSelectionManager.Clear();

            TotalWeight = 0;
            UsedSlots = 0;

            if (characterItems == null || characterItems.Count == 0)
            {
                if (uiItemDialog != null)
                    uiItemDialog.Hide();
                CacheItemList.HideAll();
                return;
            }

            UICharacterItem selectedUI = null;
            UICharacterItem tempUI;
            CacheItemList.Generate(characterItems, (index, characterItem, ui) =>
            {
                tempUI = ui.GetComponent<UICharacterItem>();
                tempUI.Setup(new UICharacterItemData(characterItem, InventoryType.StorageItems), BasePlayerCharacterController.OwningCharacter, index);
                tempUI.Show();
                if (characterItem.NotEmptySlot())
                {
                    TotalWeight += characterItem.GetItem().Weight * characterItem.amount;
                    UsedSlots++;
                }
                UICharacterItemDragHandler dragHandler = tempUI.GetComponentInChildren<UICharacterItemDragHandler>();
                if (dragHandler != null)
                    dragHandler.SetupForStorageItems(tempUI);
                CacheItemSelectionManager.Add(tempUI);
                if (!string.IsNullOrEmpty(selectedId) && selectedId.Equals(characterItem.id))
                    selectedUI = tempUI;
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
    }
}
