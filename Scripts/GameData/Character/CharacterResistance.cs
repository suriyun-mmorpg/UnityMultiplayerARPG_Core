using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterResistance", menuName = "Create GameData/CharacterResistance")]
public class CharacterResistance : BaseGameData
{
    [Range(0f, 1f)]
    public float maxAmount;
}

[System.Serializable]
public class CharacterResistanceAmount
{
    public CharacterResistance resistance;
    public float amount;
}

[System.Serializable]
public class CharacterResistanceIncremental
{
    public CharacterResistance resistance;
    public float baseAmount;
    public float amountIncreaseEachLevel;
}
