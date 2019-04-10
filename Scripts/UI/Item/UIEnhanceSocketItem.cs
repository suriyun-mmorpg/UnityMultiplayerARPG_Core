using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
	public class UIEnhanceSocketItem : BaseUICharacterItemByIndex
	{
        public Item EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public bool CanEnhance { get { return EquipmentItem != null && EquipmentItem.socket > 0 && CharacterItem.Sockets.Count < EquipmentItem.socket; } }
		public int SelectedEnhancerId { get { return 0; } }
		
        public override void Hide()
        {
            Data = new CharacterItemByIndexTuple(InventoryType.NonEquipItems, -1);
            base.Hide();
        }

        public void OnClickEnhanceSocket()
        {
            if (IndexOfData < 0)
                return;
            OwningCharacter.RequestEnhanceSocketItem((byte)InventoryType, (short)IndexOfData, SelectedEnhancerId);
        }
	}
}
