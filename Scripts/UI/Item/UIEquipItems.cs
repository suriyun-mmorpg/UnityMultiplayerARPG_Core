using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class UIEquipItems : UIBase
    {
        public ICharacterData character { get; protected set; }
        public UICharacterItem uiItemDialog;
        public UICharacterItem rightHandSlot;
        public UICharacterItem leftHandSlot;
        public UICharacterItem rightHandSlot2;
        public UICharacterItem leftHandSlot2;
        public UICharacterItemPair[] otherEquipSlots;

        private Dictionary<string, UICharacterItem> cacheEquipItemSlots;
        public Dictionary<string, UICharacterItem> CacheEquipItemSlots
        {
            get
            {
                if (cacheEquipItemSlots == null)
                {
                    cacheEquipItemSlots = new Dictionary<string, UICharacterItem>();
                    CacheItemSelectionManager.Clear();
                    // Equip weapons
                    CacheEquipWeaponSlots(rightHandSlot, leftHandSlot, 0);
                    CacheEquipWeaponSlots(rightHandSlot2, leftHandSlot2, 1);
                    // Armor equipments
                    byte tempEquipSlotIndex;
                    string tempEquipPosition;
                    foreach (UICharacterItemPair otherEquipSlot in otherEquipSlots)
                    {
                        tempEquipSlotIndex = otherEquipSlot.equipSlotIndex;
                        tempEquipPosition = GetEquipPosition(otherEquipSlot.armorType.Id, tempEquipSlotIndex);
                        if (!string.IsNullOrEmpty(tempEquipPosition) &&
                            otherEquipSlot.ui != null &&
                            !cacheEquipItemSlots.ContainsKey(tempEquipPosition))
                        {
                            otherEquipSlot.ui.Setup(GetEmptyUIData(InventoryType.EquipItems, tempEquipSlotIndex), character, -1);
                            UICharacterItemDragHandler dragHandler = otherEquipSlot.ui.GetComponentInChildren<UICharacterItemDragHandler>();
                            if (dragHandler != null)
                                dragHandler.SetupForEquipItems(otherEquipSlot.ui);
                            cacheEquipItemSlots.Add(tempEquipPosition, otherEquipSlot.ui);
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
            if (ui.Data.characterItem.IsEmptySlot())
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
            UICharacterItem equipSlot;
            foreach (string equipPosition in CacheEquipItemSlots.Keys)
            {
                equipSlot = CacheEquipItemSlots[equipPosition];
                equipSlot.Setup(GetEmptyUIData(equipSlot.InventoryType, GetEquipSlotIndexFromEquipPosition(equipPosition)), character, -1);
                equipSlot.Show();
            }

            if (character == null)
                return;

            CharacterItem tempEquipItem;
            Item tempArmorItem;
            UICharacterItem tempSlot;
            IList<CharacterItem> equipItems = character.EquipItems;
            for (int i = 0; i < equipItems.Count; ++i)
            {
                tempEquipItem = equipItems[i];
                tempArmorItem = tempEquipItem.GetArmorItem();
                if (tempArmorItem == null)
                    continue;
                
                if (CacheEquipItemSlots.TryGetValue(GetEquipPosition(tempArmorItem.EquipPosition, tempEquipItem.equipSlotIndex), out tempSlot))
                    tempSlot.Setup(new CharacterItemTuple(tempEquipItem, tempEquipItem.level, InventoryType.EquipItems), character, i);
            }

            SetEquipWeapons(character.EquipWeapons, 0);
            SetEquipWeapons(character.EquipWeapons2, 1);
        }

        private void CacheEquipWeaponSlots(UICharacterItem rightHandSlot, UICharacterItem leftHandSlot, byte equipSlotIndex)
        {
            CacheEquipWeaponSlot(rightHandSlot, false, equipSlotIndex);
            CacheEquipWeaponSlot(leftHandSlot, true, equipSlotIndex);
        }

        private void CacheEquipWeaponSlot(UICharacterItem slot, bool isLeftHand, byte equipSlotIndex)
        {
            if (slot == null)
                return;
            slot.Setup(GetEmptyUIData(isLeftHand ? InventoryType.EquipWeaponLeft : InventoryType.EquipWeaponRight, equipSlotIndex), character, -1);
            UICharacterItemDragHandler dragHandler = slot.GetComponentInChildren<UICharacterItemDragHandler>();
            if (dragHandler != null)
                dragHandler.SetupForEquipItems(slot);
            cacheEquipItemSlots.Add(GetEquipPosition(isLeftHand ? GameDataConst.EQUIP_POSITION_LEFT_HAND : GameDataConst.EQUIP_POSITION_RIGHT_HAND, equipSlotIndex), slot);
            CacheItemSelectionManager.Add(slot);
        }

        private void SetEquipWeapons(EquipWeapons equipWeapons, byte equipSlotIndex)
        {
            SetEquipWeapon(equipWeapons.rightHand, false, equipSlotIndex);
            SetEquipWeapon(equipWeapons.leftHand, true, equipSlotIndex);
        }

        private void SetEquipWeapon(CharacterItem equipWeapon, bool isLeftHand, byte equipSlotIndex)
        {
            string tempPosition = GetEquipPosition(isLeftHand ? GameDataConst.EQUIP_POSITION_LEFT_HAND : GameDataConst.EQUIP_POSITION_RIGHT_HAND, equipSlotIndex);
            UICharacterItem tempSlot;
            if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
            {
                if (equipWeapon.GetEquipmentItem() != null)
                    tempSlot.Setup(new CharacterItemTuple(equipWeapon, equipWeapon.level, isLeftHand ? InventoryType.EquipWeaponLeft : InventoryType.EquipWeaponRight), character, 0);
            }
        }

        private CharacterItemTuple GetEmptyUIData(InventoryType inventoryType, byte equipSlotIndex)
        {
            return new CharacterItemTuple(new CharacterItem()
            {
                equipSlotIndex = equipSlotIndex
            }, 1, inventoryType);
        }

        private string GetEquipPosition(string equipPositionId, byte equipSlotIndex)
        {
            return equipPositionId + ":" + equipSlotIndex;
        }

        private byte GetEquipSlotIndexFromEquipPosition(string equipPosition)
        {
            string[] splitEquipPosition = equipPosition.Split(':');
            return byte.Parse(splitEquipPosition[splitEquipPosition.Length - 1]);
        }
    }
}
