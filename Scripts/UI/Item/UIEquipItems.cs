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
                        string equipPosition = GameDataConst.EQUIP_POSITION_RIGHT_HAND;
                        rightHandSlot.Setup(GetEmptyUIData(), character, -1);
                        cacheEquipItemSlots.Add(equipPosition, rightHandSlot);
                        SelectionManager.Add(rightHandSlot);
                    }
                    if (leftHandSlot != null)
                    {
                        string equipPosition = GameDataConst.EQUIP_POSITION_LEFT_HAND;
                        leftHandSlot.Setup(GetEmptyUIData(), character, -1);
                        cacheEquipItemSlots.Add(equipPosition, leftHandSlot);
                        SelectionManager.Add(leftHandSlot);
                    }
                    foreach (UICharacterItemPair otherEquipSlot in otherEquipSlots)
                    {
                        if (!string.IsNullOrEmpty(otherEquipSlot.armorType.Id) &&
                            otherEquipSlot.ui != null &&
                            !cacheEquipItemSlots.ContainsKey(otherEquipSlot.armorType.Id))
                        {
                            string equipPosition = otherEquipSlot.armorType.Id;
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
            Dictionary<string, UICharacterItem>.ValueCollection slots = CacheEquipItemSlots.Values;
            // Clear slots data
            foreach (UICharacterItem slot in slots)
            {
                slot.Setup(GetEmptyUIData(), this.character, -1);
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
                    tempSlot.Setup(new CharacterItemTuple(equipItem, equipItem.level, tempPosition), this.character, -1);
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
