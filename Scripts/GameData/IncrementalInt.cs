using UnityEngine;

[System.Serializable]
public struct IncrementalInt
{
    [Tooltip("Amount at level 1")]
    public int baseAmount;
    [Tooltip("Increase amount when level > 1 (it will be decreasing when level < 0)")]
    public float amountIncreaseEachLevel;
    [Tooltip("It won't automatically sort by `minLevel`, you have to sort it from low to high to make it calculate properly")]
    public IncrementalIntByLevel[] amountIncreaseEachLevelByLevels;

    public int GetAmount(int level)
    {
        if (amountIncreaseEachLevelByLevels == null || amountIncreaseEachLevelByLevels.Length == 0)
            return (int)(baseAmount + (amountIncreaseEachLevel * (level - (level > 0 ? 1 : 0))));
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
        return (int)result;
    }
}

[System.Serializable]
public struct IncrementalIntByLevel
{
    public int minLevel;
    public float amountIncreaseEachLevel;
}