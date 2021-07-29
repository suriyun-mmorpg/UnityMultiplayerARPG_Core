using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Shield Item", menuName = "Create GameData/Item/Shield Item", order = -4888)]
    public partial class ShieldItem : BaseDefendEquipmentItem, IShieldItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_SHIELD.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Shield; }
        }
    }
}
