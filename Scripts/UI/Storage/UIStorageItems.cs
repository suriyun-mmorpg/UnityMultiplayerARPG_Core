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
        public BaseGameEntity TargetEntity { get; private set; }
        public short WeightLimit { get; private set; }
        public short SlotLimit { get; private set; }
        public float TotalWeight { get; private set; }
        public short UsedSlots { get; private set; }

        protected virtual void OnEnable()
        {
            CacheItemSelectionManager.eventOnSelected.RemoveListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnSelected.AddListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselectCharacterItem);
            CacheItemSelectionManager.eventOnDeselected.AddListener(OnDeselectCharacterItem);
            if (uiItemDialog != null)
                uiItemDialog.onHide.AddListener(OnItemDialogHide);
            UpdateData();
            if (!BasePlayerCharacterController.OwningCharacter) return;
            BasePlayerCharacterController.OwningCharacter.onStorageItemsChange += OnStorageItemsChange;
        }

        protected virtual void OnDisable()
        {
            // Close storage
            if (StorageType != StorageType.None && BasePlayerCharacterController.OwningCharacter)
                BasePlayerCharacterController.OwningCharacter.CallServerCloseStorage();
            // Clear data
            StorageType = StorageType.None;
            TargetEntity = null;
            WeightLimit = 0;
            SlotLimit = 0;
            // Hide
            if (uiItemDialog != null)
                uiItemDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
            if (!BasePlayerCharacterController.OwningCharacter) return;
            BasePlayerCharacterController.OwningCharacter.onStorageItemsChange -= OnStorageItemsChange;
        }

        private void OnStorageItemsChange(CharacterItem[] storageItems)
        {
            UpdateData();
        }

        public void Show(StorageType storageType, BaseGameEntity targetEntity, short weightLimit, short slotLimit)
        {
            StorageType = storageType;
            TargetEntity = targetEntity;
            WeightLimit = weightLimit;
            SlotLimit = slotLimit;
            Show();
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
                uiItemDialog.Setup(ui.Data, BasePlayerCharacterController.OwningCharacter, ui.IndexOfData);
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

        public void UpdateData()
        {
            if (StorageType == StorageType.None)
                return;
            string selectedId = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.CharacterItem.id : string.Empty;
            CacheItemSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.Clear();
            TotalWeight = 0;
            UsedSlots = 0;
            IList<CharacterItem> characterItems = BasePlayerCharacterController.OwningCharacter.StorageItems;
            UICharacterItem tempUiCharacterItem;
            CacheItemList.Generate(characterItems, (index, characterItem, ui) =>
            {
                tempUiCharacterItem = ui.GetComponent<UICharacterItem>();
                tempUiCharacterItem.Setup(new UICharacterItemData(characterItem, InventoryType.StorageItems), BasePlayerCharacterController.OwningCharacter, index);
                tempUiCharacterItem.Show();
                if (characterItem.NotEmptySlot())
                {
                    TotalWeight += characterItem.GetItem().Weight * characterItem.amount;
                    UsedSlots++;
                }
                UICharacterItemDragHandler dragHandler = tempUiCharacterItem.GetComponentInChildren<UICharacterItemDragHandler>();
                if (dragHandler != null)
                    dragHandler.SetupForStorageItems(tempUiCharacterItem);
                CacheItemSelectionManager.Add(tempUiCharacterItem);
                if (!string.IsNullOrEmpty(selectedId) && selectedId.Equals(characterItem.id))
                    tempUiCharacterItem.OnClickSelect();
            });
        }

        // TODO: Add function to 
    }
}
