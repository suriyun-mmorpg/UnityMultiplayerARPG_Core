using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemDrop
    {
        public BaseItem item;
        public short amount;
        [Range(0f, 1f)]
        public float dropRate;
    }
}
