using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    [StructLayout(LayoutKind.Auto)]
    public partial struct CharacterStats
    {
        public static readonly CharacterStats Empty = new CharacterStats();
        [CharacterStatTextGen(false)]
        public float hp;
        [CharacterStatTextGen(false)]
        public float hpRecovery;
        [CharacterStatTextGen(true)]
        public float hpLeechRate;
        [CharacterStatTextGen(false)]
        public float mp;
        [CharacterStatTextGen(false)]
        public float mpRecovery;
        [CharacterStatTextGen(true)]
        public float mpLeechRate;
        [CharacterStatTextGen(false)]
        public float stamina;
        [CharacterStatTextGen(false)]
        public float staminaRecovery;
        [CharacterStatTextGen(true)]
        public float staminaLeechRate;
        [CharacterStatTextGen(false)]
        public float food;
        [CharacterStatTextGen(false)]
        public float water;
        [CharacterStatTextGen(false)]
        public float accuracy;
        [CharacterStatTextGen(false)]
        public float evasion;
        [CharacterStatTextGen("criticalRate", true)]
        public float criRate;
        [CharacterStatTextGen("criticalDamageRate", true)]
        public float criDmgRate;
        [CharacterStatTextGen(true)]
        public float blockRate;
        [CharacterStatTextGen("blockDamageRate", true)]
        public float blockDmgRate;
        [CharacterStatTextGen(false)]
        public float moveSpeed;
        [CharacterStatTextGen(false)]
        public float sprintSpeed;
        [CharacterStatTextGen("attackSpeed", false)]
        public float atkSpeed;
        [CharacterStatTextGen("weight", false)]
        public float weightLimit;
        [CharacterStatTextGen("slot", false)]
        public float slotLimit;
        [CharacterStatTextGen(true)]
        public float goldRate;
        [CharacterStatTextGen(true)]
        public float expRate;
        [CharacterStatTextGen(true)]
        public float itemDropRate;
        [CharacterStatTextGen(false)]
        public float jumpHeight;
        [CharacterStatTextGen(true)]
        public float headDamageAbsorbs;
        [CharacterStatTextGen(true)]
        public float bodyDamageAbsorbs;
        [CharacterStatTextGen(true)]
        public float fallDamageAbsorbs;
        [CharacterStatTextGen(true)]
        public float gravityRate;
        [CharacterStatTextGen(false)]
        public float protectedSlotLimit;
        [FormerlySerializedAs("ammoCapacity")]
        [CharacterStatTextGen(false)]
        public float ammoCapacityModifier;
        [CharacterStatTextGen(true)]
        public float ammoCapacityRate;
        [CharacterStatTextGen(false)]
        public float recoilModifier;
        [CharacterStatTextGen(false)]
        public float recoilYawModifier;
        [CharacterStatTextGen(false)]
        public float recoilRollModifier;
        [CharacterStatTextGen(true)]
        public float recoilRate;
        [CharacterStatTextGen(true)]
        public float recoilYawRate;
        [CharacterStatTextGen(true)]
        public float recoilRollRate;
        [CharacterStatTextGen(false)]
        public float rateOfFireModifier;
        [CharacterStatTextGen(true)]
        public float rateOfFireRate;
        [FormerlySerializedAs("reloadDuration")]
        [CharacterStatTextGen(false)]
        public float reloadDurationModifier;
        [CharacterStatTextGen(true)]
        public float reloadDurationRate;
        [FormerlySerializedAs("fireSpreadRange")]
        [CharacterStatTextGen(false)]
        public float fireSpreadRangeModifier;
        [CharacterStatTextGen(true)]
        public float fireSpreadRangeRate;
        [FormerlySerializedAs("fireSpread")]
        [CharacterStatTextGen(false)]
        public float fireSpreadModifier;
        [CharacterStatTextGen(true)]
        public float fireSpreadRate;
        [CharacterStatTextGen(false)]
        public float decreaseFoodDecreation;
        [CharacterStatTextGen(false)]
        public float decreaseWaterDecreation;
        [CharacterStatTextGen(false)]
        public float decreaseStaminaDecreation;
        [CharacterStatTextGen(true)]
        public float buyItemPriceRate;
        [CharacterStatTextGen(true)]
        public float sellItemPriceRate;

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
            a.sprintSpeed = a.sprintSpeed + b.sprintSpeed;
            a.atkSpeed = a.atkSpeed + b.atkSpeed;
            a.weightLimit = a.weightLimit + b.weightLimit;
            a.slotLimit = a.slotLimit + b.slotLimit;
            a.goldRate = a.goldRate + b.goldRate;
            a.expRate = a.expRate + b.expRate;
            a.itemDropRate = a.itemDropRate + b.itemDropRate;
            a.jumpHeight = a.jumpHeight + b.jumpHeight;
            a.headDamageAbsorbs = a.headDamageAbsorbs + b.headDamageAbsorbs;
            a.bodyDamageAbsorbs = a.bodyDamageAbsorbs + b.bodyDamageAbsorbs;
            a.fallDamageAbsorbs = a.fallDamageAbsorbs + b.fallDamageAbsorbs;
            a.gravityRate = a.gravityRate + b.gravityRate;
            a.protectedSlotLimit = a.protectedSlotLimit + b.protectedSlotLimit;
            a.ammoCapacityModifier = a.ammoCapacityModifier + b.ammoCapacityModifier;
            a.ammoCapacityRate = a.ammoCapacityRate + b.ammoCapacityRate;
            a.recoilModifier = a.recoilModifier + b.recoilModifier;
            a.recoilYawModifier = a.recoilYawModifier + b.recoilYawModifier;
            a.recoilRollModifier = a.recoilRollModifier + b.recoilRollModifier;
            a.recoilRate = a.recoilRate + b.recoilRate;
            a.recoilYawRate = a.recoilYawRate + b.recoilYawRate;
            a.recoilRollRate = a.recoilRollRate + b.recoilRollRate;
            a.rateOfFireModifier = a.rateOfFireModifier + b.rateOfFireModifier;
            a.rateOfFireRate = a.rateOfFireRate + b.rateOfFireRate;
            a.reloadDurationModifier = a.reloadDurationModifier + b.reloadDurationModifier;
            a.reloadDurationRate = a.reloadDurationRate + b.reloadDurationRate;
            a.fireSpreadRangeModifier = a.fireSpreadRangeModifier + b.fireSpreadRangeModifier;
            a.fireSpreadRangeRate = a.fireSpreadRangeRate + b.fireSpreadRangeRate;
            a.fireSpreadModifier = a.fireSpreadModifier + b.fireSpreadModifier;
            a.fireSpreadRate = a.fireSpreadRate + b.fireSpreadRate;
            a.decreaseFoodDecreation = a.decreaseFoodDecreation + b.decreaseFoodDecreation;
            a.decreaseWaterDecreation = a.decreaseWaterDecreation + b.decreaseWaterDecreation;
            a.decreaseStaminaDecreation = a.decreaseStaminaDecreation + b.decreaseStaminaDecreation;
            a.buyItemPriceRate = a.buyItemPriceRate + b.buyItemPriceRate;
            a.sellItemPriceRate = a.sellItemPriceRate + b.sellItemPriceRate;
            if (GameExtensionInstance.onIncreaseCharacterStats != null)
                GameExtensionInstance.onIncreaseCharacterStats(ref a, b);
            return a;
        }

        public static CharacterStats operator -(CharacterStats a, CharacterStats b)
        {
            a.hp = a.hp - b.hp;
            a.hpRecovery = a.hpRecovery - b.hpRecovery;
            a.hpLeechRate = a.hpLeechRate - b.hpLeechRate;
            a.mp = a.mp - b.mp;
            a.mpRecovery = a.mpRecovery - b.mpRecovery;
            a.mpLeechRate = a.mpLeechRate - b.mpLeechRate;
            a.stamina = a.stamina - b.stamina;
            a.staminaRecovery = a.staminaRecovery - b.staminaRecovery;
            a.staminaLeechRate = a.staminaLeechRate - b.staminaLeechRate;
            a.food = a.food - b.food;
            a.water = a.water - b.water;
            a.accuracy = a.accuracy - b.accuracy;
            a.evasion = a.evasion - b.evasion;
            a.criRate = a.criRate - b.criRate;
            a.criDmgRate = a.criDmgRate - b.criDmgRate;
            a.blockRate = a.blockRate - b.blockRate;
            a.blockDmgRate = a.blockDmgRate - b.blockDmgRate;
            a.moveSpeed = a.moveSpeed - b.moveSpeed;
            a.sprintSpeed = a.sprintSpeed - b.sprintSpeed;
            a.atkSpeed = a.atkSpeed - b.atkSpeed;
            a.weightLimit = a.weightLimit - b.weightLimit;
            a.slotLimit = a.slotLimit - b.slotLimit;
            a.goldRate = a.goldRate - b.goldRate;
            a.expRate = a.expRate - b.expRate;
            a.itemDropRate = a.itemDropRate - b.itemDropRate;
            a.jumpHeight = a.jumpHeight - b.jumpHeight;
            a.headDamageAbsorbs = a.headDamageAbsorbs - b.headDamageAbsorbs;
            a.bodyDamageAbsorbs = a.bodyDamageAbsorbs - b.bodyDamageAbsorbs;
            a.fallDamageAbsorbs = a.fallDamageAbsorbs - b.fallDamageAbsorbs;
            a.gravityRate = a.gravityRate - b.gravityRate;
            a.protectedSlotLimit = a.protectedSlotLimit - b.protectedSlotLimit;
            a.ammoCapacityModifier = a.ammoCapacityModifier - b.ammoCapacityModifier;
            a.ammoCapacityRate = a.ammoCapacityRate - b.ammoCapacityRate;
            a.recoilModifier = a.recoilModifier - b.recoilModifier;
            a.recoilYawModifier = a.recoilYawModifier - b.recoilYawModifier;
            a.recoilRollModifier = a.recoilRollModifier - b.recoilRollModifier;
            a.recoilRate = a.recoilRate - b.recoilRate;
            a.recoilYawRate = a.recoilYawRate - b.recoilYawRate;
            a.recoilRollRate = a.recoilRollRate - b.recoilRollRate;
            a.rateOfFireModifier = a.rateOfFireModifier - b.rateOfFireModifier;
            a.rateOfFireRate = a.rateOfFireRate - b.rateOfFireRate;
            a.reloadDurationModifier = a.reloadDurationModifier - b.reloadDurationModifier;
            a.reloadDurationRate = a.reloadDurationRate - b.reloadDurationRate;
            a.fireSpreadRangeModifier = a.fireSpreadRangeModifier - b.fireSpreadRangeModifier;
            a.fireSpreadRangeRate = a.fireSpreadRangeRate - b.fireSpreadRangeRate;
            a.fireSpreadModifier = a.fireSpreadModifier - b.fireSpreadModifier;
            a.fireSpreadRate = a.fireSpreadRate - b.fireSpreadRate;
            a.decreaseFoodDecreation = a.decreaseFoodDecreation - b.decreaseFoodDecreation;
            a.decreaseWaterDecreation = a.decreaseWaterDecreation - b.decreaseWaterDecreation;
            a.decreaseStaminaDecreation = a.decreaseStaminaDecreation - b.decreaseStaminaDecreation;
            a.buyItemPriceRate = a.buyItemPriceRate - b.buyItemPriceRate;
            a.sellItemPriceRate = a.sellItemPriceRate - b.sellItemPriceRate;
            if (GameExtensionInstance.onDecreaseCharacterStats != null)
                GameExtensionInstance.onDecreaseCharacterStats(ref a, b);
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
            a.sprintSpeed = a.sprintSpeed * multiplier;
            a.atkSpeed = a.atkSpeed * multiplier;
            a.weightLimit = a.weightLimit * multiplier;
            a.slotLimit = a.slotLimit * multiplier;
            a.goldRate = a.goldRate * multiplier;
            a.expRate = a.expRate * multiplier;
            a.itemDropRate = a.itemDropRate * multiplier;
            a.jumpHeight = a.jumpHeight * multiplier;
            a.headDamageAbsorbs = a.headDamageAbsorbs * multiplier;
            a.bodyDamageAbsorbs = a.bodyDamageAbsorbs * multiplier;
            a.fallDamageAbsorbs = a.fallDamageAbsorbs * multiplier;
            a.gravityRate = a.gravityRate * multiplier;
            a.protectedSlotLimit = a.protectedSlotLimit * multiplier;
            a.ammoCapacityModifier = a.ammoCapacityModifier * multiplier;
            a.ammoCapacityRate = a.ammoCapacityRate * multiplier;
            a.recoilModifier = a.recoilModifier * multiplier;
            a.recoilYawModifier = a.recoilYawModifier * multiplier;
            a.recoilRollModifier = a.recoilRollModifier * multiplier;
            a.recoilRate = a.recoilRate * multiplier;
            a.recoilYawRate = a.recoilYawRate * multiplier;
            a.recoilRollRate = a.recoilRollRate * multiplier;
            a.rateOfFireModifier = a.rateOfFireModifier * multiplier;
            a.rateOfFireRate = a.rateOfFireRate * multiplier;
            a.reloadDurationModifier = a.reloadDurationModifier * multiplier;
            a.reloadDurationRate = a.reloadDurationRate * multiplier;
            a.fireSpreadRangeModifier = a.fireSpreadRangeModifier * multiplier;
            a.fireSpreadRangeRate = a.fireSpreadRangeRate * multiplier;
            a.fireSpreadModifier = a.fireSpreadModifier * multiplier;
            a.fireSpreadRate = a.fireSpreadRate * multiplier;
            a.decreaseFoodDecreation = a.decreaseFoodDecreation * multiplier;
            a.decreaseWaterDecreation = a.decreaseWaterDecreation * multiplier;
            a.decreaseStaminaDecreation = a.decreaseStaminaDecreation * multiplier;
            a.buyItemPriceRate = a.buyItemPriceRate * multiplier;
            a.sellItemPriceRate = a.sellItemPriceRate * multiplier;
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
            a.sprintSpeed = a.sprintSpeed * b.sprintSpeed;
            a.atkSpeed = a.atkSpeed * b.atkSpeed;
            a.weightLimit = a.weightLimit * b.weightLimit;
            a.slotLimit = a.slotLimit * b.slotLimit;
            a.goldRate = a.goldRate * b.goldRate;
            a.expRate = a.expRate * b.expRate;
            a.itemDropRate = a.itemDropRate * b.itemDropRate;
            a.jumpHeight = a.jumpHeight * b.jumpHeight;
            a.headDamageAbsorbs = a.headDamageAbsorbs * b.headDamageAbsorbs;
            a.bodyDamageAbsorbs = a.bodyDamageAbsorbs * b.bodyDamageAbsorbs;
            a.fallDamageAbsorbs = a.fallDamageAbsorbs * b.fallDamageAbsorbs;
            a.gravityRate = a.gravityRate * b.gravityRate;
            a.protectedSlotLimit = a.protectedSlotLimit * b.protectedSlotLimit;
            a.ammoCapacityModifier = a.ammoCapacityModifier * b.ammoCapacityModifier;
            a.ammoCapacityRate = a.ammoCapacityRate * b.ammoCapacityRate;
            a.recoilModifier = a.recoilModifier * b.recoilModifier;
            a.recoilYawModifier = a.recoilYawModifier * b.recoilYawModifier;
            a.recoilRollModifier = a.recoilRollModifier * b.recoilRollModifier;
            a.recoilRate = a.recoilRate * b.recoilRate;
            a.recoilYawRate = a.recoilYawRate * b.recoilYawRate;
            a.recoilRollRate = a.recoilRollRate * b.recoilRollRate;
            a.rateOfFireModifier = a.rateOfFireModifier * b.rateOfFireModifier;
            a.rateOfFireRate = a.rateOfFireRate * b.rateOfFireRate;
            a.reloadDurationModifier = a.reloadDurationModifier * b.reloadDurationModifier;
            a.reloadDurationRate = a.reloadDurationRate * b.reloadDurationRate;
            a.fireSpreadRangeModifier = a.fireSpreadRangeModifier * b.fireSpreadRangeModifier;
            a.fireSpreadRangeRate = a.fireSpreadRangeRate * b.fireSpreadRangeRate;
            a.fireSpreadModifier = a.fireSpreadModifier * b.fireSpreadModifier;
            a.fireSpreadRate = a.fireSpreadRate * b.fireSpreadRate;
            a.decreaseFoodDecreation = a.decreaseFoodDecreation * b.decreaseFoodDecreation;
            a.decreaseWaterDecreation = a.decreaseWaterDecreation * b.decreaseWaterDecreation;
            a.decreaseStaminaDecreation = a.decreaseStaminaDecreation * b.decreaseStaminaDecreation;
            a.buyItemPriceRate = a.buyItemPriceRate * b.buyItemPriceRate;
            a.sellItemPriceRate = a.sellItemPriceRate * b.sellItemPriceRate;
            if (GameExtensionInstance.onMultiplyCharacterStats != null)
                GameExtensionInstance.onMultiplyCharacterStats(ref a, b);
            return a;
        }
    }

    [System.Serializable]
    public struct CharacterStatsIncremental
    {
        [Tooltip("Amount at level 1")]
        public CharacterStats baseStats;
        [Tooltip("Increase amount when level > 1 (it will be decreasing when level < 0)")]
        public CharacterStats statsIncreaseEachLevel;
        [Tooltip("Percentage rate increase per level (0.05 = +5% per level)")]
        public CharacterStats rateIncreaseEachLevel;
        [Tooltip("It won't automatically sort by `minLevel`, you have to sort it from low to high to make it calculate properly")]
        public CharacterStatsIncrementalByLevel[] statsIncreaseEachLevelByLevels;

        public CharacterStats GetCharacterStats(int level)
        {
            if (statsIncreaseEachLevelByLevels == null || statsIncreaseEachLevelByLevels.Length == 0)
                return baseStats + (statsIncreaseEachLevel * (level - (level > 0 ? 1 : 0))) + ((baseStats + (statsIncreaseEachLevel * (level - (level > 0 ? 1 : 0)))) * rateIncreaseEachLevel);
            CharacterStats result = baseStats;
            int countLevel = 2;
            int indexOfIncremental = 0;
            int firstMinLevel = statsIncreaseEachLevelByLevels[indexOfIncremental].minLevel;
            while (countLevel <= level)
            {
                CharacterStats flat = statsIncreaseEachLevel;
                CharacterStats rate = rateIncreaseEachLevel;
                if (countLevel >= firstMinLevel)
                {
                    flat = statsIncreaseEachLevelByLevels[indexOfIncremental].statsIncreaseEachLevel;
                    rate = statsIncreaseEachLevelByLevels[indexOfIncremental].rateIncreaseEachLevel;
                }
                result += flat;
                result += result * rate;
                countLevel++;
                if (indexOfIncremental + 1 < statsIncreaseEachLevelByLevels.Length && countLevel >= statsIncreaseEachLevelByLevels[indexOfIncremental + 1].minLevel)
                    indexOfIncremental++;
            }
            return result;
        }

        public CharacterStats GetCharacterLevelStats(int level)
        {
            if (statsIncreaseEachLevelByLevels == null || statsIncreaseEachLevelByLevels.Length == 0)
                return statsIncreaseEachLevel;

            for (int i = statsIncreaseEachLevelByLevels.Length - 1; i >= 0; i--)
            {
                if (level - 1 >= statsIncreaseEachLevelByLevels[i].minLevel)
                    return statsIncreaseEachLevelByLevels[i].statsIncreaseEachLevel;
            }

            return statsIncreaseEachLevel;
        }
    }

    [System.Serializable]
    public struct CharacterStatsIncrementalByLevel
    {
        public int minLevel;
        public CharacterStats statsIncreaseEachLevel;
        public CharacterStats rateIncreaseEachLevel;
    }
}
