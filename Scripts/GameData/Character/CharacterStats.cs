using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct CharacterStats
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

        public CharacterStats Add(CharacterStats b)
        {
            hp = hp + b.hp;
            mp = mp + b.mp;
            armor = armor + b.armor;
            accuracy = accuracy + b.accuracy;
            evasion = evasion + b.evasion;
            criRate = criRate + b.criRate;
            criDmgRate = criDmgRate + b.criDmgRate;
            blockRate = blockRate + b.blockRate;
            blockDmgRate = blockDmgRate + b.blockDmgRate;
            moveSpeed = moveSpeed + b.moveSpeed;
            atkSpeed = atkSpeed + b.atkSpeed;
            weightLimit = weightLimit + b.weightLimit;
            stamina = stamina + b.stamina;
            food = food + b.food;
            water = water + b.water;
            this.InvokeAddOnMethods("Add");
            return this;
        }

        public CharacterStats Multiply(float multiplier)
        {
            hp = hp * multiplier;
            mp = mp * multiplier;
            armor = armor * multiplier;
            accuracy = accuracy * multiplier;
            evasion = evasion * multiplier;
            criRate = criRate * multiplier;
            criDmgRate = criDmgRate * multiplier;
            blockRate = blockRate * multiplier;
            blockDmgRate = blockDmgRate * multiplier;
            moveSpeed = moveSpeed * multiplier;
            atkSpeed = atkSpeed * multiplier;
            weightLimit = weightLimit * multiplier;
            stamina = stamina * multiplier;
            food = food * multiplier;
            water = water * multiplier;
            this.InvokeAddOnMethods("Multiply");
            return this;
        }

        public static CharacterStats operator +(CharacterStats a, CharacterStats b)
        {
            return a.Add(b);
        }

        public static CharacterStats operator *(CharacterStats a, float multiplier)
        {
            return a.Multiply(multiplier);
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
}
