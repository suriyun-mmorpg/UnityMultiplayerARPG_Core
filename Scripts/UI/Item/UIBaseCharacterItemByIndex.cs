using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
	public abstract class BaseUICharacterItemByIndex : UISelectionEntry<UICharacterItemByIndexData>
	{
        public BasePlayerCharacterEntity OwningCharacter { get { return BasePlayerCharacterController.OwningCharacter; } }
		public InventoryType InventoryType { get { return Data.inventoryType; } }
		public int IndexOfData { get { return Data.indexOfData; } }
		public CharacterItem CharacterItem
		{
			get
			{
				switch (InventoryType)
				{
					case InventoryType.NonEquipItems:
						if (IndexOfData >= 0 && IndexOfData < OwningCharacter.NonEquipItems.Count)
							return OwningCharacter.NonEquipItems[IndexOfData];
						break;
					case InventoryType.EquipItems:
						if (IndexOfData >= 0 && IndexOfData < OwningCharacter.EquipItems.Count)
							return OwningCharacter.EquipItems[IndexOfData];
						break;
					case InventoryType.EquipWeaponRight:
						return OwningCharacter.EquipWeapons.rightHand;
					case InventoryType.EquipWeaponLeft:
						return OwningCharacter.EquipWeapons.leftHand;
				}
				return null;
			}
		}
        public short Level { get { return (short)(CharacterItem != null ? CharacterItem.level : 1); } }

        public UICharacterItem uiCharacterItem;

        protected override void UpdateData()
        {
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
        }
	}
}
