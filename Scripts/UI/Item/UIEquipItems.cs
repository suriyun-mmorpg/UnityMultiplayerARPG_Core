using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UIEquipItems : UIBase
    {
        [Header("UI Elements")]
        [FormerlySerializedAs("uiItemDialog")]
        public UICharacterItem uiDialog;
        public UIEquipWeaponsPair[] equipWeaponSlots;
        public UIEquipItemPair[] otherEquipSlots;

        [Header("Options")]
        [Tooltip("If this is `TRUE` it won't update data when controlling character's data changes")]
        public bool notForOwningCharacter;

        public bool NotForOwningCharacter
        {
            get { return notForOwningCharacter; }
            set
            {
                notForOwningCharacter = value;
                RegisterOwningCharacterEvents();
            }
        }

        private Dictionary<string, UICharacterItem> cacheEquipItemSlots;
        public Dictionary<string, UICharacterItem> CacheEquipItemSlots
        {
            get
            {
                if (cacheEquipItemSlots == null)
                {
                    cacheEquipItemSlots = new Dictionary<string, UICharacterItem>();
                    CacheSelectionManager.Clear();
                    // Weapons
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
                        tempEquipPosition = GetEquipPosition(otherEquipSlot.armorType.EquipPosition, tempEquipSlotIndex);
                        if (!string.IsNullOrEmpty(tempEquipPosition) &&
                            otherEquipSlot.ui != null &&
                            !cacheEquipItemSlots.ContainsKey(tempEquipPosition))
                        {
                            otherEquipSlot.ui.Setup(CreateEmptyUIData(InventoryType.EquipItems), Character, -1);
                            otherEquipSlot.ui.SetupAsEquipSlot(otherEquipSlot.armorType.EquipPosition, tempEquipSlotIndex);
                            UICharacterItemDragHandler dragHandler = otherEquipSlot.ui.GetComponentInChildren<UICharacterItemDragHandler>();
                            if (dragHandler != null)
                                dragHandler.SetupForEquipItems(otherEquipSlot.ui);
                            cacheEquipItemSlots.Add(tempEquipPosition, otherEquipSlot.ui);
                            CacheSelectionManager.Add(otherEquipSlot.ui);
                        }
                    }
                }
                return cacheEquipItemSlots;
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

        public ICharacterData Character { get; protected set; }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelect);
            CacheSelectionManager.eventOnDeselected.RemoveListener(OnDeselect);
            CacheSelectionManager.eventOnDeselected.AddListener(OnDeselect);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnDialogHide);
            UpdateOwningCharacterData();
            RegisterOwningCharacterEvents();
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
            UnregisterOwningCharacterEvents();
        }

        public void RegisterOwningCharacterEvents()
        {
            UnregisterOwningCharacterEvents();
            if (notForOwningCharacter || !GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onEquipWeaponSetChange += OnEquipWeaponSetChange;
            GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
            GameInstance.PlayingCharacterEntity.onEquipItemsOperation += OnEquipItemsOperation;
        }

        public void UnregisterOwningCharacterEvents()
        {
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onEquipWeaponSetChange -= OnEquipWeaponSetChange;
            GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation -= OnSelectableWeaponSetsOperation;
            GameInstance.PlayingCharacterEntity.onEquipItemsOperation -= OnEquipItemsOperation;
        }

        private void OnEquipWeaponSetChange(byte equipWeaponSet)
        {
            UpdateOwningCharacterData();
        }

        private void OnSelectableWeaponSetsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        private void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        public void UpdateOwningCharacterData()
        {
            if (notForOwningCharacter || GameInstance.PlayingCharacter == null) return;
            UpdateData(GameInstance.PlayingCharacter);
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
                uiDialog.Setup(ui.Data, Character, ui.IndexOfData);
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

        public virtual void UpdateData(ICharacterData character)
        {
            this.Character = character;
            string selectedId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.CharacterItem.id : string.Empty;
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
            IArmorItem tempArmorItem;
            UICharacterItem selectedUI = null;
            UICharacterItem tempUI;
            int i;
            for (i = 0; i < character.EquipItems.Count; ++i)
            {
                tempEquipItem = character.EquipItems[i];
                tempArmorItem = tempEquipItem.GetArmorItem();
                if (tempArmorItem == null)
                    continue;

                if (CacheEquipItemSlots.TryGetValue(GetEquipPosition(tempArmorItem.GetEquipPosition(), tempEquipItem.equipSlotIndex), out tempUI))
                {
                    tempUI.Setup(new UICharacterItemData(tempEquipItem, InventoryType.EquipItems), character, i);
                    if (selectedId.Equals(tempEquipItem.id))
                        selectedUI = tempUI;
                }
            }
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

            for (i = 0; i < character.SelectableWeaponSets.Count; ++i)
            {
                SetEquipWeapons(selectedId, character.SelectableWeaponSets[i], (byte)i);
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
            slot.Setup(CreateEmptyUIData(isLeftHand ? InventoryType.EquipWeaponLeft : InventoryType.EquipWeaponRight), Character, -1);
            slot.SetupAsEquipSlot(isLeftHand ? GameDataConst.EQUIP_POSITION_LEFT_HAND : GameDataConst.EQUIP_POSITION_RIGHT_HAND, equipWeaponSet);
            UICharacterItemDragHandler dragHandler = slot.GetComponentInChildren<UICharacterItemDragHandler>();
            if (dragHandler != null)
                dragHandler.SetupForEquipItems(slot);
            cacheEquipItemSlots.Add(GetEquipPosition(isLeftHand ? GameDataConst.EQUIP_POSITION_LEFT_HAND : GameDataConst.EQUIP_POSITION_RIGHT_HAND, equipWeaponSet), slot);
            CacheSelectionManager.Add(slot);
        }

        private void SetEquipWeapons(string selectedId, EquipWeapons equipWeapons, byte equipWeaponSet)
        {
            SetEquipWeapon(selectedId, equipWeapons.rightHand, false, equipWeaponSet);
            SetEquipWeapon(selectedId, equipWeapons.leftHand, true, equipWeaponSet);
        }

        private void SetEquipWeapon(string selectedId, CharacterItem equipWeapon, bool isLeftHand, byte equipWeaponSet)
        {
            string tempPosition = GetEquipPosition(isLeftHand ? GameDataConst.EQUIP_POSITION_LEFT_HAND : GameDataConst.EQUIP_POSITION_RIGHT_HAND, equipWeaponSet);
            UICharacterItem tempSlot;
            if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
            {
                if (equipWeapon.GetEquipmentItem() != null)
                {
                    equipWeapon.equipSlotIndex = equipWeaponSet;
                    tempSlot.Setup(new UICharacterItemData(equipWeapon, isLeftHand ? InventoryType.EquipWeaponLeft : InventoryType.EquipWeaponRight), Character, 0);
                    if (selectedId.Equals(equipWeapon.id))
                        tempSlot.OnClickSelect();
                }
            }
        }

        private UICharacterItemData CreateEmptyUIData(InventoryType inventoryType)
        {
            return new UICharacterItemData(CharacterItem.Empty, 1, inventoryType);
        }

        private string GetEquipPosition(string equipPosition, byte equipSlotIndex)
        {
            return equipPosition + ":" + equipSlotIndex;
        }

        private byte GetEquipSlotIndexFromEquipPosition(string equipPosition)
        {
            string[] splitEquipPosition = equipPosition.Split(':');
            return byte.Parse(splitEquipPosition[splitEquipPosition.Length - 1]);
        }
    }
}
