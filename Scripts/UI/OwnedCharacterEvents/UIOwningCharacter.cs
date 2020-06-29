using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacter))]
    public class UIOwningCharacter : MonoBehaviour
    {
        UICharacter cachedUI;
        private void Awake()
        {
            cachedUI = GetComponent<UICharacter>();
        }

        private void OnEnable()
        {
            UpdateData();
            BasePlayerCharacterController.OwningCharacter.onDataIdChange += OnDataIdChange;
            BasePlayerCharacterController.OwningCharacter.onEquipWeaponSetChange += OnEquipWeaponSetChange;
            BasePlayerCharacterController.OwningCharacter.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
            BasePlayerCharacterController.OwningCharacter.onAttributesOperation += OnAttributesOperation;
            BasePlayerCharacterController.OwningCharacter.onSkillsOperation += OnSkillsOperation;
            BasePlayerCharacterController.OwningCharacter.onSummonsOperation += OnSummonsOperation;
            BasePlayerCharacterController.OwningCharacter.onBuffsOperation += OnBuffsOperation;
            BasePlayerCharacterController.OwningCharacter.onEquipItemsOperation += OnEquipItemsOperation;
            BasePlayerCharacterController.OwningCharacter.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        private void OnDisable()
        {
            BasePlayerCharacterController.OwningCharacter.onDataIdChange -= OnDataIdChange;
            BasePlayerCharacterController.OwningCharacter.onEquipWeaponSetChange -= OnEquipWeaponSetChange;
            BasePlayerCharacterController.OwningCharacter.onSelectableWeaponSetsOperation -= OnSelectableWeaponSetsOperation;
            BasePlayerCharacterController.OwningCharacter.onAttributesOperation -= OnAttributesOperation;
            BasePlayerCharacterController.OwningCharacter.onSkillsOperation -= OnSkillsOperation;
            BasePlayerCharacterController.OwningCharacter.onSummonsOperation -= OnSummonsOperation;
            BasePlayerCharacterController.OwningCharacter.onBuffsOperation -= OnBuffsOperation;
            BasePlayerCharacterController.OwningCharacter.onEquipItemsOperation -= OnEquipItemsOperation;
            BasePlayerCharacterController.OwningCharacter.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
        }

        private void OnDataIdChange(int dataId)
        {
            UpdateData();
        }

        private void OnEquipWeaponSetChange(byte equipWeaponSet)
        {
            UpdateData();
        }

        private void OnSelectableWeaponSetsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData();
        }

        private void OnAttributesOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData();
        }

        private void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData();
        }

        private void OnSummonsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData();
        }

        private void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData();
        }

        private void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData();
        }

        private void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData();
        }

        private void UpdateData()
        {
            cachedUI.Data = BasePlayerCharacterController.OwningCharacter;
        }
    }
}
