[System.Serializable]
public struct IncrementalShort
{
    public short baseAmount;
    public float amountIncreaseEachLevel;

    public short GetAmount(int level)
    {
        return (short)(baseAmount + (amountIncreaseEachLevel * (level - 1)));
    }
}
