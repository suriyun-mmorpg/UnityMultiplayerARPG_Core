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
    public short amount;
}

[System.Serializable]
public struct AttributeIncremental
{
    public Attribute attribute;
    public IncrementalShort amount;
}
