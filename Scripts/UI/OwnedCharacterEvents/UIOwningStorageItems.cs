using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
	[RequireComponent(typeof(UICharacterSkills))]
	public class UIOwningStorageItems : MonoBehaviour
	{
		UIStorageItems cachedUI;
		private void Awake()
		{
			cachedUI = GetComponent<UIStorageItems>();
		}

		private void OnEnable()
		{
			UpdateData();
			BasePlayerCharacterController.OwningCharacter.onStorageItemsChange += OnStorageItemsChange;
		}

		private void OnDisable()
		{
			BasePlayerCharacterController.OwningCharacter.onStorageItemsChange -= OnStorageItemsChange;
		}

		private void OnStorageItemsChange(CharacterItem[] storageItems)
		{
			UpdateData();
		}

		private void UpdateData()
		{
			cachedUI.UpdateData();
		}
	}
}
