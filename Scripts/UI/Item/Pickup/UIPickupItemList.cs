using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
	public class UIPickupItemList : UICharacterItems
    {
        public bool pickUpOnSelect;

        protected override void OnSelectCharacterItem(UICharacterItem ui)
        {
            base.OnSelectCharacterItem(ui);
            if (pickUpOnSelect)
                OnClickPickUpSelectedItem();
        }

        public void OnClickPickUpSelectedItem()
        {
            string selectedId = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.SelectedUI.CharacterItem.id : string.Empty;
            if (string.IsNullOrEmpty(selectedId))
                return;
            BasePlayerCharacterController.OwningCharacter.CallServerPickupItem(uint.Parse(selectedId));
        }

        public void OnClickPickupNearbyItems()
        {
            BasePlayerCharacterController.OwningCharacter.CallServerPickupNearbyItems();
        }
    }
}
