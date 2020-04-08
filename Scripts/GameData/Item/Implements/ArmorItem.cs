using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Armor Item", menuName = "Create GameData/Item/Armor Item", order = -4887)]
    public partial class ArmorItem : BaseDefendEquipmentItem, IArmorItem
    {
        public override string TypeTitle
        {
            get { return ArmorType.Title; }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Armor; }
        }

        [SerializeField]
        private ArmorType armorType;
        public ArmorType ArmorType
        {
            get { return armorType; }
        }

        [SerializeField]
        private string equipPosition;
        public string EquipPosition
        {
            get { return equipPosition; }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            // Add armor type
            GameInstance.AddArmorTypes(new ArmorType[] { armorType });
        }
    }
}
