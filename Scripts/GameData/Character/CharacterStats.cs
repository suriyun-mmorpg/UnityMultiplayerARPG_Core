using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
#pragma warning disable CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
    public partial struct CharacterStats
#pragma warning restore CS0282 // There is no defined ordering between fields in multiple declarations of partial struct
    {
        public static readonly CharacterStats Empty = new CharacterStats();
        [Header("Default Stats")]
        public float hp;
        public float mp;
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
        public float stamina;
        public float food;
        public float water;
        // TODO: This is deprecated, will be removed later
        [Header("Deprecated")]
        public float armor;

        public CharacterStats Add(CharacterStats b)
        {
            hp = hp + b.hp;
            mp = mp + b.mp;
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
            stamina = stamina + b.stamina;
            food = food + b.food;
            water = water + b.water;
            return this.InvokeInstanceDevExtMethodsLoopItself("Add", b);
        }

        public CharacterStats Multiply(float multiplier)
        {
            hp = hp * multiplier;
            mp = mp * multiplier;
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
            stamina = stamina * multiplier;
            food = food * multiplier;
            water = water * multiplier;
            return this.InvokeInstanceDevExtMethodsLoopItself("Multiply", multiplier);
        }

        public CharacterStats MultiplyStats(CharacterStats b)
        {
            hp = hp * b.hp;
            mp = mp * b.mp;
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
            stamina = stamina * b.stamina;
            food = food * b.food;
            water = water * b.water;
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
            string mpStatsFormat,
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
            string staminaStatsFormat,
            string foodStatsFormat,
            string waterStatsFormat,
            TextWrapper uiTextHp = null,
            TextWrapper uiTextMp = null,
            TextWrapper uiTextArmor = null,
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
            TextWrapper uiTextStamina = null,
            TextWrapper uiTextFood = null,
            TextWrapper uiTextWater = null)
        {
            string statsString = string.Empty;
            string statsStringPart = string.Empty;
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
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextHp != null)
                uiTextHp.text = statsStringPart;

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
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextMp != null)
                uiTextMp.text = statsStringPart;

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
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
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
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextEvasion != null)
                uiTextEvasion.text = statsStringPart;

            // Cri Rate
            if (isBonus)
                tempValue = isRate ? (data.criRate * 10000).ToBonusString("N2") : (data.criRate * 100).ToBonusString("N2");
            else
                tempValue = isRate ? (data.criRate * 10000).ToString("N2") : (data.criRate * 100).ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(criRateStatsFormat),
                tempValue);
            if (data.criRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextCriRate != null)
                uiTextCriRate.text = statsStringPart;

            // Cri Dmg Rate
            if (isBonus)
                tempValue = isRate ? (data.criDmgRate * 10000).ToBonusString("N2") : (data.criDmgRate * 100).ToBonusString("N2");
            else
                tempValue = isRate ? (data.criDmgRate * 10000).ToString("N2") : (data.criDmgRate * 100).ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(criDmgRateStatsFormat),
                tempValue);
            if (data.criDmgRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextCriDmgRate != null)
                uiTextCriDmgRate.text = statsStringPart;

            // Block Rate
            if (isBonus)
                tempValue = isRate ? (data.blockRate * 10000).ToBonusString("N2") : (data.blockRate * 100).ToBonusString("N2");
            else
                tempValue = isRate ? (data.blockRate * 10000).ToString("N2") : (data.blockRate * 100).ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(blockRateStatsFormat),
                tempValue);
            if (data.blockRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextBlockRate != null)
                uiTextBlockRate.text = statsStringPart;

            // Block Dmg Rate
            if (isBonus)
                tempValue = isRate ? (data.blockDmgRate * 10000).ToBonusString("N2") : (data.blockDmgRate * 100).ToBonusString("N2");
            else
                tempValue = isRate ? (data.blockDmgRate * 10000).ToString("N2") : (data.blockDmgRate * 100).ToString("N2");
            statsStringPart = string.Format(
                LanguageManager.GetText(blockDmgRateStatsFormat),
                tempValue);
            if (data.blockDmgRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextBlockDmgRate != null)
                uiTextBlockDmgRate.text = statsStringPart;

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
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
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
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextSlotLimit != null)
                uiTextSlotLimit.text = statsStringPart;

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
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
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
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextAtkSpeed != null)
                uiTextAtkSpeed.text = statsStringPart;

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
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextStamina != null)
                uiTextStamina.text = statsStringPart;

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
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
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
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextWater != null)
                uiTextWater.text = statsStringPart;

            return statsString;
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
