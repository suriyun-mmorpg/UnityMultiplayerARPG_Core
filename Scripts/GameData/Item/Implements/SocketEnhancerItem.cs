using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Socket Enhancer Item", menuName = "Create GameData/Item/Socket Enhancer Item", order = -4880)]
    public class SocketEnhancerItem : BaseItem, ISocketEnhancerItem
    {
        [SerializeField]
        private EquipmentBonus socketEnhanceEffect;
        public EquipmentBonus SocketEnhanceEffect
        {
            get { return socketEnhanceEffect; }
        }
    }
}
