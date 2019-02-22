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
        public float slotLimit;
        public float stamina;
        public float food;
        public float water;

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
            slotLimit = slotLimit * multiplier;
            stamina = stamina * multiplier;
            food = food * multiplier;
            water = water * multiplier;
            return this.InvokeInstanceDevExtMethodsLoopItself("Multiply", multiplier);
        }

        public static CharacterStats operator +(CharacterStats a, CharacterStats b)
        {
            return a.Add(b);
        }

        public static CharacterStats operator *(CharacterStats a, float multiplier)
        {
            return a.Multiply(multiplier);
        }

        public static string GetText(
            CharacterStats data,
            string hpStatsFormat,
            string mpStatsFormat,
            string armorStatsFormat,
            string accuracyStatsFormat,
            string evasionStatsFormat,
            string criRateStatsFormat,
            string criDmgRateStatsFormat,
            string blockRateStatsFormat,
            string blockDmgRateStatsFormat,
            string moveSpeedStatsFormat,
            string atkSpeedStatsFormat,
            string weightLimitStatsFormat,
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
            TextWrapper uiTextStamina = null,
            TextWrapper uiTextFood = null,
            TextWrapper uiTextWater = null)
        {
            string statsString = string.Empty;
            string statsStringPart = string.Empty;

            // Hp
            statsStringPart = string.Format(hpStatsFormat, data.hp.ToString("N0"));
            if (data.hp != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextHp != null)
                uiTextHp.text = statsStringPart;

            // Mp
            statsStringPart = string.Format(mpStatsFormat, data.mp.ToString("N0"));
            if (data.mp != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextMp != null)
                uiTextMp.text = statsStringPart;

            // Armor
            statsStringPart = string.Format(armorStatsFormat, data.armor.ToString("N0"));
            if (data.armor != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextArmor != null)
                uiTextArmor.text = statsStringPart;

            // Accuracy
            statsStringPart = string.Format(accuracyStatsFormat, data.accuracy.ToString("N0"));
            if (data.accuracy != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextAccuracy != null)
                uiTextAccuracy.text = statsStringPart;

            // Evasion
            statsStringPart = string.Format(evasionStatsFormat, data.evasion.ToString("N0"));
            if (data.evasion != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextEvasion != null)
                uiTextEvasion.text = statsStringPart;

            // Cri Rate
            statsStringPart = string.Format(criRateStatsFormat, (data.criRate * 100).ToString("N2"));
            if (data.criRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextCriRate != null)
                uiTextCriRate.text = statsStringPart;

            // Cri Dmg Rate
            statsStringPart = string.Format(criDmgRateStatsFormat, (data.criDmgRate * 100).ToString("N2"));
            if (data.criDmgRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextCriDmgRate != null)
                uiTextCriDmgRate.text = statsStringPart;

            // Block Rate
            statsStringPart = string.Format(blockRateStatsFormat, (data.blockRate * 100).ToString("N2"));
            if (data.blockRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextBlockRate != null)
                uiTextBlockRate.text = statsStringPart;

            // Block Dmg Rate
            statsStringPart = string.Format(blockDmgRateStatsFormat, (data.blockDmgRate * 100).ToString("N2"));
            if (data.blockDmgRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextBlockDmgRate != null)
                uiTextBlockDmgRate.text = statsStringPart;

            // Weight
            statsStringPart = string.Format(weightLimitStatsFormat, data.weightLimit.ToString("N2"));
            if (data.weightLimit != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextWeightLimit != null)
                uiTextWeightLimit.text = statsStringPart;

            // Move Speed
            statsStringPart = string.Format(moveSpeedStatsFormat, data.moveSpeed.ToString("N2"));
            if (data.moveSpeed != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextMoveSpeed != null)
                uiTextMoveSpeed.text = statsStringPart;

            // Attack Speed
            statsStringPart = string.Format(atkSpeedStatsFormat, data.atkSpeed.ToString("N2"));
            if (data.atkSpeed != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextAtkSpeed != null)
                uiTextAtkSpeed.text = statsStringPart;

            // Stamina
            statsStringPart = string.Format(staminaStatsFormat, data.stamina.ToString("N0"));
            if (data.stamina != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextStamina != null)
                uiTextStamina.text = statsStringPart;

            // Food
            statsStringPart = string.Format(foodStatsFormat, data.food.ToString("N0"));
            if (data.food != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (uiTextFood != null)
                uiTextFood.text = statsStringPart;

            // Water
            statsStringPart = string.Format(waterStatsFormat, data.water.ToString("N0"));
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
