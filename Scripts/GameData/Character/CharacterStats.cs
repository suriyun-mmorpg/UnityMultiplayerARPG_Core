namespace MultiplayerARPG
{
    [System.Serializable]
    public partial class CharacterStats
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

        public CharacterStats Add(CharacterStats b)
        {
            CharacterStats result = new CharacterStats();
            result.hp = hp + b.hp;
            result.mp = mp + b.mp;
            result.armor = armor + b.armor;
            result.accuracy = accuracy + b.accuracy;
            result.evasion = evasion + b.evasion;
            result.criRate = criRate + b.criRate;
            result.criDmgRate = criDmgRate + b.criDmgRate;
            result.blockRate = blockRate + b.blockRate;
            result.blockDmgRate = blockDmgRate + b.blockDmgRate;
            result.moveSpeed = moveSpeed + b.moveSpeed;
            result.atkSpeed = atkSpeed + b.atkSpeed;
            result.weightLimit = weightLimit + b.weightLimit;
            result.stamina = stamina + b.stamina;
            result.food = food + b.food;
            result.water = water + b.water;
            this.InvokeInstanceDevExtMethods("Add", result, b);
            return result;
        }

        public CharacterStats Multiply(float multiplier)
        {
            CharacterStats result = new CharacterStats();
            result.hp = hp * multiplier;
            result.mp = mp * multiplier;
            result.armor = armor * multiplier;
            result.accuracy = accuracy * multiplier;
            result.evasion = evasion * multiplier;
            result.criRate = criRate * multiplier;
            result.criDmgRate = criDmgRate * multiplier;
            result.blockRate = blockRate * multiplier;
            result.blockDmgRate = blockDmgRate * multiplier;
            result.moveSpeed = moveSpeed * multiplier;
            result.atkSpeed = atkSpeed * multiplier;
            result.weightLimit = weightLimit * multiplier;
            result.stamina = stamina * multiplier;
            result.food = food * multiplier;
            result.water = water * multiplier;
            this.InvokeInstanceDevExtMethods("Multiply", result, multiplier);
            return result;
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
