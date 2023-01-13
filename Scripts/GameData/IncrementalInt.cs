using UnityEngine;

[System.Serializable]
public struct IncrementalInt
{
    public int baseAmount;
    public int amountIncreaseEachLevel;
    [Tooltip("It won't automatically sort by `minLevel`, you have to sort it from low to high to make it calculate properly")]
    public IncrementalIntByLevel[] amountIncreaseEachLevelByLevels;

    public int GetAmount(int level)
    {
        if (amountIncreaseEachLevelByLevels == null || amountIncreaseEachLevelByLevels.Length == 0)
            return (int)(baseAmount + (amountIncreaseEachLevel * (level - 1)));
        int result = baseAmount;
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
public struct IncrementalIntByLevel
{
    public int minLevel;
    public int amountIncreaseEachLevel;
}