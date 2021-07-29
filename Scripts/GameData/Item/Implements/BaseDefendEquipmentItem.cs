using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseDefendEquipmentItem : BaseEquipmentItem, IDefendEquipmentItem
    {
        [Category("Buff/Bonus Settings")]
        [SerializeField]
        private ArmorIncremental armorAmount = default(ArmorIncremental);
        public ArmorIncremental ArmorAmount
        {
            get { return armorAmount; }
        }
    }
}
