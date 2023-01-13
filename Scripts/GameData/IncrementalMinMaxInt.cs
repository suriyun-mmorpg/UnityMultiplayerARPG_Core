[System.Serializable]
public struct IncrementalMinMaxInt
{
    public MinMaxInt baseAmount;
    public MinMaxInt amountIncreaseEachLevel;

    public MinMaxInt GetAmount(int level)
    {
        return baseAmount + (amountIncreaseEachLevel * (level - 1));
    }
}
