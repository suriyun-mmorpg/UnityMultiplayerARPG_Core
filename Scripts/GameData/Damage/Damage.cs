using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Melee,
    Missile,
}

[System.Serializable]
public class DamageInfo
{
    public DamageType damageType;

    [Header("Melee damage settings")]
    [Tooltip("This will be sum with character's radius before find hitting characters")]
    public float hitDistance = 1f;
    [Range(0f, 360f)]
    public float hitFov;

    [Header("Missile damage settings")]
    public float missileDistance = 5f;
    public float missileSpeed = 5f;
    public MissileDamageEntity missileDamageEntity;
    
    public float GetDistance()
    {
        var distance = 0f;
        switch (damageType)
        {
            case DamageType.Melee:
                distance = hitDistance;
                break;
            case DamageType.Missile:
                distance = missileDistance;
                break;
        }
        return distance;
    }
}

[System.Serializable]
public struct DamageAmount
{
    public float minDamage;
    public float maxDamage;

    public static DamageAmount operator +(DamageAmount a, DamageAmount b)
    {
        var result = new DamageAmount();
        result.minDamage = a.minDamage + b.minDamage;
        result.maxDamage = a.maxDamage + b.maxDamage;
        return result;
    }

    public static DamageAmount operator +(DamageAmount a, float damage)
    {
        var result = new DamageAmount();
        result.minDamage = a.minDamage + damage;
        result.maxDamage = a.maxDamage + damage;
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
public struct DamageAttribute
{
    [Tooltip("You can leave Damage to be empty to make it as physical damage which won't calculate with resistance stats")]
    public DamageElement damageElement;
    public DamageAmount baseDamageAmount;
    public DamageAmount damageAmountIncreaseEachLevel;
}

[System.Serializable]
public struct DamageEffectivenessAttribute
{
    public Attribute attribute;
    public float effectiveness;
}
