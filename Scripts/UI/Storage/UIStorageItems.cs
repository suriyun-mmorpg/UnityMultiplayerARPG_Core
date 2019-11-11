using System.Collections;
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

        private UIList cacheCharacterItemList;
        public UIList CacheCharacterItemList
        {
            get
            {
                if (cacheCharacterItemList == null)
                {
                    cacheCharacterItemList = gameObject.AddComponent<UIList>();
                    cacheCharacterItemList.uiPrefab = uiCharacterItemPrefab.gameObject;
                    cacheCharacterItemList.uiContainer = uiCharacterItemContainer;
                }
                return cacheCharacterItemList;
            }
        }

        private UICharacterItemSelectionManager cacheCharacterItemSelectionManager;
        public UICharacterItemSelectionManager CacheCharacterItemSelectionManager
        {
            get
            {
                if (cacheCharacterItemSelectionManager == null)
                    cacheCharacterItemSelectionManager = GetComponent<UICharacterItemSelectionManager>();
                if (cacheCharacterItemSelectionManager == null)
                    cacheCharacterItemSelectionManager = gameObject.AddComponent<UICharacterItemSelectionManager>();
                cacheCharacterItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheCharacterItemSelectionManager;
            }
        }

        public StorageType storageType { get; private set; }
        public short weightLimit { get; private set; }
        public short slotLimit { get; private set; }
        public float totalWeight { get; private set; }
        public short usedSlots { get; private set; }

        public override void Show()
        {
            CacheCharacterItemSelectionManager.eventOnSelected.RemoveListener(OnSelectCharacterItem);
            CacheCharacterItemSelectionManager.eventOnSelected.AddListener(OnSelectCharacterItem);
            CacheCharacterItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselectCharacterItem);
            CacheCharacterItemSelectionManager.eventOnDeselected.AddListener(OnDeselectCharacterItem);
            base.Show();
        }

        public void Show(StorageType storageType, short weightLimit, short slotLimit)
        {
            this.storageType = storageType;
            this.weightLimit = weightLimit;
            this.slotLimit = slotLimit;
            Show();
        }

        public override void Hide()
        {
            // Close storage
            if (storageType != StorageType.None)
                BasePlayerCharacterController.OwningCharacter.RequestCloseStorage();
            // Clear data
            storageType = StorageType.None;
            weightLimit = 0;
            slotLimit = 0;
            // Hide
            CacheCharacterItemSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            if (ui.Data.characterItem.IsEmptySlot())
            {
                CacheCharacterItemSelectionManager.DeselectSelectedUI();
                return;
            }
            if (uiItemDialog != null)
            {
                uiItemDialog.selectionManager = CacheCharacterItemSelectionManager;
                uiItemDialog.Setup(ui.Data, BasePlayerCharacterController.OwningCharacter, ui.IndexOfData);
                uiItemDialog.Show();
            }
        }

        protected void OnDeselectCharacterItem(UICharacterItem ui)
        {
            if (uiItemDialog != null)
                uiItemDialog.Hide();
        }

        private void Update()
        {
            if (uiTextWeightLimit != null)
            {
                if (weightLimit <= 0)
                    uiTextWeightLimit.text = LanguageManager.GetText(UITextKeys.UI_LABEL_UNLIMIT_WEIGHT.ToString());
                else
                    uiTextWeightLimit.text = string.Format(LanguageManager.GetText(formatKeyWeightLimit), totalWeight.ToString("N2"), weightLimit.ToString("N2"));
            }

            if (uiTextSlotLimit != null)
            {
                if (slotLimit <= 0)
                    uiTextSlotLimit.text = LanguageManager.GetText(UITextKeys.UI_LABEL_UNLIMIT_SLOT.ToString());
                else
                    uiTextSlotLimit.text = string.Format(LanguageManager.GetText(formatKeySlotLimit), usedSlots.ToString("N0"), slotLimit.ToString("N0"));
            }
        }

        public void UpdateData()
        {
            if (storageType == StorageType.None)
                return;

            int selectedIdx = CacheCharacterItemSelectionManager.SelectedUI != null ? CacheCharacterItemSelectionManager.IndexOf(CacheCharacterItemSelectionManager.SelectedUI) : -1;
            CacheCharacterItemSelectionManager.DeselectSelectedUI();
            CacheCharacterItemSelectionManager.Clear();
            totalWeight = 0;
            usedSlots = 0;
            IList<CharacterItem> characterItems = BasePlayerCharacterController.OwningCharacter.StorageItems;
            UICharacterItem tempUiCharacterItem;
            CacheCharacterItemList.Generate(characterItems, (index, characterItem, ui) =>
            {
                tempUiCharacterItem = ui.GetComponent<UICharacterItem>();
                tempUiCharacterItem.Setup(new UICharacterItemData(characterItem, characterItem.level, InventoryType.StorageItems), BasePlayerCharacterController.OwningCharacter, index);
                tempUiCharacterItem.Show();
                if (characterItem.NotEmptySlot())
                {
                    totalWeight += characterItem.GetItem().weight * characterItem.amount;
                    usedSlots++;
                }
                UICharacterItemDragHandler dragHandler = tempUiCharacterItem.GetComponentInChildren<UICharacterItemDragHandler>();
                if (dragHandler != null)
                    dragHandler.SetupForStorageItems(tempUiCharacterItem);
                CacheCharacterItemSelectionManager.Add(tempUiCharacterItem);
                if (selectedIdx == index)
                    tempUiCharacterItem.OnClickSelect();
            });
        }
    }
}
