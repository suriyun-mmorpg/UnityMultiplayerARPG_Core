using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemRandomByWeight
    {
        public BaseItem item;
        [Min(1)]
        public int amount;
        public int randomWeight;
    }
}
