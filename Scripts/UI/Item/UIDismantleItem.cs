using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIDismantleItem : BaseUICharacterItemByIndex
    {
        public void OnUpdateCharacterItems()
        {

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

        public void OnClickRefine()
        {
            if (IndexOfData < 0)
                return;
            OwningCharacter.RequestRefineItem(InventoryType, (short)IndexOfData);
        }
    }
}
