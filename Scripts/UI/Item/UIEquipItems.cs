using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterItemSelectionManager))]
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
                    SelectionManager.Clear();
                    if (rightHandSlot != null)
                    {
                        var equipPosition = GameDataConst.EQUIP_POSITION_RIGHT_HAND;
                        rightHandSlot.Setup(GetEmptyUIData(), character, -1);
                        cacheEquipItemSlots.Add(equipPosition, rightHandSlot);
                        SelectionManager.Add(rightHandSlot);
                    }
                    if (leftHandSlot != null)
                    {
                        var equipPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
                        leftHandSlot.Setup(GetEmptyUIData(), character, -1);
                        cacheEquipItemSlots.Add(equipPosition, leftHandSlot);
                        SelectionManager.Add(leftHandSlot);
                    }
                    foreach (var otherEquipSlot in otherEquipSlots)
                    {
                        if (!string.IsNullOrEmpty(otherEquipSlot.armorType.Id) &&
                            otherEquipSlot.ui != null &&
                            !cacheEquipItemSlots.ContainsKey(otherEquipSlot.armorType.Id))
                        {
                            var equipPosition = otherEquipSlot.armorType.Id;
                            otherEquipSlot.ui.Setup(GetEmptyUIData(), character, -1);
                            cacheEquipItemSlots.Add(equipPosition, otherEquipSlot.ui);
                            SelectionManager.Add(otherEquipSlot.ui);
                        }
                    }
                }
                return cacheEquipItemSlots;
            }
        }

        private UICharacterItemSelectionManager selectionManager;
        public UICharacterItemSelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UICharacterItemSelectionManager>();
                selectionManager.selectionMode = UISelectionMode.SelectSingle;
                return selectionManager;
            }
        }

        public override void Show()
        {
            SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
            SelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
            SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterItem);
            SelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterItem);
            base.Show();
        }

        public override void Hide()
        {
            SelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            if (uiItemDialog != null && ui.Data.characterItem.IsValid())
            {
                uiItemDialog.selectionManager = SelectionManager;
                uiItemDialog.Setup(ui.Data, character, ui.indexOfData);
                uiItemDialog.Show();
            }
        }

        protected void OnDeselectCharacterItem(UICharacterItem ui)
        {
            if (uiItemDialog != null)
                uiItemDialog.Hide();
        }

        public void UpdateData(ICharacterData character)
        {
            this.character = character;
            var slots = CacheEquipItemSlots.Values;
            // Clear slots data
            foreach (var slot in slots)
            {
                slot.Setup(GetEmptyUIData(), this.character, -1);
                slot.Show();
            }

            if (character == null)
                return;

            string tempPosition;
            UICharacterItem tempSlot;
            var equipItems = character.EquipItems;
            for (var i = 0; i < equipItems.Count; ++i)
            {
                var equipItem = equipItems[i];
                var armorItem = equipItem.GetArmorItem();
                if (armorItem == null)
                    continue;

                tempPosition = armorItem.EquipPosition;
                if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
                    tempSlot.Setup(new CharacterItemTuple(equipItem, equipItem.level, tempPosition), this.character, -1);
            }

            var equipWeapons = character.EquipWeapons;
            var rightHand = equipWeapons.rightHand;
            var leftHand = equipWeapons.leftHand;
            var rightHandEquipment = rightHand.GetEquipmentItem();
            var leftHandEquipment = leftHand.GetEquipmentItem();
            tempPosition = GameDataConst.EQUIP_POSITION_RIGHT_HAND;
            if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
            {
                if (rightHandEquipment != null)
                    tempSlot.Setup(new CharacterItemTuple(rightHand, rightHand.level, tempPosition), this.character, -1);
            }
            tempPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
            if (CacheEquipItemSlots.TryGetValue(tempPosition, out tempSlot))
            {
                if (leftHandEquipment != null)
                    tempSlot.Setup(new CharacterItemTuple(leftHand, leftHand.level, tempPosition), this.character, -1);
            }
        }

        private CharacterItemTuple GetEmptyUIData()
        {
            return new CharacterItemTuple(CharacterItem.Empty, 1, string.Empty);
        }
    }
}
