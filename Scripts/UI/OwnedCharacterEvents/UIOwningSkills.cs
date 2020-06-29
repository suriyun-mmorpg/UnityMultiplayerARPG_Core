using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
	[RequireComponent(typeof(UICharacterSkills))]
	public class UIOwningSkills : MonoBehaviour
	{
		UICharacterSkills cachedUI;
		private void Awake()
		{
			cachedUI = GetComponent<UICharacterSkills>();
		}

		private void OnEnable()
		{
			UpdateData();
			BasePlayerCharacterController.OwningCharacter.onDataIdChange += OnDataIdChange;
			BasePlayerCharacterController.OwningCharacter.onEquipWeaponSetChange += OnEquipWeaponSetChange;
			BasePlayerCharacterController.OwningCharacter.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
			BasePlayerCharacterController.OwningCharacter.onEquipItemsOperation += OnEquipItemsOperation;
			BasePlayerCharacterController.OwningCharacter.onSkillsOperation += OnSkillsOperation;
		}

		private void OnDisable()
		{
			BasePlayerCharacterController.OwningCharacter.onDataIdChange -= OnDataIdChange;
			BasePlayerCharacterController.OwningCharacter.onEquipWeaponSetChange -= OnEquipWeaponSetChange;
			BasePlayerCharacterController.OwningCharacter.onSelectableWeaponSetsOperation -= OnSelectableWeaponSetsOperation;
			BasePlayerCharacterController.OwningCharacter.onEquipItemsOperation -= OnEquipItemsOperation;
			BasePlayerCharacterController.OwningCharacter.onSkillsOperation -= OnSkillsOperation;
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

		private void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
		{
			UpdateData();
		}

		private void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
		{
			UpdateData();
		}

		private void UpdateData()
		{
			cachedUI.UpdateData(BasePlayerCharacterController.OwningCharacter);
		}
	}
}
