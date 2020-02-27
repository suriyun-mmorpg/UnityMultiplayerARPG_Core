using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Socket Enhancer Item", menuName = "Create GameData/Item/Socket Enhancer Item", order = -4880)]
    public class SocketEnhancerItem : BaseItem, ISocketEnhancerItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_SOCKET_ENHANCER.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.SocketEnhancer; }
        }

        [SerializeField]
        private EquipmentBonus socketEnhanceEffect;
        public EquipmentBonus SocketEnhanceEffect
        {
            get { return socketEnhanceEffect; }
        }
    }
}
