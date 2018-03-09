using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterAttribute", menuName = "Create GameData/CharacterAttribute")]
public class CharacterAttribute : BaseGameData
{
    public CharacterStats statsIncreaseEachLevel;
}

[System.Serializable]
public class CharacterAttributeAmount
{
    public CharacterAttribute attribute;
    public int amount;
}

[System.Serializable]
public class CharacterAttributeIncremental
{
    public CharacterAttribute attribute;
    public int baseAmount;
    public float amountIncreaseEachLevel;
}
