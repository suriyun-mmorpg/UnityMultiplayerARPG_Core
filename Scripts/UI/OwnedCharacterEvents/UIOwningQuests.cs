using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
	[RequireComponent(typeof(UICharacterQuests))]
	public class UIOwningQuests : MonoBehaviour
	{
		UICharacterQuests cachedUI;
		private void Awake()
		{
			cachedUI = GetComponent<UICharacterQuests>();
		}

		private void OnEnable()
		{
			UpdateData();
			BasePlayerCharacterController.OwningCharacter.onQuestsOperation += OnQuestsOperation;
		}

		private void OnDisable()
		{
			BasePlayerCharacterController.OwningCharacter.onQuestsOperation -= OnQuestsOperation;
		}

		private void OnQuestsOperation(LiteNetLibSyncList.Operation operation, int index)
		{
			UpdateData();
		}

		private void UpdateData()
		{
			cachedUI.UpdateData(BasePlayerCharacterController.OwningCharacter);
		}
	}
}
