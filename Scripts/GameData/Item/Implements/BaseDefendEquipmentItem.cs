using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseDefendEquipmentItem : BaseEquipmentItem, IDefendEquipmentItem
    {
        [SerializeField]
        private ArmorIncremental armorAmount;
        public ArmorIncremental ArmorAmount
        {
            get { return armorAmount; }
        }
    }
}
