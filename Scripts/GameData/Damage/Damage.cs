using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Melee,
    Missile,
}

[System.Serializable]
public class Damage
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
}
