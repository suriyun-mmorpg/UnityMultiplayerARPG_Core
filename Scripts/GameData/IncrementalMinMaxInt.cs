using UnityEngine;

[System.Serializable]
public struct IncrementalMinMaxInt
{
    public MinMaxInt baseAmount;
    public MinMaxFloat amountIncreaseEachLevel;
    [Tooltip("It won't automatically sort by `minLevel`, you have to sort it from low to high to make it calculate properly")]
    public IncrementalMinMaxIntByLevel[] amountIncreaseEachLevelByLevels;

    public MinMaxInt GetAmount(int level)
    {
        MinMaxFloat result = new MinMaxFloat()
        {
            min = baseAmount.min,
            max = baseAmount.max,
        };
        if (amountIncreaseEachLevelByLevels == null || amountIncreaseEachLevelByLevels.Length == 0)
        {
            result += amountIncreaseEachLevel * (level - 1);
            return new MinMaxInt()
            {
                min = (int)result.min,
                max = (int)result.max,
            };
        }
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
}