using System.Runtime.InteropServices;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    [StructLayout(LayoutKind.Auto)]
    public partial struct CharacterStats
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
        public float sprintSpeed;
        public float atkSpeed;
        public float weightLimit;
        public float slotLimit;
        public float goldRate;
        public float expRate;
        public float itemDropRate;
        public float jumpHeight;
        public float headDamageAbsorbs;
        public float bodyDamageAbsorbs;
        public float fallDamageAbsorbs;
        public float gravityRate;
        public float protectedSlotLimit;
        public float ammoCapacity;
        public float recoilModifier;
        public float recoilRate;
        public float rateOfFire;
        public float reloadDuration;
        public float fireSpreadRangeRate;
        public float fireSpread;
        public float decreaseFoodDecreation;
        public float decreaseWaterDecreation;
        public float decreaseStaminaDecreation;
        public float buyItemPriceRate;
        public float sellItemPriceRate;

        public CharacterStats()
        {
            Reset();
        }

        public void Reset()
        {
            hp = 0f;
            hpRecovery = 0f;
            hpLeechRate = 0f;
            mp = 0f;
            mpRecovery = 0f;
            mpLeechRate = 0f;
            stamina = 0f;
            staminaRecovery = 0f;
            staminaLeechRate = 0f;
            food = 0f;
            water = 0f;
            accuracy = 0f;
            evasion = 0f;
            criRate = 0f;
            criDmgRate = 0f;
            blockRate = 0f;
            blockDmgRate = 0f;
            moveSpeed = 0f;
            sprintSpeed = 0f;
            atkSpeed = 0f;
            weightLimit = 0f;
            slotLimit = 0f;
            goldRate = 0f;
            expRate = 0f;
            itemDropRate = 0f;
            jumpHeight = 0f;
            headDamageAbsorbs = 0f;
            bodyDamageAbsorbs = 0f;
            fallDamageAbsorbs = 0f;
            gravityRate = 0f;
            protectedSlotLimit = 0f;
            ammoCapacity = 0f;
            recoilModifier = 0f;
            recoilRate = 0f;
            rateOfFire = 0f;
            reloadDuration = 0f;
            fireSpreadRangeRate = 0f;
            fireSpread = 0f;
            decreaseFoodDecreation = 0f;
            decreaseWaterDecreation = 0f;
            decreaseStaminaDecreation = 0f;
            buyItemPriceRate = 0f;
            sellItemPriceRate = 0f;
        }

        public CharacterStats(CharacterStats other)
        {
            hp = other.hp;
            hpRecovery = other.hpRecovery;
            hpLeechRate = other.hpLeechRate;
            mp = other.mp;
            mpRecovery = other.mpRecovery;
            mpLeechRate = other.mpLeechRate;
            stamina = other.stamina;
            staminaRecovery = other.staminaRecovery;
            staminaLeechRate = other.staminaLeechRate;
            food = other.food;
            water = other.water;
            accuracy = other.accuracy;
            evasion = other.evasion;
            criRate = other.criRate;
            criDmgRate = other.criDmgRate;
            blockRate = other.blockRate;
            blockDmgRate = other.blockDmgRate;
            moveSpeed = other.moveSpeed;
            sprintSpeed = other.sprintSpeed;
            atkSpeed = other.atkSpeed;
            weightLimit = other.weightLimit;
            slotLimit = other.slotLimit;
            goldRate = other.goldRate;
            expRate = other.expRate;
            itemDropRate = other.itemDropRate;
            jumpHeight = other.jumpHeight;
            headDamageAbsorbs = other.headDamageAbsorbs;
            bodyDamageAbsorbs = other.bodyDamageAbsorbs;
            fallDamageAbsorbs = other.fallDamageAbsorbs;
            gravityRate = other.gravityRate;
            protectedSlotLimit = other.protectedSlotLimit;
            ammoCapacity = other.ammoCapacity;
            recoilModifier = other.recoilModifier;
            recoilRate = other.recoilRate;
            rateOfFire = other.rateOfFire;
            reloadDuration = other.reloadDuration;
            fireSpreadRangeRate = other.fireSpreadRangeRate;
            fireSpread = other.fireSpread;
            decreaseFoodDecreation = other.decreaseFoodDecreation;
            decreaseWaterDecreation = other.decreaseWaterDecreation;
            decreaseStaminaDecreation = other.decreaseStaminaDecreation;
            buyItemPriceRate = other.buyItemPriceRate;
            sellItemPriceRate = other.sellItemPriceRate;
        }

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
            a.ammoCapacity = a.ammoCapacity + b.ammoCapacity;
            a.recoilModifier = a.recoilModifier + b.recoilModifier;
            a.recoilRate = a.recoilRate + b.recoilRate;
            a.rateOfFire = a.rateOfFire + b.rateOfFire;
            a.reloadDuration = a.reloadDuration + b.reloadDuration;
            a.fireSpreadRangeRate = a.fireSpreadRangeRate + b.fireSpreadRangeRate;
            a.fireSpread = a.fireSpread + b.fireSpread;
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
            a.ammoCapacity = a.ammoCapacity - b.ammoCapacity;
            a.recoilModifier = a.recoilModifier - b.recoilModifier;
            a.recoilRate = a.recoilRate - b.recoilRate;
            a.rateOfFire = a.rateOfFire - b.rateOfFire;
            a.reloadDuration = a.reloadDuration - b.reloadDuration;
            a.fireSpreadRangeRate = a.fireSpreadRangeRate - b.fireSpreadRangeRate;
            a.fireSpread = a.fireSpread - b.fireSpread;
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
            a.ammoCapacity = a.ammoCapacity * multiplier;
            a.recoilModifier = a.recoilModifier * multiplier;
            a.recoilRate = a.recoilRate * multiplier;
            a.rateOfFire = a.rateOfFire * multiplier;
            a.reloadDuration = a.reloadDuration * multiplier;
            a.fireSpreadRangeRate = a.fireSpreadRangeRate * multiplier;
            a.fireSpread = a.fireSpread * multiplier;
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
            a.ammoCapacity = a.ammoCapacity * b.ammoCapacity;
            a.recoilModifier = a.recoilModifier * b.recoilModifier;
            a.recoilRate = a.recoilRate * b.recoilRate;
            a.rateOfFire = a.rateOfFire * b.rateOfFire;
            a.reloadDuration = a.reloadDuration * b.reloadDuration;
            a.fireSpreadRangeRate = a.fireSpreadRangeRate * b.fireSpreadRangeRate;
            a.fireSpread = a.fireSpread * b.fireSpread;
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
        [Tooltip("It won't automatically sort by `minLevel`, you have to sort it from low to high to make it calculate properly")]
        public CharacterStatsIncrementalByLevel[] statsIncreaseEachLevelByLevels;

        public CharacterStats GetCharacterStats(int level)
        {
            if (statsIncreaseEachLevelByLevels == null || statsIncreaseEachLevelByLevels.Length == 0)
                return baseStats + (statsIncreaseEachLevel * (level - (level > 0 ? 1 : 0)));
            CharacterStats result = new CharacterStats(baseStats);
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
