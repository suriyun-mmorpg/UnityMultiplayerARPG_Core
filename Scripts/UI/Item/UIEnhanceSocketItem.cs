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
                if (uiSocketEnhancerItems.CacheNonEquipItemSelectionManager != null &&
                    uiSocketEnhancerItems.CacheNonEquipItemSelectionManager.SelectedUI != null &&
                    uiSocketEnhancerItems.CacheNonEquipItemSelectionManager.SelectedUI.SocketEnhancerItem != null)
                    return uiSocketEnhancerItems.CacheNonEquipItemSelectionManager.SelectedUI.SocketEnhancerItem.DataId;
                return 0;
            }
        }

        [Header("UI Elements for UI Enhance Socket Item")]
        public UINonEquipItems uiSocketEnhancerItems;

        public void OnUpdateCharacterItems()
        {
            if (uiCharacterItem != null)
            {
                if (CharacterItem == null)
                    uiCharacterItem.Hide();
                else
                {
                    uiCharacterItem.Setup(new CharacterItemTuple(CharacterItem, Level, InventoryType), OwningCharacter, IndexOfData);
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

        public override void Hide()
        {
            Data = new CharacterItemByIndexTuple(InventoryType.NonEquipItems, -1);
            base.Hide();
        }

        public void OnClickEnhanceSocket()
        {
            if (IndexOfData < 0 || SelectedEnhancerId == 0)
                return;
            OwningCharacter.RequestEnhanceSocketItem((byte)InventoryType, (short)IndexOfData, SelectedEnhancerId);
        }
	}
}
