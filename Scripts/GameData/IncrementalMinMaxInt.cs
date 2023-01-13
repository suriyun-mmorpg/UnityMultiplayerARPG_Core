using UnityEngine;

[System.Serializable]
public struct IncrementalMinMaxInt
{
    public MinMaxInt baseAmount;
    public MinMaxInt amountIncreaseEachLevel;
    [Tooltip("It won't automatically sort by `minLevel`, you have to sort it from low to high to make it calculate properly")]
    public IncrementalMinMaxIntByLevel[] amountIncreaseEachLevelByLevels;

    public MinMaxInt GetAmount(int level)
    {
        if (amountIncreaseEachLevelByLevels == null || amountIncreaseEachLevelByLevels.Length == 0)
            return baseAmount + (amountIncreaseEachLevel * (level - 1));
        MinMaxInt result = baseAmount;
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
public struct IncrementalMinMaxIntByLevel
{
    public int minLevel;
    public MinMaxInt amountIncreaseEachLevel;
}