using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
	public abstract class UIBaseOwningCharacterItem : UISelectionEntry<UIOwningCharacterItemData>
	{
        public BasePlayerCharacterEntity OwningCharacter { get { return BasePlayerCharacterController.OwningCharacter; } }
		public InventoryType InventoryType { get { return Data.inventoryType; } }
		public int IndexOfData { get { return Data.indexOfData; } }
		public CharacterItem CharacterItem
		{
			get
			{
				switch (InventoryType)
				{
					case InventoryType.NonEquipItems:
						if (IndexOfData >= 0 && IndexOfData < OwningCharacter.NonEquipItems.Count)
							return OwningCharacter.NonEquipItems[IndexOfData];
						break;
					case InventoryType.EquipItems:
						if (IndexOfData >= 0 && IndexOfData < OwningCharacter.EquipItems.Count)
							return OwningCharacter.EquipItems[IndexOfData];
						break;
					case InventoryType.EquipWeaponRight:
						return OwningCharacter.EquipWeapons.rightHand;
					case InventoryType.EquipWeaponLeft:
						return OwningCharacter.EquipWeapons.leftHand;
				}
				return null;
			}
		}
        public short Level { get { return (short)(CharacterItem != null ? CharacterItem.level : 1); } }
        public short Amount { get { return (short)(CharacterItem != null ? CharacterItem.amount : 0); } }

        public UICharacterItem uiCharacterItem;
        [Tooltip("These objects will be activated while item is not set")]
        public GameObject[] noItemObjects;

        protected override void OnEnable()
        {
            base.OnEnable();
            OwningCharacter.onEquipWeaponSetChange += OnEquipWeaponSetChange;
            OwningCharacter.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
            OwningCharacter.onEquipItemsOperation += OnEquipItemsOperation;
            OwningCharacter.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OwningCharacter.onEquipWeaponSetChange -= OnEquipWeaponSetChange;
            OwningCharacter.onSelectableWeaponSetsOperation -= OnSelectableWeaponSetsOperation;
            OwningCharacter.onEquipItemsOperation -= OnEquipItemsOperation;
            OwningCharacter.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
        }

        protected void OnEquipWeaponSetChange(byte equipWeaponSet)
        {
            OnUpdateCharacterItems();
        }

        protected void OnSelectableWeaponSetsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            OnUpdateCharacterItems();
        }

        protected void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            OnUpdateCharacterItems();
        }

        protected void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            OnUpdateCharacterItems();
        }

        protected override void Update()
        {
            base.Update();
            if (noItemObjects != null && noItemObjects.Length > 0)
            {
                foreach (GameObject noItemObject in noItemObjects)
                {
                    if (noItemObject == null)
                        continue;
                    noItemObject.SetActive(CharacterItem.IsEmptySlot());
                }
            }
        }

        protected override void UpdateData()
        {
            if (uiCharacterItem != null)
            {
                if (CharacterItem.IsEmptySlot())
                {
                    uiCharacterItem.Hide();
                }
                else
                {
                    uiCharacterItem.Setup(new UICharacterItemData(CharacterItem, InventoryType), OwningCharacter, IndexOfData);
                    uiCharacterItem.Show();
                }
            }
        }

        public abstract void OnUpdateCharacterItems();

    }
}
