using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class UIEquipItems : UIBase
    {
        public ICharacterData character { get; protected set; }
        public UICharacterItem uiItemDialog;
        public UICharacterItem rightHandSlot;
        public UICharacterItem leftHandSlot;
        public UICharacterItemPair[] otherEquipSlots;

        private Dictionary<string, UICharacterItem> cacheEquipItemSlots = null;
        public Dictionary<string, UICharacterItem> CacheEquipItemSlots
        {
            get
            {
                if (cacheEquipItemSlots == null)
                {
                    cacheEquipItemSlots = new Dictionary<string, UICharacterItem>();
                    CacheItemSelectionManager.Clear();
                    if (rightHandSlot != null)
                    {
                        string equipPosition = GameDataConst.EQUIP_POSITION_RIGHT_HAND;
                        rightHandSlot.Setup(GetEmptyUIData(InventoryType.EquipWeaponRight), character, -1);
                        UICharacterItemDragHandler dragHandler = rightHandSlot.GetComponentInChildren<UICharacterItemDragHandler>();
                        if (dragHandler != null)
                            dragHandler.SetupForEquipItems(rightHandSlot);
                        cacheEquipItemSlots.Add(equipPosition, rightHandSlot);
                        CacheItemSelectionManager.Add(rightHandSlot);
                    }
                    if (leftHandSlot != null)
                    {
                        string equipPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
                        leftHandSlot.Setup(GetEmptyUIData(InventoryType.EquipWeaponLeft), character, -1);
                        UICharacterItemDragHandler dragHandler = leftHandSlot.GetComponentInChildren<UICharacterItemDragHandler>();
                        if (dragHandler != null)
                            dragHandler.SetupForEquipItems(leftHandSlot);
                        cacheEquipItemSlots.Add(equipPosition, leftHandSlot);
                        CacheItemSelectionManager.Add(leftHandSlot);
                    }
                    foreach (UICharacterItemPair otherEquipSlot in otherEquipSlots)
                    {
                        if (!string.IsNullOrEmpty(otherEquipSlot.armorType.Id) &&
                            otherEquipSlot.ui != null &&
                            !cacheEquipItemSlots.ContainsKey(otherEquipSlot.armorType.Id))
                        {
                            string equipPosition = otherEquipSlot.armorType.Id;
                            otherEquipSlot.ui.Setup(GetEmptyUIData(InventoryType.EquipItems), character, -1);
                            UICharacterItemDragHandler dragHandler = otherEquipSlot.ui.GetComponentInChildren<UICharacterItemDragHandler>();
                            if (dragHandler != null)
                                dragHandler.SetupForEquipItems(otherEquipSlot.ui);
                            cacheEquipItemSlots.Add(equipPosition, otherEquipSlot.ui);
                            CacheItemSelectionManager.Add(otherEquipSlot.ui);
                        }
                    }
                }
                return cacheEquipItemSlots;
            }
        }

        private UICharacterItemSelectionManager cacheItemSelectionManager;
        public UICharacterItemSelectionManager CacheItemSelectionManager
        {
            get
            {
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = GetComponent<UICharacterItemSelectionManager>();
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = gameObject.AddComponent<UICharacterItemSelectionManager>();
                cacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheItemSelectionManager;
            }
        }

        public override void Show()
        {
            CacheItemSelectionManager.eventOnSelected.RemoveListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnSelected.AddListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselectCharacterItem);
            CacheItemSelectionManager.eventOnDeselected.AddListener(OnDeselectCharacterItem);
            if (uiItemDialog != null)
                uiItemDialog.onHide.AddListener(OnItemDialogHide);
            base.Show();
        }

        public override void Hide()
        {
            if (uiItemDialog != null)
                uiItemDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnItemDialogHide()
        {
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            if (!ui.Data.characterItem.NotEmptySlot())
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
            // Clear slots data
            foreach (UICharacterItem slot in CacheEquipItemSlots.Values)
            {
                slot.Setup(GetEmptyUIData(slot.InventoryType), this.character, -1);
                slot.Show();
            }

            if (character == null)
                return;

            string tempPosition;
            UICharacterItem tempSlot;
            IList<CharacterItem> equipItems = character.EquipItems;
            for (int i = 0; i < equipItems.Count; ++i)
            {
                CharacterItem equipItem = equipItems[i];
                Item armorItem = equipItem.GetArmorItem();
                if (armorItem == null)
                    continue;

                tempPosition = armorItem.EquipPosition;
                if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
                    tempSlot.Setup(new CharacterItemTuple(equipItem, equipItem.level, InventoryType.EquipItems), this.character, i);
            }

            EquipWeapons equipWeapons = character.EquipWeapons;
            CharacterItem rightHand = equipWeapons.rightHand;
            CharacterItem leftHand = equipWeapons.leftHand;
            Item rightHandEquipment = rightHand.GetEquipmentItem();
            Item leftHandEquipment = leftHand.GetEquipmentItem();
            tempPosition = GameDataConst.EQUIP_POSITION_RIGHT_HAND;
            if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
            {
                if (rightHandEquipment != null)
                    tempSlot.Setup(new CharacterItemTuple(rightHand, rightHand.level, InventoryType.EquipWeaponRight), this.character, 0);
            }
            tempPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
            if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
            {
                if (leftHandEquipment != null)
                    tempSlot.Setup(new CharacterItemTuple(leftHand, leftHand.level, InventoryType.EquipWeaponLeft), this.character, 0);
            }
        }

        private CharacterItemTuple GetEmptyUIData(InventoryType inventoryType)
        {
            return new CharacterItemTuple(CharacterItem.Empty, 1, inventoryType);
        }
    }
}
