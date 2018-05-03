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
    
    [Tooltip("This will be sum with character's radius before find hitting characters")]
    [StringShowConditional(conditionFieldName: "damageType", conditionValue: "Melee")]
    public float hitDistance = 1f;
    [Range(0f, 360f)]
    [StringShowConditional(conditionFieldName: "damageType", conditionValue: "Melee")]
    public float hitFov;
    
    [StringShowConditional(conditionFieldName: "damageType", conditionValue: "Missile")]
    public float missileDistance = 5f;
    [StringShowConditional(conditionFieldName: "damageType", conditionValue: "Missile")]
    public float missileSpeed = 5f;
    [StringShowConditional(conditionFieldName: "damageType", conditionValue: "Missile")]
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

    public float GetFov()
    {
        var fov = 0f;
        switch (damageType)
        {
            case DamageType.Melee:
                fov = hitFov;
                break;
            case DamageType.Missile:
                fov = 15f;
                break;
        }
        return fov;
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