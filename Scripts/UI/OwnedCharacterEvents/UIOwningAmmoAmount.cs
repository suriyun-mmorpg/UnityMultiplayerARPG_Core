using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
	[RequireComponent(typeof(UIAmmoAmount))]
	public class UIOwningAmmoAmount : MonoBehaviour
	{
		UIAmmoAmount cachedUI;
		private void Awake()
		{
			cachedUI = GetComponent<UIAmmoAmount>();
		}

		private void OnEnable()
		{
			UpdateData();
			BasePlayerCharacterController.OwningCharacter.onEquipItemsOperation += OnEquipItemsOperation;
			BasePlayerCharacterController.OwningCharacter.onEquipWeaponSetChange += OnEquipWeaponSetChange;
			BasePlayerCharacterController.OwningCharacter.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
			BasePlayerCharacterController.OwningCharacter.onNonEquipItemsOperation += OnNonEquipItemsOperation;
		}

		private void OnDisable()
		{
			BasePlayerCharacterController.OwningCharacter.onEquipItemsOperation -= OnEquipItemsOperation;
			BasePlayerCharacterController.OwningCharacter.onEquipWeaponSetChange -= OnEquipWeaponSetChange;
			BasePlayerCharacterController.OwningCharacter.onSelectableWeaponSetsOperation -= OnSelectableWeaponSetsOperation;
			BasePlayerCharacterController.OwningCharacter.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
		}

		protected void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
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

		protected void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
		{
			UpdateData();
		}

		private void UpdateData()
		{
			cachedUI.UpdateData(BasePlayerCharacterController.OwningCharacter);
		}
	}
}
