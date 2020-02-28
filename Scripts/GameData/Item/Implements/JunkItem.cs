using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Junk Item", menuName = "Create GameData/Item/Junk Item", order = -4881)]
    public partial class JunkItem : BaseItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_JUNK.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Junk; }
        }
    }
}
