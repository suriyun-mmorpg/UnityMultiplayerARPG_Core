using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ResistanceAmount
    {
        [Tooltip("If `damageElement` is empty it will use default damage element from game instance")]
        public DamageElement damageElement;
        public float amount;
    }

    [System.Serializable]
    public struct ResistanceIncremental
    {
        [Tooltip("If `damageElement` is empty it will use default damage element from game instance")]
        public DamageElement damageElement;
        public IncrementalFloat amount;
    }
}

