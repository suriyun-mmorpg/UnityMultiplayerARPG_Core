using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIWeaponSets : UIBase
    {
        public UIWeaponSet currentWeaponSet;
        public UIWeaponSet[] otherWeaponSets;

        private void OnEnable()
        {
            UpdateData(GameInstance.PlayingCharacter);
            GameInstance.PlayingCharacterEntity.onEquipItemsOperation += OnEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onEquipWeaponSetChange += OnEquipWeaponSetChange;
            GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        private void OnDisable()
        {
            GameInstance.PlayingCharacterEntity.onEquipItemsOperation += OnEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onEquipWeaponSetChange += OnEquipWeaponSetChange;
            GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        protected void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        private void OnEquipWeaponSetChange(byte equipWeaponSet)
        {
            UpdateOwningCharacterData();
        }

        private void OnSelectableWeaponSetsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        protected void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        public void UpdateOwningCharacterData()
        {
            if (GameInstance.PlayingCharacter == null) return;
            UpdateData(GameInstance.PlayingCharacter);
        }

        public void ChangeWeaponSet(byte index)
        {
            GameInstance.ClientInventoryHandlers.RequestSwitchEquipWeaponSet(new RequestSwitchEquipWeaponSetMessage()
            {
                equipWeaponSet = index,
            }, ClientInventoryActions.ResponseSwitchEquipWeaponSet);
        }

        public void UpdateData(IPlayerCharacterData playerCharacter)
        {
            byte equipWeaponSet = playerCharacter.EquipWeaponSet;
            currentWeaponSet.SetData(this, equipWeaponSet, playerCharacter.SelectableWeaponSets[equipWeaponSet]);
            byte j = 0;
            for (byte i = 0; i < playerCharacter.SelectableWeaponSets.Count; ++i)
            {
                if (i != equipWeaponSet && j < otherWeaponSets.Length)
                    otherWeaponSets[j++].SetData(this, i, playerCharacter.SelectableWeaponSets[i]);
            }
        }
    }
}
