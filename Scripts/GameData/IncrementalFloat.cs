using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct IncrementalFloat
{
    public int baseAmount;
    public float amountIncreaseEachLevel;

    public float GetAmount(int level)
    {
        return baseAmount + (amountIncreaseEachLevel * (level - 1));
    }
}
