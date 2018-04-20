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
public struct DamageIncremental
{
    [Tooltip("You can leave Damage to be empty to make it as physical damage which won't calculate with resistance stats")]
    public DamageElement damageElement;
    public IncrementalMinMaxFloat amount;
}

[System.Serializable]
public struct DamageEffectivenessAttribute
{
    public Attribute attribute;
    public float effectiveness;
}

[System.Serializable]
public struct DamageInflictionAmount
{
    public DamageElement damageElement;
    public float rate;
}

[System.Serializable]
public struct DamageInflictionIncremental
{
    public DamageElement damageElement;
    public IncrementalFloat rate;
}