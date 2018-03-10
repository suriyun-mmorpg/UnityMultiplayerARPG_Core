using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resistance", menuName = "Create GameData/Resistance")]
public class Resistance : BaseGameData
{
    [Range(0f, 1f)]
    public float maxAmount;
}

[System.Serializable]
public struct ResistanceAmount
{
    public Resistance resistance;
    public float amount;
}

[System.Serializable]
public struct ResistanceIncremental
{
    public Resistance resistance;
    public float baseAmount;
    public float amountIncreaseEachLevel;
}
