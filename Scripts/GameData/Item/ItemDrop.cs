using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemDrop
    {
        public BaseItem item;
        [Min(1)]
        public int minAmount;
        [FormerlySerializedAs("amount")]
        [Min(1)]
        public int maxAmount;
        [Range(0f, 1f)]
        public float dropRate;
    }
}
