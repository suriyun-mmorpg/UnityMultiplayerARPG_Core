using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCharacterDatabase : BaseGameData
{
    public CharacterModel model;

    [Header("Attributes/Stats")]
    public AttributeIncremental[] attributes;
    public CharacterStatsIncremental stats;
    
    public CharacterStats GetCharacterStats(int level)
    {
        return stats.GetCharacterStats(level);
    }

    public Dictionary<Attribute, int> GetCharacterAttributes(int level)
    {
        return GameDataHelpers.MakeAttributeAmountsDictionary(attributes, new Dictionary<Attribute, int>(), level);
    }
}
