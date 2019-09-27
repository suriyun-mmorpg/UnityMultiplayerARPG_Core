using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ArmorAmount
    {
        [Tooltip("If `damageElement` is empty it will use default damage element from game instance")]
        public DamageElement damageElement;
        public float amount;
    }

    [System.Serializable]
    public struct ArmorIncremental
    {
        [Tooltip("If `damageElement` is empty it will use default damage element from game instance")]
        public DamageElement damageElement;
        public IncrementalFloat amount;
    }
}
