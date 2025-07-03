using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct IncrementalMinMaxInt
    {
        [Tooltip("Amount at level 1")]
        public MinMaxInt baseAmount;
        [Tooltip("Increase amount when level > 1 (it will be decreasing when level < 0)")]
        public MinMaxFloat amountIncreaseEachLevel;
        [Tooltip("Percentage rate increase per level (0.05 = +5% per level)")]
        public MinMaxFloat rateIncreaseEachLevel;
        [Tooltip("It won't automatically sort by `minLevel`, you have to sort it from low to high to make it calculate properly")]
        public IncrementalMinMaxIntByLevel[] amountIncreaseEachLevelByLevels;

        public MinMaxInt GetAmount(int level)
        {
            MinMaxFloat result = new MinMaxFloat()
            {
                min = baseAmount.min,
                max = baseAmount.max,
            };

            if (level <= 1)
            {
                return new MinMaxInt()
                {
                    min = (int)result.min,
                    max = (int)result.max,
                };
            }

            int countLevel = 2;
            int indexOfIncremental = 0;
            int firstMinLevel = amountIncreaseEachLevelByLevels != null && amountIncreaseEachLevelByLevels.Length > 0
                ? amountIncreaseEachLevelByLevels[indexOfIncremental].minLevel
                : int.MaxValue;

            while (countLevel <= level)
            {
                MinMaxFloat flat = amountIncreaseEachLevel;
                MinMaxFloat rate = rateIncreaseEachLevel;

                if (countLevel >= firstMinLevel &&
                    amountIncreaseEachLevelByLevels != null &&
                    amountIncreaseEachLevelByLevels.Length > 0)
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

            return new MinMaxInt()
            {
                min = (int)result.min,
                max = (int)result.max,
            };
        }
    }

    [System.Serializable]
    public struct IncrementalMinMaxIntByLevel
    {
        public int minLevel;
        public MinMaxFloat amountIncreaseEachLevel;
        public MinMaxFloat rateIncreaseEachLevel;
    }
}
