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
            criRate = criRate + b.criRate;
            criDmgRate = criDmgRate + b.criDmgRate;
            blockRate = blockRate + b.blockRate;
            blockDmgRate = blockDmgRate + b.blockDmgRate;
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

            // Hp
            statsStringPart = string.Format(
                LanguageManager.GetText(hpStatsFormat),
                data.hp.ToString("N0"));
            if (data.hp != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextHp != null)
                uiTextHp.text = statsStringPart;

            // Mp
            statsStringPart = string.Format(
                LanguageManager.GetText(mpStatsFormat),
                data.mp.ToString("N0"));
            if (data.mp != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextMp != null)
                uiTextMp.text = statsStringPart;

            // Accuracy
            statsStringPart = string.Format(
                LanguageManager.GetText(accuracyStatsFormat),
                data.accuracy.ToString("N0"));
            if (data.accuracy != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextAccuracy != null)
                uiTextAccuracy.text = statsStringPart;

            // Evasion
            statsStringPart = string.Format(
                LanguageManager.GetText(evasionStatsFormat),
                data.evasion.ToString("N0"));
            if (data.evasion != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextEvasion != null)
                uiTextEvasion.text = statsStringPart;

            // Cri Rate
            statsStringPart = string.Format(
                LanguageManager.GetText(criRateStatsFormat),
                (data.criRate * 100).ToString("N2"));
            if (data.criRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextCriRate != null)
                uiTextCriRate.text = statsStringPart;

            // Cri Dmg Rate
            statsStringPart = string.Format(
                LanguageManager.GetText(criDmgRateStatsFormat),
                (data.criDmgRate * 100).ToString("N2"));
            if (data.criDmgRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextCriDmgRate != null)
                uiTextCriDmgRate.text = statsStringPart;

            // Block Rate
            statsStringPart = string.Format(
                LanguageManager.GetText(blockRateStatsFormat),
                (data.blockRate * 100).ToString("N2"));
            if (data.blockRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextBlockRate != null)
                uiTextBlockRate.text = statsStringPart;

            // Block Dmg Rate
            statsStringPart = string.Format(
                LanguageManager.GetText(blockDmgRateStatsFormat),
                (data.blockDmgRate * 100).ToString("N2"));
            if (data.blockDmgRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextBlockDmgRate != null)
                uiTextBlockDmgRate.text = statsStringPart;

            // Weight
            statsStringPart = string.Format(
                LanguageManager.GetText(weightLimitStatsFormat),
                data.weightLimit.ToString("N2"));
            if (data.weightLimit != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextWeightLimit != null)
                uiTextWeightLimit.text = statsStringPart;

            // Slot
            statsStringPart = string.Format(
                LanguageManager.GetText(slotLimitStatsFormat),
                data.slotLimit.ToString("N0"));
            if (data.slotLimit != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextSlotLimit != null)
                uiTextSlotLimit.text = statsStringPart;

            // Move Speed
            statsStringPart = string.Format(
                LanguageManager.GetText(moveSpeedStatsFormat),
                data.moveSpeed.ToString("N2"));
            if (data.moveSpeed != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextMoveSpeed != null)
                uiTextMoveSpeed.text = statsStringPart;

            // Attack Speed
            statsStringPart = string.Format(
                LanguageManager.GetText(atkSpeedStatsFormat),
                data.atkSpeed.ToString("N2"));
            if (data.atkSpeed != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextAtkSpeed != null)
                uiTextAtkSpeed.text = statsStringPart;

            // Stamina
            statsStringPart = string.Format(
                LanguageManager.GetText(staminaStatsFormat),
                data.stamina.ToString("N0"));
            if (data.stamina != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextStamina != null)
                uiTextStamina.text = statsStringPart;

            // Food
            statsStringPart = string.Format(
                LanguageManager.GetText(foodStatsFormat),
                data.food.ToString("N0"));
            if (data.food != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextFood != null)
                uiTextFood.text = statsStringPart;

            // Water
            statsStringPart = string.Format(
                LanguageManager.GetText(waterStatsFormat),
                data.water.ToString("N0"));
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

        public static string GetRateText(
            CharacterStats data,
            string hpRateStatsFormat,
            string mpRateStatsFormat,
            string accuracyRateStatsFormat,
            string evasionRateStatsFormat,
            string criRateStatsFormat,
            string criDmgRateStatsFormat,
            string blockRateStatsFormat,
            string blockDmgRateStatsFormat,
            string moveSpeedRateStatsFormat,
            string atkSpeedRateStatsFormat,
            string weightLimitRateStatsFormat,
            string slotLimitRateStatsFormat,
            string staminaRateStatsFormat,
            string foodRateStatsFormat,
            string waterRateStatsFormat,
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

            // Hp
            statsStringPart = string.Format(
                LanguageManager.GetText(hpRateStatsFormat),
                (data.hp * 100).ToString("N2"));
            if (data.hp != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextHp != null)
                uiTextHp.text = statsStringPart;

            // Mp
            statsStringPart = string.Format(
                LanguageManager.GetText(mpRateStatsFormat),
                (data.mp * 100).ToString("N2"));
            if (data.mp != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextMp != null)
                uiTextMp.text = statsStringPart;

            // Accuracy
            statsStringPart = string.Format(
                LanguageManager.GetText(accuracyRateStatsFormat),
                (data.accuracy * 100).ToString("N2"));
            if (data.accuracy != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextAccuracy != null)
                uiTextAccuracy.text = statsStringPart;

            // Evasion
            statsStringPart = string.Format(
                LanguageManager.GetText(evasionRateStatsFormat),
                (data.evasion * 100).ToString("N2"));
            if (data.evasion != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextEvasion != null)
                uiTextEvasion.text = statsStringPart;

            // Cri Rate
            statsStringPart = string.Format(
                LanguageManager.GetText(criRateStatsFormat),
                (data.criRate * 100).ToString("N2"));
            if (data.criRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextCriRate != null)
                uiTextCriRate.text = statsStringPart;

            // Cri Dmg Rate
            statsStringPart = string.Format(
                LanguageManager.GetText(criDmgRateStatsFormat),
                (data.criDmgRate * 100).ToString("N2"));
            if (data.criDmgRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextCriDmgRate != null)
                uiTextCriDmgRate.text = statsStringPart;

            // Block Rate
            statsStringPart = string.Format(
                LanguageManager.GetText(blockRateStatsFormat),
                (data.blockRate * 100).ToString("N2"));
            if (data.blockRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextBlockRate != null)
                uiTextBlockRate.text = statsStringPart;

            // Block Dmg Rate
            statsStringPart = string.Format(
                LanguageManager.GetText(blockDmgRateStatsFormat),
                (data.blockDmgRate * 100).ToString("N2"));
            if (data.blockDmgRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextBlockDmgRate != null)
                uiTextBlockDmgRate.text = statsStringPart;

            // Weight
            statsStringPart = string.Format(
                LanguageManager.GetText(weightLimitRateStatsFormat),
                (data.weightLimit * 100).ToString("N2"));
            if (data.weightLimit != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextWeightLimit != null)
                uiTextWeightLimit.text = statsStringPart;

            // Slot
            statsStringPart = string.Format(
                LanguageManager.GetText(slotLimitRateStatsFormat),
                (data.slotLimit * 100).ToString("N2"));
            if (data.slotLimit != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextSlotLimit != null)
                uiTextSlotLimit.text = statsStringPart;

            // Move Speed
            statsStringPart = string.Format(
                LanguageManager.GetText(moveSpeedRateStatsFormat),
                (data.moveSpeed * 100).ToString("N2"));
            if (data.moveSpeed != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextMoveSpeed != null)
                uiTextMoveSpeed.text = statsStringPart;

            // Attack Speed
            statsStringPart = string.Format(
                LanguageManager.GetText(atkSpeedRateStatsFormat),
                (data.atkSpeed * 100).ToString("N2"));
            if (data.atkSpeed != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextAtkSpeed != null)
                uiTextAtkSpeed.text = statsStringPart;

            // Stamina
            statsStringPart = string.Format(
                LanguageManager.GetText(staminaRateStatsFormat),
                (data.stamina * 100).ToString("N2"));
            if (data.stamina != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextStamina != null)
                uiTextStamina.text = statsStringPart;

            // Food
            statsStringPart = string.Format(
                LanguageManager.GetText(foodRateStatsFormat),
                (data.food * 100).ToString("N2"));
            if (data.food != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextFood != null)
                uiTextFood.text = statsStringPart;

            // Water
            statsStringPart = string.Format(
                LanguageManager.GetText(waterRateStatsFormat),
                (data.water * 100).ToString("N2"));
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
            return baseStats + (statsIncreaseEachLevel * (level - 1));
        }
    }
}
