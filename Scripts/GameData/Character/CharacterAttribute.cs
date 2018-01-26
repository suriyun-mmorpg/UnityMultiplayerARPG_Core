using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterAttribute", menuName = "Create GameData/CharacterAttribute")]
public class CharacterAttribute : BaseGameData
{
    public CharacterStats statsIncreaseEachLevel;
    public CharacterStatsPercentage statsPercentageIncreaseEachLevel;
}

[System.Serializable]
public class CharacterAttributeAmount
{
    public CharacterAttribute attribute;
    public int amount;
}