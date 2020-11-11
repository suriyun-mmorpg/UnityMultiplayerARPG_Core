using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UINonEquipItems : UICharacterItems
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateOwningCharacterData();
            if (!BasePlayerCharacterController.OwningCharacter) return;
            BasePlayerCharacterController.OwningCharacter.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!BasePlayerCharacterController.OwningCharacter) return;
            BasePlayerCharacterController.OwningCharacter.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
        }

        private void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        private void UpdateOwningCharacterData()
        {
            if (!BasePlayerCharacterController.OwningCharacter) return;
            UpdateData(BasePlayerCharacterController.OwningCharacter);
        }

        public void UpdateData(ICharacterData character)
        {
            UpdateData(character, character.NonEquipItems);
        }
    }
}
