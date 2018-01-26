using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterStats
{
    public float hp;
    public float mp;
    public float atk;
    public float atkRate;
    public float def;
    public float criHitRate;
    public float criDmgRate;

    public static CharacterStats operator +(CharacterStats a, CharacterStats b)
    {
        var result = new CharacterStats();
        result.hp = a.hp + b.hp;
        result.mp = a.mp + b.mp;
        result.atk = a.atk + b.atk;
        result.atkRate = a.atkRate + b.atkRate;
        result.def = a.def + b.def;
        result.criHitRate = a.criHitRate + b.criHitRate;
        result.criDmgRate = a.criDmgRate + b.criDmgRate;
        return result;
    }
}

[System.Serializable]
public class CharacterStatsPercentage
{
    public float hp;
    public float mp;
    public float atk;
    public float atkRate;
    public float def;
    public float criHitRate;
    public float criDmgRate;

    public static CharacterStats operator +(CharacterStats a, CharacterStatsPercentage b)
    {
        var result = new CharacterStats();
        result.hp = a.hp + (a.hp * b.hp / 100);
        result.mp = a.mp + (a.hp * b.mp / 100);
        result.atk = a.atk + (a.hp * b.atk / 100);
        result.atkRate = a.atkRate + (a.atkRate * b.atkRate / 100);
        result.def = a.def + (a.def * b.def / 100);
        result.criHitRate = a.criHitRate + (a.criHitRate * b.criHitRate / 100);
        result.criDmgRate = a.criDmgRate + (a.criDmgRate * b.criDmgRate / 100);
        return result;
    }
}