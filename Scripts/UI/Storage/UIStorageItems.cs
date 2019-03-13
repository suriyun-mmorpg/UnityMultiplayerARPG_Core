using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIStorageItems : UIBase
    {
        [Header("Generic Info Format")]
        [Tooltip("Weight Limit Stats Format => {0} = {Current Total Weights}, {1} = {Weight Limit}")]
        public string weightLimitFormat = "Weight: {0}/{1}";
        [Tooltip("Slot Limit Stats Format => {0} = {Current Used Slots}, {1} = {Slot Limit}")]
        public string slotLimitFormat = "Used Slot: {0}/{1}";
        [Tooltip("This text will be shown when it is not limit weight")]
        public string unlimitWeightText = "Unlimit";
        [Tooltip("This text will be shown when it is not limit slot")]
        public string unlimitSlotText = "Unlimit";

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
            CacheCharacterItemSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
            CacheCharacterItemSelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
            CacheCharacterItemSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterItem);
            CacheCharacterItemSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterItem);
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
            BasePlayerCharacterController.OwningCharacter.RequestCloseStorage();
            CacheCharacterItemSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
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
                    uiTextWeightLimit.text = unlimitWeightText;
                else
                    uiTextWeightLimit.text = string.Format(weightLimitFormat, totalWeight.ToString("N2"), weightLimit.ToString("N2"));
            }

            if (uiTextSlotLimit != null)
            {
                if (slotLimit <= 0)
                    uiTextSlotLimit.text = unlimitSlotText;
                else
                    uiTextSlotLimit.text = string.Format(slotLimitFormat, usedSlots.ToString("N0"), slotLimit.ToString("N0"));
            }
        }

        public void UpdateData()
        {
            int selectedIdx = CacheCharacterItemSelectionManager.SelectedUI != null ? CacheCharacterItemSelectionManager.IndexOf(CacheCharacterItemSelectionManager.SelectedUI) : -1;
            CacheCharacterItemSelectionManager.DeselectSelectedUI();
            CacheCharacterItemSelectionManager.Clear();
            totalWeight = 0;
            usedSlots = 0;
            IList<CharacterItem> characterItems = BasePlayerCharacterController.OwningCharacter.StorageItems;
            CacheCharacterItemList.Generate(characterItems, (index, characterItem, ui) =>
            {
                UICharacterItem uiCharacterItem = ui.GetComponent<UICharacterItem>();
                uiCharacterItem.Setup(new CharacterItemTuple(characterItem, characterItem.level, InventoryType.StorageItems), BasePlayerCharacterController.OwningCharacter, index);
                uiCharacterItem.Show();
                if (!characterItem.NotEmptySlot())
                {
                    totalWeight += characterItem.GetItem().weight * characterItem.amount;
                    usedSlots++;
                }
                UICharacterItemDragHandler dragHandler = uiCharacterItem.GetComponentInChildren<UICharacterItemDragHandler>();
                if (dragHandler != null)
                    dragHandler.SetupForStorageItems(uiCharacterItem);
                CacheCharacterItemSelectionManager.Add(uiCharacterItem);
                if (selectedIdx == index)
                    uiCharacterItem.OnClickSelect();
            });
        }
    }
}
