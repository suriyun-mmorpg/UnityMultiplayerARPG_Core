[System.Serializable]
public struct IncrementalMinMaxFloat
{
    public MinMaxFloat baseAmount;
    public MinMaxFloat amountIncreaseEachLevel;

    public MinMaxFloat GetAmount(short level)
    {
        return baseAmount + (amountIncreaseEachLevel * (level - 1));
    }
}
