using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
	[RequireComponent(typeof(UICharacterSummons))]
	public class UIOwningSummons : MonoBehaviour
	{
		UICharacterSummons cachedUI;
		private void Awake()
		{
			cachedUI = GetComponent<UICharacterSummons>();
		}

		private void OnEnable()
		{
			UpdateData();
			BasePlayerCharacterController.OwningCharacter.onSummonsOperation += OnSummonsOperation;
		}

		private void OnDisable()
		{
			BasePlayerCharacterController.OwningCharacter.onSummonsOperation -= OnSummonsOperation;
		}

		private void OnSummonsOperation(LiteNetLibSyncList.Operation operation, int index)
		{
			UpdateData();
		}

		private void UpdateData()
		{
			cachedUI.UpdateData(BasePlayerCharacterController.OwningCharacter);
		}
	}
}
