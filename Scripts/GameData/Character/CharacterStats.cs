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

        public static string GetText(
            CharacterStats data,
            bool isRate,
            bool isBonus,
            string hpStatsFormat,
            string hpRecoveryStatsFormat,
            string hpLeechRateStatsFormat,
            string mpStatsFormat,
            string mpRecoveryStatsFormat,
            string mpLeechRateStatsFormat,
            string staminaStatsFormat,
            string staminaRecoveryStatsFormat,
            string staminaLeechRateStatsFormat,
            string foodStatsFormat,
            string waterStatsFormat,
            string accuracyStatsFormat,
            string evasionStatsFormat,
            string criRateStatsFormat,
            string criDmgRateStatsFormat,
            string blockRateStatsFormat,
            string blockDmgRateStatsFormat,
            string moveSpeedStatsFormat,
            string atkSpeedStatsFormat,
            string weightLimitStatsFormat,
            string slotLimitStatsFormat,
            string goldRateStatsFormat,
            string expRateStatsFormat,
            TextWrapper uiTextHp = null,
            TextWrapper uiTextHpRecovery = null,
            TextWrapper uiTextHpLeechRate = null,
            TextWrapper uiTextMp = null,
            TextWrapper uiTextMpRecovery = null,
            TextWrapper uiTextMpLeechRate = null,
            TextWrapper uiTextStamina = null,
            TextWrapper uiTextStaminaRecovery = null,
            TextWrapper uiTextStaminaLeechRate = null,
            TextWrapper uiTextFood = null,
            TextWrapper uiTextWater = null,
            TextWrapper uiTextAccuracy = null,
            TextWrapper uiTextEvasion = null,
            TextWrapper uiTextCriRate = null,
            TextWrapper uiTextCriDmgRate = null,
            TextWrapper uiTextBlockRate = null,
            TextWrapper uiTextBlockDmgRate = null,
            TextWrapper uiTextMoveSpeed = null,
            TextWrapper uiTextAtkSpeed = null,
            TextWrapper uiTextWeightLimit = null,
            TextWrapper uiTextSlotLimit = null,
            TextWrapper uiTextGoldRate = null,
            TextWrapper uiTextExpRate = null)
        {
            StringBuilder statsString = new StringBuilder();
            string statsStringPart;
            string tempValue;

            // Hp
            if (isBonus)
                tempValue = isRate ? (data.hp * 100).ToBonusString("N2") : data.hp.ToBonusString("N0");
            else
                tempValue = isRate ? (data.hp * 100).ToString("N2") : data.hp.ToString("N0");
            statsStringPart = string.Format(
                LanguageManager.GetText(hpStatsFormat),
                tempValue);
            if (data.hp != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextHp != null)
                uiTextHp.text = statsStringPart;

            // Hp Recovery
            if (isBonus)
                tempValue = isRate ? (data.hpRecovery * 100).ToBonusString("N2") : data.hpRecovery.ToBonusString("N0");
            else
                tempValue = isRate ? (data.hpRecovery * 100).ToString("N2") : data.hpRecovery.ToString("N0");
            statsStringPart = string.Format(
                LanguageManager.GetText(hpRecoveryStatsFormat),
                tempValue);
            if (data.hpRecovery != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextHpRecovery != null)
                uiTextHpRecovery.text = statsStringPart;

            // Hp Leech Rate
            if (isBonus)
                tempValue = isRate ? (data.hpLeechRate * 100).ToBonusString("N2") : (data.hpLeechRate * 100).ToBonusString("N2");
            else
                tempValue = isRate ? (data.hpLeechRate * 100).ToString("N2") : (data.hpLeechRate * 100).ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(hpLeechRateStatsFormat),
                tempValue);
            if (data.hpLeechRate != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextHpLeechRate != null)
                uiTextHpLeechRate.text = statsStringPart;

            // Mp
            if (isBonus)
                tempValue = isRate ? (data.mp * 100).ToBonusString("N2") : data.mp.ToBonusString("N0");
            else
                tempValue = isRate ? (data.mp * 100).ToString("N2") : data.mp.ToString("N0");
            statsStringPart = string.Format(
                LanguageManager.GetText(mpStatsFormat),
                tempValue);
            if (data.mp != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextMp != null)
                uiTextMp.text = statsStringPart;

            // Mp Recovery
            if (isBonus)
                tempValue = isRate ? (data.mpRecovery * 100).ToBonusString("N2") : data.mpRecovery.ToBonusString("N0");
            else
                tempValue = isRate ? (data.mpRecovery * 100).ToString("N2") : data.mpRecovery.ToString("N0");
            statsStringPart = string.Format(
                LanguageManager.GetText(mpRecoveryStatsFormat),
                tempValue);
            if (data.mpRecovery != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextMpRecovery != null)
                uiTextMpRecovery.text = statsStringPart;

            // Mp Leech Rate
            if (isBonus)
                tempValue = isRate ? (data.mpLeechRate * 100).ToBonusString("N2") : (data.mpLeechRate * 100).ToBonusString("N2");
            else
                tempValue = isRate ? (data.mpLeechRate * 100).ToString("N2") : (data.mpLeechRate * 100).ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(mpLeechRateStatsFormat),
                tempValue);
            if (data.mpLeechRate != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextMpLeechRate != null)
                uiTextMpLeechRate.text = statsStringPart;

            // Stamina
            if (isBonus)
                tempValue = isRate ? (data.stamina * 100).ToBonusString("N2") : data.stamina.ToBonusString("N0");
            else
                tempValue = isRate ? (data.stamina * 100).ToString("N2") : data.stamina.ToString("N0");
            statsStringPart = string.Format(
                LanguageManager.GetText(staminaStatsFormat),
                tempValue);
            if (data.stamina != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextStamina != null)
                uiTextStamina.text = statsStringPart;

            // Stamina Recovery
            if (isBonus)
                tempValue = isRate ? (data.staminaRecovery * 100).ToBonusString("N2") : data.staminaRecovery.ToBonusString("N0");
            else
                tempValue = isRate ? (data.staminaRecovery * 100).ToString("N2") : data.staminaRecovery.ToString("N0");
            statsStringPart = string.Format(
                LanguageManager.GetText(staminaRecoveryStatsFormat),
                tempValue);
            if (data.staminaRecovery != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextStaminaRecovery != null)
                uiTextStaminaRecovery.text = statsStringPart;

            // Stamina Leech Rate
            if (isBonus)
                tempValue = isRate ? (data.staminaLeechRate * 100).ToBonusString("N2") : (data.staminaLeechRate * 100).ToBonusString("N2");
            else
                tempValue = isRate ? (data.staminaLeechRate * 100).ToString("N2") : (data.staminaLeechRate * 100).ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(staminaLeechRateStatsFormat),
                tempValue);
            if (data.staminaLeechRate != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextStaminaLeechRate != null)
                uiTextStaminaLeechRate.text = statsStringPart;

            // Food
            if (isBonus)
                tempValue = isRate ? (data.food * 100).ToBonusString("N2") : data.food.ToBonusString("N0");
            else
                tempValue = isRate ? (data.food * 100).ToString("N2") : data.food.ToString("N0");
            statsStringPart = string.Format(
                LanguageManager.GetText(foodStatsFormat),
                tempValue);
            if (data.food != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextFood != null)
                uiTextFood.text = statsStringPart;

            // Water
            if (isBonus)
                tempValue = isRate ? (data.water * 100).ToBonusString("N2") : data.water.ToBonusString("N0");
            else
                tempValue = isRate ? (data.water * 100).ToString("N2") : data.water.ToString("N0");
            statsStringPart = string.Format(
                LanguageManager.GetText(waterStatsFormat),
                tempValue);
            if (data.water != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextWater != null)
                uiTextWater.text = statsStringPart;

            // Accuracy
            if (isBonus)
                tempValue = isRate ? (data.accuracy * 100).ToBonusString("N2") : data.accuracy.ToBonusString("N0");
            else
                tempValue = isRate ? (data.accuracy * 100).ToString("N2") : data.accuracy.ToString("N0");
            statsStringPart = string.Format(
                LanguageManager.GetText(accuracyStatsFormat),
                tempValue);
            if (data.accuracy != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextAccuracy != null)
                uiTextAccuracy.text = statsStringPart;

            // Evasion
            if (isBonus)
                tempValue = isRate ? (data.evasion * 100).ToBonusString("N2") : data.evasion.ToBonusString("N0");
            else
                tempValue = isRate ? (data.evasion * 100).ToString("N2") : data.evasion.ToString("N0");
            statsStringPart = string.Format(
                LanguageManager.GetText(evasionStatsFormat),
                tempValue);
            if (data.evasion != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextEvasion != null)
                uiTextEvasion.text = statsStringPart;

            // Cri Rate
            if (isBonus)
                tempValue = isRate ? (data.criRate * 100).ToBonusString("N2") : (data.criRate * 100).ToBonusString("N2");
            else
                tempValue = isRate ? (data.criRate * 100).ToString("N2") : (data.criRate * 100).ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(criRateStatsFormat),
                tempValue);
            if (data.criRate != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextCriRate != null)
                uiTextCriRate.text = statsStringPart;

            // Cri Dmg Rate
            if (isBonus)
                tempValue = isRate ? (data.criDmgRate * 100).ToBonusString("N2") : (data.criDmgRate * 100).ToBonusString("N2");
            else
                tempValue = isRate ? (data.criDmgRate * 100).ToString("N2") : (data.criDmgRate * 100).ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(criDmgRateStatsFormat),
                tempValue);
            if (data.criDmgRate != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextCriDmgRate != null)
                uiTextCriDmgRate.text = statsStringPart;

            // Block Rate
            if (isBonus)
                tempValue = isRate ? (data.blockRate * 100).ToBonusString("N2") : (data.blockRate * 100).ToBonusString("N2");
            else
                tempValue = isRate ? (data.blockRate * 100).ToString("N2") : (data.blockRate * 100).ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(blockRateStatsFormat),
                tempValue);
            if (data.blockRate != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextBlockRate != null)
                uiTextBlockRate.text = statsStringPart;

            // Block Dmg Rate
            if (isBonus)
                tempValue = isRate ? (data.blockDmgRate * 100).ToBonusString("N2") : (data.blockDmgRate * 100).ToBonusString("N2");
            else
                tempValue = isRate ? (data.blockDmgRate * 100).ToString("N2") : (data.blockDmgRate * 100).ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(blockDmgRateStatsFormat),
                tempValue);
            if (data.blockDmgRate != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextBlockDmgRate != null)
                uiTextBlockDmgRate.text = statsStringPart;

            // Move Speed
            if (isBonus)
                tempValue = isRate ? (data.moveSpeed * 100).ToBonusString("N2") : data.moveSpeed.ToBonusString("N2");
            else
                tempValue = isRate ? (data.moveSpeed * 100).ToString("N2") : data.moveSpeed.ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(moveSpeedStatsFormat),
                tempValue);
            if (data.moveSpeed != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextMoveSpeed != null)
                uiTextMoveSpeed.text = statsStringPart;

            // Attack Speed
            if (isBonus)
                tempValue = isRate ? (data.atkSpeed * 100).ToBonusString("N2") : data.atkSpeed.ToBonusString("N2");
            else
                tempValue = isRate ? (data.atkSpeed * 100).ToString("N2") : data.atkSpeed.ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(atkSpeedStatsFormat),
                tempValue);
            if (data.atkSpeed != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextAtkSpeed != null)
                uiTextAtkSpeed.text = statsStringPart;

            // Weight
            if (isBonus)
                tempValue = isRate ? (data.weightLimit * 100).ToBonusString("N2") : data.weightLimit.ToBonusString("N2");
            else
                tempValue = isRate ? (data.weightLimit * 100).ToString("N2") : data.weightLimit.ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(weightLimitStatsFormat),
                tempValue);
            if (data.weightLimit != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextWeightLimit != null)
                uiTextWeightLimit.text = statsStringPart;

            // Slot
            if (isBonus)
                tempValue = isRate ? (data.slotLimit * 100).ToBonusString("N2") : data.slotLimit.ToBonusString("N0");
            else
                tempValue = isRate ? (data.slotLimit * 100).ToString("N2") : data.slotLimit.ToString("N0");
            statsStringPart = string.Format(
                LanguageManager.GetText(slotLimitStatsFormat),
                tempValue);
            if (data.slotLimit != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextSlotLimit != null)
                uiTextSlotLimit.text = statsStringPart;

            // Gold Rate
            if (isBonus)
                tempValue = isRate ? (data.goldRate * 100).ToBonusString("N2") : (data.goldRate * 100).ToBonusString("N0");
            else
                tempValue = isRate ? (data.goldRate * 100).ToString("N2") : (data.goldRate * 100).ToString("N0");
            statsStringPart = string.Format(
                LanguageManager.GetText(goldRateStatsFormat),
                tempValue);
            if (data.goldRate != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextGoldRate != null)
                uiTextGoldRate.text = statsStringPart;

            // Exp Rate
            if (isBonus)
                tempValue = isRate ? (data.expRate * 100).ToBonusString("N2") : (data.expRate * 100).ToBonusString("N0");
            else
                tempValue = isRate ? (data.expRate * 100).ToString("N2") : (data.expRate * 100).ToString("N0");
            statsStringPart = string.Format(
                LanguageManager.GetText(expRateStatsFormat),
                tempValue);
            if (data.expRate != 0)
            {
                if (statsString.Length > 0)
                    statsString.Append('\n');
                statsString.Append(statsStringPart);
            }
            if (uiTextExpRate != null)
                uiTextExpRate.text = statsStringPart;

            return statsString.ToString();
        }
    }

#pragma warning disable CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
    public partial struct RandomCharacterStats
#pragma warning restore CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
    {
        [Header("HP")]
        public float minHp;
        public float maxHp;
        [Range(0, 1f)]
        public float hpApplyRate;

        [Header("HP Recovery")]
        public float minHpRecovery;
        public float maxHpRecovery;
        [Range(0, 1f)]
        public float hpRecoveryApplyRate;

        [Header("HP Leech Rate")]
        public float minHpLeechRate;
        public float maxHpLeechRate;
        [Range(0, 1f)]
        public float hpLeechRateApplyRate;

        [Header("MP")]
        public float minMp;
        public float maxMp;
        [Range(0, 1f)]
        public float mpApplyRate;

        [Header("MP Recovery")]
        public float minMpRecovery;
        public float maxMpRecovery;
        [Range(0, 1f)]
        public float mpRecoveryApplyRate;

        [Header("MP Leech Rate")]
        public float minMpLeechRate;
        public float maxMpLeechRate;
        [Range(0, 1f)]
        public float mpLeechRateApplyRate;

        [Header("Stamina")]
        public float minStamina;
        public float maxStamina;
        [Range(0, 1f)]
        public float staminaApplyRate;

        [Header("Stamina Recovery")]
        public float minStaminaRecovery;
        public float maxStaminaRecovery;
        [Range(0, 1f)]
        public float staminaRecoveryApplyRate;

        [Header("Stamina Leech Rate")]
        public float minStaminaLeechRate;
        public float maxStaminaLeechRate;
        [Range(0, 1f)]
        public float staminaLeechRateApplyRate;

        [Header("Food")]
        public float minFood;
        public float maxFood;
        [Range(0, 1f)]
        public float foodApplyRate;

        [Header("Water")]
        public float minWater;
        public float maxWater;
        [Range(0, 1f)]
        public float waterApplyRate;

        [Header("Accuracy")]
        public float minAccuracy;
        public float maxAccuracy;
        [Range(0, 1f)]
        public float accuracyApplyRate;

        [Header("Evasion")]
        public float minEvasion;
        public float maxEvasion;
        [Range(0, 1f)]
        public float evasionApplyRate;

        [Header("Cri Rate")]
        public float minCriRate;
        public float maxCriRate;
        [Range(0, 1f)]
        public float criRateApplyRate;

        [Header("Cri Dmg Rate")]
        public float minCriDmgRate;
        public float maxCriDmgRate;
        [Range(0, 1f)]
        public float criDmgRateApplyRate;

        [Header("Block Rate")]
        public float minBlockRate;
        public float maxBlockRate;
        [Range(0, 1f)]
        public float blockRateApplyRate;

        [Header("Block Dmg Rate")]
        public float minBlockDmgRate;
        public float maxBlockDmgRate;
        [Range(0, 1f)]
        public float blockDmgRateApplyRate;

        [Header("Move Speed")]
        public float minMoveSpeed;
        public float maxMoveSpeed;
        [Range(0, 1f)]
        public float moveSpeedApplyRate;

        [Header("Atk Speed")]
        public float minAtkSpeed;
        public float maxAtkSpeed;
        [Range(0, 1f)]
        public float atkSpeedApplyRate;

        [Header("Weight Limit")]
        public float minWeightLimit;
        public float maxWeightLimit;
        [Range(0, 1f)]
        public float weightLimitApplyRate;

        [Header("Slot Limit")]
        public float minSlotLimit;
        public float maxSlotLimit;
        [Range(0, 1f)]
        public float slotLimitApplyRate;

        [Header("Gold Rate")]
        public float minGoldRate;
        public float maxGoldRate;
        [Range(0, 1f)]
        public float goldRateApplyRate;

        [Header("Exp Rate")]
        public float minExpRate;
        public float maxExpRate;
        [Range(0, 1f)]
        public float expRateApplyRate;

        public float GetRandomedHp(System.Random random)
        {
            return random.RandomFloat(minHp, maxHp);
        }

        public float GetRandomedHpRecovery(System.Random random)
        {
            return random.RandomFloat(minHpRecovery, maxHpRecovery);
        }

        public float GetRandomedHpLeechRate(System.Random random)
        {
            return random.RandomFloat(minHpLeechRate, maxHpLeechRate);
        }

        public float GetRandomedMp(System.Random random)
        {
            return random.RandomFloat(minMp, maxMp);
        }

        public float GetRandomedMpRecovery(System.Random random)
        {
            return random.RandomFloat(minMpRecovery, maxMpRecovery);
        }

        public float GetRandomedMpLeechRate(System.Random random)
        {
            return random.RandomFloat(minMpLeechRate, maxMpLeechRate);
        }

        public float GetRandomedStamina(System.Random random)
        {
            return random.RandomFloat(minStamina, maxStamina);
        }

        public float GetRandomedStaminaRecovery(System.Random random)
        {
            return random.RandomFloat(minStaminaRecovery, maxStaminaRecovery);
        }

        public float GetRandomedStaminaLeechRate(System.Random random)
        {
            return random.RandomFloat(minStaminaLeechRate, maxStaminaLeechRate);
        }

        public float GetRandomedFood(System.Random random)
        {
            return random.RandomFloat(minFood, maxFood);
        }

        public float GetRandomedWater(System.Random random)
        {
            return random.RandomFloat(minWater, maxWater);
        }

        public float GetRandomedAccuracy(System.Random random)
        {
            return random.RandomFloat(minAccuracy, maxAccuracy);
        }

        public float GetRandomedEvasion(System.Random random)
        {
            return random.RandomFloat(minEvasion, maxEvasion);
        }

        public float GetRandomedCriRate(System.Random random)
        {
            return random.RandomFloat(minCriRate, maxCriRate);
        }

        public float GetRandomedCriDmgRate(System.Random random)
        {
            return random.RandomFloat(minCriDmgRate, maxCriDmgRate);
        }

        public float GetRandomedBlockRate(System.Random random)
        {
            return random.RandomFloat(minBlockRate, maxBlockRate);
        }

        public float GetRandomedBlockDmgRate(System.Random random)
        {
            return random.RandomFloat(minBlockDmgRate, maxBlockDmgRate);
        }

        public float GetRandomedMoveSpeed(System.Random random)
        {
            return random.RandomFloat(minMoveSpeed, maxMoveSpeed);
        }

        public float GetRandomedAtkSpeed(System.Random random)
        {
            return random.RandomFloat(minAtkSpeed, maxAtkSpeed);
        }

        public float GetRandomedWeightLimit(System.Random random)
        {
            return random.RandomFloat(minWeightLimit, maxWeightLimit);
        }

        public float GetRandomedSlotLimit(System.Random random)
        {
            return random.RandomFloat(minSlotLimit, maxSlotLimit);
        }

        public float GetRandomedGoldRate(System.Random random)
        {
            return random.RandomFloat(minGoldRate, maxGoldRate);
        }

        public float GetRandomedExpRate(System.Random random)
        {
            return random.RandomFloat(minExpRate, maxExpRate);
        }

        public bool ApplyHp(System.Random random)
        {
            return random.NextDouble() <= hpApplyRate;
        }

        public bool ApplyHpRecovery(System.Random random)
        {
            return random.NextDouble() <= hpRecoveryApplyRate;
        }

        public bool ApplyHpLeechRate(System.Random random)
        {
            return random.NextDouble() <= hpLeechRateApplyRate;
        }

        public bool ApplyMp(System.Random random)
        {
            return random.NextDouble() <= mpApplyRate;
        }

        public bool ApplyMpRecovery(System.Random random)
        {
            return random.NextDouble() <= mpRecoveryApplyRate;
        }

        public bool ApplyMpLeechRate(System.Random random)
        {
            return random.NextDouble() <= mpLeechRateApplyRate;
        }

        public bool ApplyStamina(System.Random random)
        {
            return random.NextDouble() <= staminaApplyRate;
        }

        public bool ApplyStaminaRecovery(System.Random random)
        {
            return random.NextDouble() <= staminaRecoveryApplyRate;
        }

        public bool ApplyStaminaLeechRate(System.Random random)
        {
            return random.NextDouble() <= staminaLeechRateApplyRate;
        }

        public bool ApplyFood(System.Random random)
        {
            return random.NextDouble() <= foodApplyRate;
        }

        public bool ApplyWater(System.Random random)
        {
            return random.NextDouble() <= waterApplyRate;
        }

        public bool ApplyAccuracy(System.Random random)
        {
            return random.NextDouble() <= accuracyApplyRate;
        }

        public bool ApplyEvasion(System.Random random)
        {
            return random.NextDouble() <= evasionApplyRate;
        }

        public bool ApplyCriRate(System.Random random)
        {
            return random.NextDouble() <= criRateApplyRate;
        }

        public bool ApplyCriDmgRate(System.Random random)
        {
            return random.NextDouble() <= criDmgRateApplyRate;
        }

        public bool ApplyBlockRate(System.Random random)
        {
            return random.NextDouble() <= blockRateApplyRate;
        }

        public bool ApplyBlockDmgRate(System.Random random)
        {
            return random.NextDouble() <= blockDmgRateApplyRate;
        }

        public bool ApplyMoveSpeed(System.Random random)
        {
            return random.NextDouble() <= moveSpeedApplyRate;
        }

        public bool ApplyAtkSpeed(System.Random random)
        {
            return random.NextDouble() <= atkSpeedApplyRate;
        }

        public bool ApplyWeightLimit(System.Random random)
        {
            return random.NextDouble() <= weightLimitApplyRate;
        }

        public bool ApplySlotLimit(System.Random random)
        {
            return random.NextDouble() <= slotLimitApplyRate;
        }

        public bool ApplyGoldRate(System.Random random)
        {
            return random.NextDouble() <= goldRateApplyRate;
        }

        public bool ApplyExpRate(System.Random random)
        {
            return random.NextDouble() <= expRateApplyRate;
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
