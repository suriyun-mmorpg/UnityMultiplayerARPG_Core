using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterStats
{
    public float hp;
    public float mp;
    public float atkRate;
    public float def;
    public float criHitRate;
    public float criDmgRate;
    public float moveSpeed;
    public float weightLimit;

    public static CharacterStats operator +(CharacterStats a, CharacterStats b)
    {
        var result = new CharacterStats();
        result.hp = a.hp + b.hp;
        result.mp = a.mp + b.mp;
        result.atkRate = a.atkRate + b.atkRate;
        result.def = a.def + b.def;
        result.criHitRate = a.criHitRate + b.criHitRate;
        result.criDmgRate = a.criDmgRate + b.criDmgRate;
        result.moveSpeed = a.moveSpeed + b.moveSpeed;
        result.weightLimit = a.weightLimit + b.weightLimit;
        return result;
    }

    public static CharacterStats operator *(CharacterStats a, float multiplier)
    {
        var result = new CharacterStats();
        result.hp = a.hp * multiplier;
        result.mp = a.mp * multiplier;
        result.atkRate = a.atkRate * multiplier;
        result.def = a.def * multiplier;
        result.criHitRate = a.criHitRate * multiplier;
        result.criDmgRate = a.criDmgRate * multiplier;
        result.moveSpeed = a.moveSpeed * multiplier;
        result.weightLimit = a.weightLimit * multiplier;
        return result;
    }
}
