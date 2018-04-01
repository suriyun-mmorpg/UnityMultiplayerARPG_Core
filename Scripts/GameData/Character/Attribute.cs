using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attribute", menuName = "Create GameData/Attribute")]
public class Attribute : BaseGameData
{
    public CharacterStats statsIncreaseEachLevel;
}

[System.Serializable]
public struct AttributeAmount
{
    public Attribute attribute;
    public int amount;
}

[System.Serializable]
public struct AttributeIncremental
{
    public Attribute attribute;
    public int baseAmount;
    public float amountIncreaseEachLevel;

    public int GetAmount(int level)
    {
        return baseAmount + (int)(amountIncreaseEachLevel * (level - 1));
    }
}
