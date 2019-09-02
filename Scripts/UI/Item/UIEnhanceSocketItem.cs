using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
	public class UIEnhanceSocketItem : BaseUICharacterItemByIndex
	{
        public Item EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public bool CanEnhance { get { return EquipmentItem != null && EquipmentItem.maxSocket > 0 && CharacterItem.Sockets.Count < EquipmentItem.maxSocket; } }
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

        public void OnUpdateCharacterItems()
        {
            if (!IsVisible())
                return;

            if (uiCharacterItem != null)
            {
                if (CharacterItem == null)
                    uiCharacterItem.Hide();
                else
                {
                    uiCharacterItem.Setup(new UICharacterItemData(CharacterItem, Level, InventoryType), OwningCharacter, IndexOfData);
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
            OnUpdateCharacterItems();
        }

        public override void Hide()
        {
            base.Hide();
            Data = new UICharacterItemByIndexData(InventoryType.NonEquipItems, -1);
        }

        public void OnClickEnhanceSocket()
        {
            if (IndexOfData < 0 || SelectedEnhancerId == 0)
                return;
            OwningCharacter.RequestEnhanceSocketItem((byte)InventoryType, (short)IndexOfData, SelectedEnhancerId);
        }
	}
}
