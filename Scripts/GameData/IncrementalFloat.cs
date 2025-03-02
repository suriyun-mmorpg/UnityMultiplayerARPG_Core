using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct IncrementalFloat
    {
        [Tooltip("Amount at level 1")]
        public float baseAmount;
        [Tooltip("Increase amount when level > 1 (it will be decreasing when level < 0)")]
        public float amountIncreaseEachLevel;
        [Tooltip("It won't automatically sort by `minLevel`, you have to sort it from low to high to make it calculate properly")]
        public IncrementalFloatByLevel[] amountIncreaseEachLevelByLevels;

        public float GetAmount(int level)
        {
            if (amountIncreaseEachLevelByLevels == null || amountIncreaseEachLevelByLevels.Length == 0)
                return baseAmount + (amountIncreaseEachLevel * (level - (level > 0 ? 1 : 0)));
            float result = baseAmount;
            int countLevel = 2;
            int indexOfIncremental = 0;
            int firstMinLevel = amountIncreaseEachLevelByLevels[indexOfIncremental].minLevel;
            while (countLevel <= level)
            {
                if (countLevel < firstMinLevel)
                    result += amountIncreaseEachLevel;
                else
                    result += amountIncreaseEachLevelByLevels[indexOfIncremental].amountIncreaseEachLevel;
                countLevel++;
                if (indexOfIncremental + 1 < amountIncreaseEachLevelByLevels.Length && countLevel >= amountIncreaseEachLevelByLevels[indexOfIncremental + 1].minLevel)
                    indexOfIncremental++;
            }
            return result;
        }
    }

    [System.Serializable]
    public struct IncrementalFloatByLevel
    {
        public int minLevel;
        public float amountIncreaseEachLevel;
    }
}
