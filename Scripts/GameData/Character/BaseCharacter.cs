using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCharacter : BaseGameData
{
    public CharacterModel model;

    [Header("Attributes/Stats")]
    public AttributeIncremental[] attributes;
    public CharacterStatsIncremental stats;
    public ResistanceIncremental[] resistances;
    
    public CharacterStats GetCharacterStats(short level)
    {
        return stats.GetCharacterStats(level);
    }

    public Dictionary<Attribute, short> GetCharacterAttributes(short level)
    {
        return GameDataHelpers.MakeAttributeAmountsDictionary(attributes, new Dictionary<Attribute, short>(), level, 1f);
    }

    public Dictionary<DamageElement, float> GetCharacterResistances(short level)
    {
        return GameDataHelpers.MakeResistanceAmountsDictionary(resistances, new Dictionary<DamageElement, float>(), level, 1f);
    }
}
