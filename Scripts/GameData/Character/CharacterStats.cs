using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct CharacterStats
{
    public static readonly CharacterStats Empty = new CharacterStats();
    public float hp;
    public float mp;
    public float armor;
    public float accuracy;
    public float evasion;
    public float criHitRate;
    public float criDmgRate;
    public float moveSpeed;
    public float atkSpeed;
    public float weightLimit;

    public bool IsEmpty()
    {
        return Equals(Empty);
    }

    public static CharacterStats operator +(CharacterStats a, CharacterStats b)
    {
        var result = new CharacterStats();
        result.hp = a.hp + b.hp;
        result.mp = a.mp + b.mp;
        result.armor = a.armor + b.armor;
        result.accuracy = a.accuracy + b.accuracy;
        result.evasion = a.evasion + b.evasion;
        result.criHitRate = a.criHitRate + b.criHitRate;
        result.criDmgRate = a.criDmgRate + b.criDmgRate;
        result.moveSpeed = a.moveSpeed + b.moveSpeed;
        result.atkSpeed = a.atkSpeed + b.atkSpeed;
        result.weightLimit = a.weightLimit + b.weightLimit;
        return result;
    }

    public static CharacterStats operator *(CharacterStats a, float multiplier)
    {
        var result = new CharacterStats();
        result.hp = a.hp * multiplier;
        result.mp = a.mp * multiplier;
        result.armor = a.armor * multiplier;
        result.accuracy = a.accuracy * multiplier;
        result.evasion = a.evasion * multiplier;
        result.criHitRate = a.criHitRate * multiplier;
        result.criDmgRate = a.criDmgRate * multiplier;
        result.moveSpeed = a.moveSpeed * multiplier;
        result.atkSpeed = a.atkSpeed * multiplier;
        result.weightLimit = a.weightLimit * multiplier;
        return result;
    }
}

[System.Serializable]
public struct CharacterStatsIncremental
{
    public CharacterStats baseStats;
    public CharacterStats statsIncreaseEachLevel;

    public CharacterStats GetCharacterStats(int level)
    {
        return baseStats + (statsIncreaseEachLevel * level);
    }
}
