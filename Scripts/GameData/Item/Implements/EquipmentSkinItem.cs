using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Equipment Skin Item", menuName = "Create GameData/Item/Equipment Skin Item", order = -4877)]
    public partial class EquipmentSkinItem : BaseEquipmentItem, IEquipmentSkinItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_EQUIPMENT_SKIN.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.EquipmentSkin; }
        }
    }
}
