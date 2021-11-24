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

        [Category("Equipment Settings")]
        [Header("Armor Settings")]
        [SerializeField]
        private ArmorType armorType = null;
        public ArmorType ArmorType
        {
            get { return armorType; }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddArmorTypes(ArmorType);
        }
    }
}
