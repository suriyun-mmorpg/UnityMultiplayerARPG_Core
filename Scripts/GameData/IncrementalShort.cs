[System.Serializable]
public struct IncrementalShort
{
    public short baseAmount;
    public float amountIncreaseEachLevel;

    public short GetAmount(short level)
    {
        return (short)(baseAmount + (short)(amountIncreaseEachLevel * (level - 1)));
    }
}
