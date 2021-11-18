using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class UINonEquipItems : UICharacterItems
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateOwningCharacterData();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
        }

        private void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        public void UpdateOwningCharacterData()
        {
            if (GameInstance.PlayingCharacter == null) return;
            UpdateData(GameInstance.PlayingCharacter);
        }

        public void UpdateData(ICharacterData character)
        {
            inventoryType = InventoryType.NonEquipItems;
            UpdateData(character, character.NonEquipItems);
        }
    }
}
