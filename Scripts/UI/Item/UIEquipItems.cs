using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class UIEquipItems : UIBase
    {
        public ICharacterData character { get; protected set; }
        public UICharacterItem uiItemDialog;
        [HideInInspector] // TODO: This is deprecated, it will be removed later
        public UICharacterItem rightHandSlot;
        [HideInInspector] // TODO: This is deprecated, it will be removed later
        public UICharacterItem leftHandSlot;
        public UIEquipWeaponsPair[] equipWeaponSlots;
        public UIEquipItemPair[] otherEquipSlots;

        private Dictionary<string, UICharacterItem> cacheEquipItemSlots;
        public Dictionary<string, UICharacterItem> CacheEquipItemSlots
        {
            get
            {
                if (cacheEquipItemSlots == null)
                {
                    cacheEquipItemSlots = new Dictionary<string, UICharacterItem>();
                    CacheItemSelectionManager.Clear();
                    // Weapons
                    MigrateUIWeaponSlots();
                    foreach (UIEquipWeaponsPair currentEquipWeaponSlots in equipWeaponSlots)
                    {
                        CacheEquipWeaponSlots(currentEquipWeaponSlots.rightHandSlot, currentEquipWeaponSlots.leftHandSlot, currentEquipWeaponSlots.equipWeaponSetIndex);
                    }
                    // Armor equipments
                    byte tempEquipSlotIndex;
                    string tempEquipPosition;
                    foreach (UIEquipItemPair otherEquipSlot in otherEquipSlots)
                    {
                        tempEquipSlotIndex = otherEquipSlot.equipSlotIndex;
                        tempEquipPosition = GetEquipPosition(otherEquipSlot.armorType.Id, tempEquipSlotIndex);
                        if (!string.IsNullOrEmpty(tempEquipPosition) &&
                            otherEquipSlot.ui != null &&
                            !cacheEquipItemSlots.ContainsKey(tempEquipPosition))
                        {
                            otherEquipSlot.ui.Setup(CreateEmptyUIData(InventoryType.EquipItems), character, -1);
                            otherEquipSlot.ui.SetupAsEquipSlot(otherEquipSlot.armorType.Id, tempEquipSlotIndex);
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

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (MigrateUIWeaponSlots())
                EditorUtility.SetDirty(this);
#endif
        }

        private bool MigrateUIWeaponSlots()
        {
            bool hasChanges = false;
            if (equipWeaponSlots == null || equipWeaponSlots.Length == 0)
            {
                equipWeaponSlots = new UIEquipWeaponsPair[]
                {
                    new UIEquipWeaponsPair()
                    {
                        equipWeaponSetIndex = 0,
                        rightHandSlot = rightHandSlot,
                        leftHandSlot = leftHandSlot,
                    },
                };
                hasChanges = true;
            }
            return hasChanges;
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
                equipSlot.Setup(CreateEmptyUIData(equipSlot.InventoryType), character, -1);
            }

            if (character == null)
                return;

            CharacterItem tempEquipItem;
            Item tempArmorItem;
            UICharacterItem tempSlot;
            int i;
            for (i = 0; i < character.EquipItems.Count; ++i)
            {
                tempEquipItem = character.EquipItems[i];
                tempArmorItem = tempEquipItem.GetArmorItem();
                if (tempArmorItem == null)
                    continue;
                
                if (CacheEquipItemSlots.TryGetValue(GetEquipPosition(tempArmorItem.EquipPosition, tempEquipItem.equipSlotIndex), out tempSlot))
                    tempSlot.Setup(new UICharacterItemData(tempEquipItem, tempEquipItem.level, InventoryType.EquipItems), character, i);
            }

            for (i = 0; i < character.SelectableWeaponSets.Count; ++i)
            {
                SetEquipWeapons(character.SelectableWeaponSets[i], (byte)i);
            };
        }

        private void CacheEquipWeaponSlots(UICharacterItem rightHandSlot, UICharacterItem leftHandSlot, byte equipWeaponSet)
        {
            CacheEquipWeaponSlot(rightHandSlot, false, equipWeaponSet);
            CacheEquipWeaponSlot(leftHandSlot, true, equipWeaponSet);
        }

        private void CacheEquipWeaponSlot(UICharacterItem slot, bool isLeftHand, byte equipWeaponSet)
        {
            if (slot == null)
                return;
            slot.Setup(CreateEmptyUIData(isLeftHand ? InventoryType.EquipWeaponLeft : InventoryType.EquipWeaponRight), character, -1);
            slot.SetupAsEquipSlot(isLeftHand ? GameDataConst.EQUIP_POSITION_LEFT_HAND : GameDataConst.EQUIP_POSITION_RIGHT_HAND, equipWeaponSet);
            UICharacterItemDragHandler dragHandler = slot.GetComponentInChildren<UICharacterItemDragHandler>();
            if (dragHandler != null)
                dragHandler.SetupForEquipItems(slot);
            cacheEquipItemSlots.Add(GetEquipPosition(isLeftHand ? GameDataConst.EQUIP_POSITION_LEFT_HAND : GameDataConst.EQUIP_POSITION_RIGHT_HAND, equipWeaponSet), slot);
            CacheItemSelectionManager.Add(slot);
        }

        private void SetEquipWeapons(EquipWeapons equipWeapons, byte equipWeaponSet)
        {
            SetEquipWeapon(equipWeapons.rightHand, false, equipWeaponSet);
            SetEquipWeapon(equipWeapons.leftHand, true, equipWeaponSet);
        }

        private void SetEquipWeapon(CharacterItem equipWeapon, bool isLeftHand, byte equipWeaponSet)
        {
            string tempPosition = GetEquipPosition(isLeftHand ? GameDataConst.EQUIP_POSITION_LEFT_HAND : GameDataConst.EQUIP_POSITION_RIGHT_HAND, equipWeaponSet);
            UICharacterItem tempSlot;
            if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
            {
                if (equipWeapon.GetEquipmentItem() != null)
                {
                    equipWeapon.equipSlotIndex = equipWeaponSet;
                    tempSlot.Setup(new UICharacterItemData(equipWeapon, equipWeapon.level, isLeftHand ? InventoryType.EquipWeaponLeft : InventoryType.EquipWeaponRight), character, 0);
                }
            }
        }

        private UICharacterItemData CreateEmptyUIData(InventoryType inventoryType)
        {
            return new UICharacterItemData(CharacterItem.Empty, 1, inventoryType);
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
