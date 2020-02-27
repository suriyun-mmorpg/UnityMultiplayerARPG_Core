using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Weapon Item", menuName = "Create GameData/Item/Weapon Item", order = -4888)]
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
