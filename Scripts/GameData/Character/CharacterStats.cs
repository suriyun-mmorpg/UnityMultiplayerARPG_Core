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
    public float criRate;
    public float criDmgRate;
    public float blockRate;
    public float blockDmgRate;
    public float moveSpeed;
    public float atkSpeed;
    public float weightLimit;
    public float stamina;
    public float food;
    public float water;

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
        result.criRate = a.criRate + b.criRate;
        result.criDmgRate = a.criDmgRate + b.criDmgRate;
        result.blockRate = a.blockRate + b.blockRate;
        result.blockDmgRate = a.blockDmgRate + b.blockDmgRate;
        result.moveSpeed = a.moveSpeed + b.moveSpeed;
        result.atkSpeed = a.atkSpeed + b.atkSpeed;
        result.weightLimit = a.weightLimit + b.weightLimit;
        result.stamina = a.stamina + b.stamina;
        result.food = a.food + b.food;
        result.water = a.water + b.water;
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
        result.criRate = a.criRate * multiplier;
        result.criDmgRate = a.criDmgRate * multiplier;
        result.blockRate = a.blockRate * multiplier;
        result.blockDmgRate = a.blockDmgRate * multiplier;
        result.moveSpeed = a.moveSpeed * multiplier;
        result.atkSpeed = a.atkSpeed * multiplier;
        result.weightLimit = a.weightLimit * multiplier;
        result.stamina = a.stamina * multiplier;
        result.food = a.food * multiplier;
        result.water = a.water * multiplier;
        return result;
    }
}

[System.Serializable]
public struct CharacterStatsIncremental
{
    public CharacterStats baseStats;
    public CharacterStats statsIncreaseEachLevel;

    public CharacterStats GetCharacterStats(short level)
    {
        return baseStats + (statsIncreaseEachLevel * (level - 1));
    }
}
