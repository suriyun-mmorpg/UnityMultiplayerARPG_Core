using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
	[RequireComponent(typeof(UICharacterHotkeys))]
	public class UIOwningHotkeys : MonoBehaviour
	{
		UICharacterHotkeys cachedUI;
		private void Awake()
		{
			cachedUI = GetComponent<UICharacterHotkeys>();
		}

		private void OnEnable()
		{
			UpdateData();
			BasePlayerCharacterController.OwningCharacter.onHotkeysOperation += OnHotkeysOperation;
		}

		private void OnDisable()
		{
			BasePlayerCharacterController.OwningCharacter.onHotkeysOperation -= OnHotkeysOperation;
		}

		private void OnHotkeysOperation(LiteNetLibSyncList.Operation operation, int index)
		{
			UpdateData();
		}

		private void UpdateData()
		{
			cachedUI.UpdateData(BasePlayerCharacterController.OwningCharacter);
		}
	}
}
