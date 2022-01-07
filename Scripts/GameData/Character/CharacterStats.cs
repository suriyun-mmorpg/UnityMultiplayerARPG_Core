using System.Text;

namespace MultiplayerARPG
{
    [System.Serializable]
#pragma warning disable CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
    public partial struct CharacterStats
#pragma warning restore CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
    {
        public static readonly CharacterStats Empty = new CharacterStats();
        public float hp;
        public float hpRecovery;
        public float hpLeechRate;
        public float mp;
        public float mpRecovery;
        public float mpLeechRate;
        public float stamina;
        public float staminaRecovery;
        public float staminaLeechRate;
        public float food;
        public float water;
        public float accuracy;
        public float evasion;
        public float criRate;
        public float criDmgRate;
        public float blockRate;
        public float blockDmgRate;
        public float moveSpeed;
        public float atkSpeed;
        public float weightLimit;
        public float slotLimit;
        public float goldRate;
        public float expRate;

        public CharacterStats Add(CharacterStats b)
        {
            hp = hp + b.hp;
            hpRecovery = hpRecovery + b.hpRecovery;
            hpLeechRate = hpLeechRate + b.hpLeechRate;
            mp = mp + b.mp;
            mpRecovery = mpRecovery + b.mpRecovery;
            mpLeechRate = mpLeechRate + b.mpLeechRate;
            stamina = stamina + b.stamina;
            staminaRecovery = staminaRecovery + b.staminaRecovery;
            staminaLeechRate = staminaLeechRate + b.staminaLeechRate;
            food = food + b.food;
            water = water + b.water;
            accuracy = accuracy + b.accuracy;
            evasion = evasion + b.evasion;
            criRate = criRate + b.criRate;
            criDmgRate = criDmgRate + b.criDmgRate;
            blockRate = blockRate + b.blockRate;
            blockDmgRate = blockDmgRate + b.blockDmgRate;
            moveSpeed = moveSpeed + b.moveSpeed;
            atkSpeed = atkSpeed + b.atkSpeed;
            weightLimit = weightLimit + b.weightLimit;
            slotLimit = slotLimit + b.slotLimit;
            goldRate = goldRate + b.goldRate;
            expRate = expRate + b.expRate;
            return this.InvokeInstanceDevExtMethodsLoopItself("Add", b);
        }

        public CharacterStats Multiply(float multiplier)
        {
            hp = hp * multiplier;
            hpRecovery = hpRecovery * multiplier;
            hpLeechRate = hpLeechRate * multiplier;
            mp = mp * multiplier;
            mpRecovery = mpRecovery * multiplier;
            mpLeechRate = mpLeechRate * multiplier;
            stamina = stamina * multiplier;
            staminaRecovery = staminaRecovery * multiplier;
            staminaLeechRate = staminaLeechRate * multiplier;
            food = food * multiplier;
            water = water * multiplier;
            accuracy = accuracy * multiplier;
            evasion = evasion * multiplier;
            criRate = criRate * multiplier;
            criDmgRate = criDmgRate * multiplier;
            blockRate = blockRate * multiplier;
            blockDmgRate = blockDmgRate * multiplier;
            moveSpeed = moveSpeed * multiplier;
            atkSpeed = atkSpeed * multiplier;
            weightLimit = weightLimit * multiplier;
            slotLimit = slotLimit * multiplier;
            goldRate = goldRate * multiplier;
            expRate = expRate * multiplier;
            return this.InvokeInstanceDevExtMethodsLoopItself("Multiply", multiplier);
        }

        public CharacterStats MultiplyStats(CharacterStats b)
        {
            hp = hp * b.hp;
            hpRecovery = hpRecovery * b.hpRecovery;
            hpLeechRate = hpLeechRate * b.hpLeechRate;
            mp = mp * b.mp;
            mpRecovery = mpRecovery * b.mpRecovery;
            mpLeechRate = mpLeechRate * b.mpLeechRate;
            stamina = stamina * b.stamina;
            staminaRecovery = staminaRecovery * b.staminaRecovery;
            staminaLeechRate = staminaLeechRate * b.staminaLeechRate;
            food = food * b.food;
            water = water * b.water;
            accuracy = accuracy * b.accuracy;
            evasion = evasion * b.evasion;
            criRate = criRate * b.criRate;
            criDmgRate = criDmgRate * b.criDmgRate;
            blockRate = blockRate * b.blockRate;
            blockDmgRate = blockDmgRate * b.blockDmgRate;
            moveSpeed = moveSpeed * b.moveSpeed;
            atkSpeed = atkSpeed * b.atkSpeed;
            weightLimit = weightLimit * b.weightLimit;
            slotLimit = slotLimit * b.slotLimit;
            goldRate = goldRate * b.slotLimit;
            expRate = expRate * b.slotLimit;
            return this.InvokeInstanceDevExtMethodsLoopItself("MultiplyStats", b);
        }

        public static CharacterStats operator +(CharacterStats a, CharacterStats b)
        {
            return a.Add(b);
        }

        public static CharacterStats operator *(CharacterStats a, float multiplier)
        {
            return a.Multiply(multiplier);
        }

        public static CharacterStats operator *(CharacterStats a, CharacterStats b)
        {
            return a.MultiplyStats(b);
        }
    }

    [System.Serializable]
    public struct CharacterStatsIncremental
    {
        public CharacterStats baseStats;
        public CharacterStats statsIncreaseEachLevel;

        public CharacterStats GetCharacterStats(short level)
        {
            CharacterStats result = new CharacterStats();
            result += baseStats;
            result += (statsIncreaseEachLevel * (level - 1));
            return result;
        }
    }
}
