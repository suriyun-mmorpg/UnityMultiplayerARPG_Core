using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
	[RequireComponent(typeof(UINonEquipItems))]
	public class UIOwningNonEquipItems : MonoBehaviour
	{
		UINonEquipItems cachedUI;
		private void Awake()
		{
			cachedUI = GetComponent<UINonEquipItems>();
		}

		private void OnEnable()
		{
			UpdateData();
			BasePlayerCharacterController.OwningCharacter.onDataIdChange += OnDataIdChange;
			BasePlayerCharacterController.OwningCharacter.onNonEquipItemsOperation += OnNonEquipItemsOperation;
		}

		private void OnDisable()
		{
			BasePlayerCharacterController.OwningCharacter.onDataIdChange -= OnDataIdChange;
			BasePlayerCharacterController.OwningCharacter.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
		}

		private void OnDataIdChange(int dataId)
		{
			UpdateData();
		}

		private void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
		{
			UpdateData();
		}

		private void UpdateData()
		{
			cachedUI.UpdateData(BasePlayerCharacterController.OwningCharacter);
		}
	}
}
