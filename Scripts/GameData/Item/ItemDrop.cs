using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemDrop
    {
        public BaseItem item;
        public int minAmount;
        [FormerlySerializedAs("amount")]
        public int maxAmount;
        [Range(0f, 1f)]
        public float dropRate;
    }
}
