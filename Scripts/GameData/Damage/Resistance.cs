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
    public struct ResistanceRandomAmount
    {
        public DamageElement damageElement;
        public float minAmount;
        public float maxAmount;
        [Range(0, 1f)]
        public float applyRate;

        public bool Apply(int seed)
        {
            return GenericUtils.RandomFloat(seed, 0f, 1f) <= applyRate;
        }

        public ResistanceAmount GetRandomedAmount(int seed)
        {
            return new ResistanceAmount()
            {
                damageElement = damageElement,
                amount = GenericUtils.RandomFloat(seed, minAmount, maxAmount),
            };
        }
    }

    [System.Serializable]
    public struct ResistanceIncremental
    {
        [Tooltip("If `damageElement` is empty it will use default damage element from game instance")]
        public DamageElement damageElement;
        public IncrementalFloat amount;
    }
}

