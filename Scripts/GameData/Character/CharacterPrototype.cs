using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterPrototype", menuName = "Create GameData/CharacterPrototype")]
public class CharacterPrototype : BaseGameData
{
    public CharacterModel characterModel;
    public CharacterClass characterClass;
}
