using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct CharacterStats
{
    public float hp;
    public float mp;
    public float accuracy;
    public float evasion;
    public float criHitRate;
    public float criDmgRate;
    public float moveSpeed;
    public float weightLimit;

    public static CharacterStats operator +(CharacterStats a, CharacterStats b)
    {
        var result = new CharacterStats();
        result.hp = a.hp + b.hp;
        result.mp = a.mp + b.mp;
        result.accuracy = a.accuracy + b.accuracy;
        result.evasion = a.evasion + b.evasion;
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
        result.accuracy = a.accuracy * multiplier;
        result.evasion = a.evasion * multiplier;
        result.criHitRate = a.criHitRate * multiplier;
        result.criDmgRate = a.criDmgRate * multiplier;
        result.moveSpeed = a.moveSpeed * multiplier;
        result.weightLimit = a.weightLimit * multiplier;
        return result;
    }
}
