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

        private void OnNonEquipItemsOperation(LiteNetLibSyncListOp operation, int index, CharacterItem oldItem, CharacterItem newItem)
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

        public void OnClickSort()
        {
            GameInstance.ClientInventoryHandlers.RequestSortItems(new RequestSortItemsMessage(), ClientInventoryActions.ResponseSortItems);
        }
    }
}
