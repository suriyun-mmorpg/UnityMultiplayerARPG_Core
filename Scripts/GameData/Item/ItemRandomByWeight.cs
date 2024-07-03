using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemRandomByWeight
    {
        public BaseItem item;
        [Tooltip("Set `minAmount` to <= `0` to not random amount, it will use `maxAmount` as a randomed amount")]
        public int minAmount;
        [FormerlySerializedAs("amount")]
        [Min(1)]
        public int maxAmount;
        public int randomWeight;
    }
}
