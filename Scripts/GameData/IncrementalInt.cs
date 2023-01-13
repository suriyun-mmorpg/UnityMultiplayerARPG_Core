[System.Serializable]
public struct IncrementalInt
{
    public int baseAmount;
    public float amountIncreaseEachLevel;

    public int GetAmount(int level)
    {
        return baseAmount + (int)(amountIncreaseEachLevel * (level - 1));
    }
}
