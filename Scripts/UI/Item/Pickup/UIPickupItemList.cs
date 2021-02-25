namespace MultiplayerARPG
{
	public class UIPickupItemList : UICharacterItems
    {
        public bool pickUpOnSelect;

        protected override void OnSelect(UICharacterItem ui)
        {
            base.OnSelect(ui);
            if (pickUpOnSelect)
                OnClickPickUpSelectedItem();
        }

        public void OnClickPickUpSelectedItem()
        {
            string selectedId = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.CharacterItem.id : string.Empty;
            if (string.IsNullOrEmpty(selectedId))
                return;
            GameInstance.PlayingCharacterEntity.CallServerPickupItem(uint.Parse(selectedId));
        }

        public void OnClickPickupNearbyItems()
        {
            GameInstance.PlayingCharacterEntity.CallServerPickupNearbyItems();
        }
    }
}
