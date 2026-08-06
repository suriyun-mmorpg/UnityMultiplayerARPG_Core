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
        [CharacterStatTextGen(nameof(hp), false)]
        public float hp;
        [CharacterStatTextGen(nameof(hpRecovery), false)]
        public float hpRecovery;
        [CharacterStatTextGen(nameof(hpLeechRate), true)]
        public float hpLeechRate;
        [CharacterStatTextGen(nameof(mp), false)]
        public float mp;
        [CharacterStatTextGen(nameof(mpRecovery), false)]
        public float mpRecovery;
        [CharacterStatTextGen(nameof(mpLeechRate), true)]
        public float mpLeechRate;
        [CharacterStatTextGen(nameof(stamina), false)]
        public float stamina;
        [CharacterStatTextGen(nameof(staminaRecovery), false)]
        public float staminaRecovery;
        [CharacterStatTextGen(nameof(staminaLeechRate), true)]
        public float staminaLeechRate;
        [CharacterStatTextGen(nameof(food), false)]
        public float food;
        [CharacterStatTextGen(nameof(water), false)]
        public float water;
        [CharacterStatTextGen(nameof(accuracy), false)]
        public float accuracy;
        [CharacterStatTextGen(nameof(evasion), false)]
        public float evasion;
        [CharacterStatTextGen(nameof(criRate), true)]
        public float criRate;
        [CharacterStatTextGen(nameof(criDmgRate), true)]
        public float criDmgRate;
        [CharacterStatTextGen(nameof(blockRate), true)]
        public float blockRate;
        [CharacterStatTextGen(nameof(blockDmgRate), true)]
        public float blockDmgRate;
        [CharacterStatTextGen(nameof(moveSpeed), false)]
        public float moveSpeed;
        [CharacterStatTextGen(nameof(sprintSpeed), false)]
        public float sprintSpeed;
        [CharacterStatTextGen(nameof(atkSpeed), false)]
        public float atkSpeed;
        [CharacterStatTextGen(nameof(weightLimit), false)]
        public float weightLimit;
        [CharacterStatTextGen(nameof(slotLimit), false)]
        public float slotLimit;
        [CharacterStatTextGen(nameof(goldRate), true)]
        public float goldRate;
        [CharacterStatTextGen(nameof(expRate), true)]
        public float expRate;
        [CharacterStatTextGen(nameof(itemDropRate), true)]
        public float itemDropRate;
        [CharacterStatTextGen(nameof(jumpHeight), false)]
        public float jumpHeight;
        [CharacterStatTextGen(nameof(headDamageAbsorbs), true)]
        public float headDamageAbsorbs;
        [CharacterStatTextGen(nameof(bodyDamageAbsorbs), true)]
        public float bodyDamageAbsorbs;
        [CharacterStatTextGen(nameof(fallDamageAbsorbs), true)]
        public float fallDamageAbsorbs;
        [CharacterStatTextGen(nameof(gravityRate), true)]
        public float gravityRate;
        [CharacterStatTextGen(nameof(protectedSlotLimit), false)]
        public float protectedSlotLimit;
        [FormerlySerializedAs("ammoCapacity")]
        [CharacterStatTextGen(nameof(ammoCapacityModifier), false)]
        public float ammoCapacityModifier;
        [CharacterStatTextGen(nameof(ammoCapacityRate), true)]
        public float ammoCapacityRate;
        [CharacterStatTextGen(nameof(recoilModifier), false)]
        public float recoilModifier;
        [CharacterStatTextGen(nameof(recoilYawModifier), false)]
        public float recoilYawModifier;
        [CharacterStatTextGen(nameof(recoilRollModifier), false)]
        public float recoilRollModifier;
        [CharacterStatTextGen(nameof(recoilRate), true)]
        public float recoilRate;
        [CharacterStatTextGen(nameof(recoilYawRate), true)]
        public float recoilYawRate;
        [CharacterStatTextGen(nameof(recoilRollRate), true)]
        public float recoilRollRate;
        [CharacterStatTextGen(nameof(rateOfFireModifier), false)]
        public float rateOfFireModifier;
        [CharacterStatTextGen(nameof(rateOfFireRate), true)]
        public float rateOfFireRate;
        [FormerlySerializedAs("reloadDuration")]
        [CharacterStatTextGen(nameof(reloadDurationModifier), false)]
        public float reloadDurationModifier;
        [CharacterStatTextGen(nameof(reloadDurationRate), true)]
        public float reloadDurationRate;
        [FormerlySerializedAs("fireSpreadRange")]
        [CharacterStatTextGen(nameof(fireSpreadRangeModifier), false)]
        public float fireSpreadRangeModifier;
        [CharacterStatTextGen(nameof(fireSpreadRangeRate), true)]
        public float fireSpreadRangeRate;
        [FormerlySerializedAs("fireSpread")]
        [CharacterStatTextGen(nameof(fireSpreadModifier), false)]
        public float fireSpreadModifier;
        [CharacterStatTextGen(nameof(fireSpreadRate), true)]
        public float fireSpreadRate;
        [CharacterStatTextGen(nameof(decreaseFoodDecreation), false)]
        public float decreaseFoodDecreation;
        [CharacterStatTextGen(nameof(decreaseWaterDecreation), false)]
        public float decreaseWaterDecreation;
        [CharacterStatTextGen(nameof(decreaseStaminaDecreation), false)]
        public float decreaseStaminaDecreation;
        [CharacterStatTextGen(nameof(buyItemPriceRate), true)]
        public float buyItemPriceRate;
        [CharacterStatTextGen(nameof(sellItemPriceRate), true)]
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
