using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemDrop
    {
        public BaseItem item;
        [Tooltip("Set `minAmount` to <= `0` to not random amount, it will use `maxAmount` as a dropped amount")]
        public int minAmount;
        [FormerlySerializedAs("amount")]
        [Min(1)]
        public int maxAmount;
        [Range(0f, 1f)]
        public float dropRate;
    }
}
