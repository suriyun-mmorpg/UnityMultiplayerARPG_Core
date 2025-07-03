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
        [Tooltip("Percentage rate increase per level (0.05 = +5% per level)")]
        public float rateIncreaseEachLevel;
        [Tooltip("It won't automatically sort by `minLevel`, you have to sort it from low to high to make it calculate properly")]
        public IncrementalFloatByLevel[] amountIncreaseEachLevelByLevels;

        public float GetAmount(int level)
        {
            if (level <= 1)
                return baseAmount;

            float result = baseAmount;
            int countLevel = 2;
            int indexOfIncremental = 0;
            int firstMinLevel = amountIncreaseEachLevelByLevels != null && amountIncreaseEachLevelByLevels.Length > 0
                ? amountIncreaseEachLevelByLevels[indexOfIncremental].minLevel
                : int.MaxValue;

            while (countLevel <= level)
            {
                float flat = amountIncreaseEachLevel;
                float rate = rateIncreaseEachLevel;

                if (countLevel >= firstMinLevel && amountIncreaseEachLevelByLevels != null && amountIncreaseEachLevelByLevels.Length > 0)
                {
                    flat = amountIncreaseEachLevelByLevels[indexOfIncremental].amountIncreaseEachLevel;
                    rate = amountIncreaseEachLevelByLevels[indexOfIncremental].rateIncreaseEachLevel;
                }

                result += flat;
                result += result * rate;

                countLevel++;

                if (amountIncreaseEachLevelByLevels != null &&
                    indexOfIncremental + 1 < amountIncreaseEachLevelByLevels.Length &&
                    countLevel >= amountIncreaseEachLevelByLevels[indexOfIncremental + 1].minLevel)
                {
                    indexOfIncremental++;
                }
            }

            return result;
        }
    }

    [System.Serializable]
    public struct IncrementalFloatByLevel
    {
        public int minLevel;
        public float amountIncreaseEachLevel;
        public float rateIncreaseEachLevel;
    }
}
