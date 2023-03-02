using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIBaseOwningCharacterItem : UISelectionEntry<UIOwningCharacterItemData>
    {
        public InventoryType InventoryType { get { return Data.inventoryType; } }
        public int IndexOfData { get { return Data.indexOfData; } }
        public CharacterItem CharacterItem
        {
            get
            {
                switch (InventoryType)
                {
                    case InventoryType.NonEquipItems:
                        if (IndexOfData >= 0 && IndexOfData < GameInstance.PlayingCharacter.NonEquipItems.Count)
                            return GameInstance.PlayingCharacter.NonEquipItems[IndexOfData];
                        break;
                    case InventoryType.EquipItems:
                        if (IndexOfData >= 0 && IndexOfData < GameInstance.PlayingCharacter.EquipItems.Count)
                            return GameInstance.PlayingCharacter.EquipItems[IndexOfData];
                        break;
                    case InventoryType.EquipWeaponRight:
                        return GameInstance.PlayingCharacter.EquipWeapons.rightHand;
                    case InventoryType.EquipWeaponLeft:
                        return GameInstance.PlayingCharacter.EquipWeapons.leftHand;
                }
                return null;
            }
        }
        public int Level { get { return (CharacterItem != null ? CharacterItem.level : 1); } }
        public int Amount { get { return (CharacterItem != null ? CharacterItem.amount : 0); } }

        public UICharacterItem uiCharacterItem;
        [Tooltip("These objects will be activated while item is set")]
        public GameObject[] hasItemObjects;
        [Tooltip("These objects will be activated while item is not set")]
        public GameObject[] noItemObjects;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onEquipWeaponSetChange += OnEquipWeaponSetChange;
            GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
            GameInstance.PlayingCharacterEntity.onEquipItemsOperation += OnEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onEquipWeaponSetChange -= OnEquipWeaponSetChange;
            GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation -= OnSelectableWeaponSetsOperation;
            GameInstance.PlayingCharacterEntity.onEquipItemsOperation -= OnEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
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
            if (hasItemObjects != null && hasItemObjects.Length > 0)
            {
                foreach (GameObject hasItemObject in hasItemObjects)
                {
                    if (hasItemObject == null)
                        continue;
                    hasItemObject.SetActive(!CharacterItem.IsEmptySlot());
                }
            }
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
                    uiCharacterItem.Setup(new UICharacterItemData(CharacterItem, InventoryType), GameInstance.PlayingCharacter, IndexOfData);
                    uiCharacterItem.Show();
                }
            }
        }

        public abstract void OnUpdateCharacterItems();

    }
}
