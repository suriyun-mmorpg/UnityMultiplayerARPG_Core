[System.Serializable]
public struct IncrementalMinMaxFloat
{
    public MinMaxFloat baseAmount;
    public MinMaxFloat amountIncreaseEachLevel;

    public MinMaxFloat GetAmount(int level)
    {
        return baseAmount + (amountIncreaseEachLevel * (level - 1));
    }
}
