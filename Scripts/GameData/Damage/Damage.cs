using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Damage", menuName = "Create GameData/Damage")]
public class Damage : BaseGameData
{
    // TODO: Resistance
}

[System.Serializable]
public class DamageAmount
{
    [Tooltip("You can leave Damage to be empty to make it as physical damage which won't calculate with resistance stats")]
    public Damage damage;
    public float minDamage;
    public float maxDamage;

    public static DamageAmount operator +(DamageAmount a, DamageAmount b)
    {
        var result = new DamageAmount();
        result.minDamage = a.minDamage + b.minDamage;
        result.maxDamage = a.maxDamage + b.maxDamage;
        return result;
    }

    public static DamageAmount operator *(DamageAmount a, float multiplier)
    {
        var result = new DamageAmount();
        result.minDamage = a.minDamage * multiplier;
        result.maxDamage = a.maxDamage * multiplier;
        return result;
    }
}

[System.Serializable]
public class DamageEffectivenessAttribute
{
    public CharacterAttribute attribute;
    public float effectiveness = 1f;
}