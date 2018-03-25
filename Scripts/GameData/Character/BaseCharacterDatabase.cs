using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCharacterDatabase : BaseGameData
{
    public CharacterModel model;

    public abstract CharacterStats GetCharacterStats(int level);
}
