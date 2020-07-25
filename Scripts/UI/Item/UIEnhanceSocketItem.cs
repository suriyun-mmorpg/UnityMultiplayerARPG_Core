using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIEnhanceSocketItem : UIBaseOwningCharacterItem
    {
        public IEquipmentItem EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public bool CanEnhance { get { return EquipmentItem != null && EquipmentItem.MaxSocket > 0 && CharacterItem.Sockets.Count < EquipmentItem.MaxSocket; } }
        public int SelectedEnhancerId
        {
            get
            {
                if (uiSocketEnhancerItems.CacheItemSelectionManager != null &&
                    uiSocketEnhancerItems.CacheItemSelectionManager.SelectedUI != null &&
                    uiSocketEnhancerItems.CacheItemSelectionManager.SelectedUI.SocketEnhancerItem != null)
                    return uiSocketEnhancerItems.CacheItemSelectionManager.SelectedUI.SocketEnhancerItem.DataId;
                return 0;
            }
        }

        [Header("UI Elements for UI Enhance Socket Item")]
        public UINonEquipItems uiSocketEnhancerItems;

        protected bool activated;
        protected string activeItemId;

        public override void OnUpdateCharacterItems()
        {
            if (!IsVisible())
                return;

            // Store data to variable so it won't lookup for data from property again
            CharacterItem characterItem = CharacterItem;

            if (activated && (characterItem.IsEmptySlot() || !characterItem.id.Equals(activeItemId)))
            {
                // Item's ID is difference to active item ID, so the item may be destroyed
                // So clear data
                Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
                return;
            }

            if (uiCharacterItem != null)
            {
                if (characterItem.IsEmptySlot())
                {
                    uiCharacterItem.Hide();
                }
                else
                {
                    uiCharacterItem.Setup(new UICharacterItemData(characterItem, Level, InventoryType), OwningCharacter, IndexOfData);
                    uiCharacterItem.Show();
                }
            }

            if (uiSocketEnhancerItems != null)
            {
                uiSocketEnhancerItems.filterItemTypes = new List<ItemType>() { ItemType.SocketEnhancer };
                uiSocketEnhancerItems.filterCategories = new List<string>();
                uiSocketEnhancerItems.UpdateData(OwningCharacter);
            }
        }

        public override void Show()
        {
            base.Show();
            activated = false;
            OnUpdateCharacterItems();
        }

        public override void Hide()
        {
            base.Hide();
            Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
        }

        public void OnClickEnhanceSocket()
        {
            if (CharacterItem.IsEmptySlot() || SelectedEnhancerId == 0)
                return;
            activated = true;
            activeItemId = CharacterItem.id;
            OwningCharacter.RequestEnhanceSocketItem(InventoryType, (short)IndexOfData, SelectedEnhancerId);
        }
    }
}
