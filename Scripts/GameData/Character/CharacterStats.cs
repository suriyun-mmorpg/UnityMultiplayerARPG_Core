using System.Text;
using UnityEngine;

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
        public float itemDropRate;

        public static CharacterStats operator +(CharacterStats a, CharacterStats b)
        {
            a.hp = a.hp + b.hp;
            a.hpRecovery = a.hpRecovery + b.hpRecovery;
            a.hpLeechRate = a.hpLeechRate + b.hpLeechRate;
            a.mp = a.mp + b.mp;
            a.mpRecovery = a.mpRecovery + b.mpRecovery;
            a.mpLeechRate = a.mpLeechRate + b.mpLeechRate;
            a.stamina = a.stamina + b.stamina;
            a.staminaRecovery = a.staminaRecovery + b.staminaRecovery;
            a.staminaLeechRate = a.staminaLeechRate + b.staminaLeechRate;
            a.food = a.food + b.food;
            a.water = a.water + b.water;
            a.accuracy = a.accuracy + b.accuracy;
            a.evasion = a.evasion + b.evasion;
            a.criRate = a.criRate + b.criRate;
            a.criDmgRate = a.criDmgRate + b.criDmgRate;
            a.blockRate = a.blockRate + b.blockRate;
            a.blockDmgRate = a.blockDmgRate + b.blockDmgRate;
            a.moveSpeed = a.moveSpeed + b.moveSpeed;
            a.atkSpeed = a.atkSpeed + b.atkSpeed;
            a.weightLimit = a.weightLimit + b.weightLimit;
            a.slotLimit = a.slotLimit + b.slotLimit;
            a.goldRate = a.goldRate + b.goldRate;
            a.expRate = a.expRate + b.expRate;
            a.itemDropRate = a.itemDropRate + b.itemDropRate;
            if (GameExtensionInstance.onAddCharacterStats != null)
                GameExtensionInstance.onAddCharacterStats(ref a, b);
            return a;
        }

        public static CharacterStats operator *(CharacterStats a, float multiplier)
        {
            a.hp = a.hp * multiplier;
            a.hpRecovery = a.hpRecovery * multiplier;
            a.hpLeechRate = a.hpLeechRate * multiplier;
            a.mp = a.mp * multiplier;
            a.mpRecovery = a.mpRecovery * multiplier;
            a.mpLeechRate = a.mpLeechRate * multiplier;
            a.stamina = a.stamina * multiplier;
            a.staminaRecovery = a.staminaRecovery * multiplier;
            a.staminaLeechRate = a.staminaLeechRate * multiplier;
            a.food = a.food * multiplier;
            a.water = a.water * multiplier;
            a.accuracy = a.accuracy * multiplier;
            a.evasion = a.evasion * multiplier;
            a.criRate = a.criRate * multiplier;
            a.criDmgRate = a.criDmgRate * multiplier;
            a.blockRate = a.blockRate * multiplier;
            a.blockDmgRate = a.blockDmgRate * multiplier;
            a.moveSpeed = a.moveSpeed * multiplier;
            a.atkSpeed = a.atkSpeed * multiplier;
            a.weightLimit = a.weightLimit * multiplier;
            a.slotLimit = a.slotLimit * multiplier;
            a.goldRate = a.goldRate * multiplier;
            a.expRate = a.expRate * multiplier;
            a.itemDropRate = a.itemDropRate * multiplier;
            if (GameExtensionInstance.onMultiplyCharacterStatsWithNumber != null)
                GameExtensionInstance.onMultiplyCharacterStatsWithNumber(ref a, multiplier);
            return a;
        }

        public static CharacterStats operator *(CharacterStats a, CharacterStats b)
        {
            a.hp = a.hp * b.hp;
            a.hpRecovery = a.hpRecovery * b.hpRecovery;
            a.hpLeechRate = a.hpLeechRate * b.hpLeechRate;
            a.mp = a.mp * b.mp;
            a.mpRecovery = a.mpRecovery * b.mpRecovery;
            a.mpLeechRate = a.mpLeechRate * b.mpLeechRate;
            a.stamina = a.stamina * b.stamina;
            a.staminaRecovery = a.staminaRecovery * b.staminaRecovery;
            a.staminaLeechRate = a.staminaLeechRate * b.staminaLeechRate;
            a.food = a.food * b.food;
            a.water = a.water * b.water;
            a.accuracy = a.accuracy * b.accuracy;
            a.evasion = a.evasion * b.evasion;
            a.criRate = a.criRate * b.criRate;
            a.criDmgRate = a.criDmgRate * b.criDmgRate;
            a.blockRate = a.blockRate * b.blockRate;
            a.blockDmgRate = a.blockDmgRate * b.blockDmgRate;
            a.moveSpeed = a.moveSpeed * b.moveSpeed;
            a.atkSpeed = a.atkSpeed * b.atkSpeed;
            a.weightLimit = a.weightLimit * b.weightLimit;
            a.slotLimit = a.slotLimit * b.slotLimit;
            a.goldRate = a.goldRate * b.goldRate;
            a.expRate = a.expRate * b.expRate;
            a.itemDropRate = a.itemDropRate * b.itemDropRate;
            if (GameExtensionInstance.onMultiplyCharacterStats != null)
                GameExtensionInstance.onMultiplyCharacterStats(ref a, b);
            return a;
        }
    }

    [System.Serializable]
    public struct CharacterStatsIncremental
    {
        public CharacterStats baseStats;
        public CharacterStats statsIncreaseEachLevel;
        [Tooltip("It won't automatically sort by `minLevel`, you have to sort it from low to high to make it calculate properly")]
        public CharacterStatsIncrementalByLevel[] statsIncreaseEachLevelByLevels;

        public CharacterStats GetCharacterStats(int level)
        {
            if (statsIncreaseEachLevelByLevels == null || statsIncreaseEachLevelByLevels.Length == 0)
                return baseStats + (statsIncreaseEachLevel * (level - 1));
            CharacterStats result = baseStats;
            int countLevel = 2;
            int indexOfIncremental = 0;
            int firstMinLevel = statsIncreaseEachLevelByLevels[indexOfIncremental].minLevel;
            while (countLevel <= level)
            {
                if (countLevel < firstMinLevel)
                    result += statsIncreaseEachLevel;
                else
                    result += statsIncreaseEachLevelByLevels[indexOfIncremental].statsIncreaseEachLevel;
                countLevel++;
                if (indexOfIncremental + 1 < statsIncreaseEachLevelByLevels.Length && countLevel >= statsIncreaseEachLevelByLevels[indexOfIncremental + 1].minLevel)
                    indexOfIncremental++;
            }
            return result;
        }
    }

    [System.Serializable]
    public struct CharacterStatsIncrementalByLevel
    {
        public int minLevel;
        public CharacterStats statsIncreaseEachLevel;
    }
}
