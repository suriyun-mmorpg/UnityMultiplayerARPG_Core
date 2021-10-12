using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemDrop
    {
        public BaseItem item;
        public short minAmount;
        [FormerlySerializedAs("amount")]
        public short maxAmount;
        [Range(0f, 1f)]
        public float dropRate;
    }
}
