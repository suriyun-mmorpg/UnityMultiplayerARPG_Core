using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct Storage
    {
        [Tooltip("If weight limit <= 0, assume that it is no limit")]
        public short weightLimit;
        [Tooltip("If slot limit <= 0, assume that it is no limit")]
        public short slotLimit;
    }
}
