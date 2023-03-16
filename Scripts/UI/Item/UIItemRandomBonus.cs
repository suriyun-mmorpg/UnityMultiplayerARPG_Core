using Cysharp.Text;
using System.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIItemRandomBonus : UISelectionEntry<ItemRandomBonus>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Attribute Title}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public UILocaleKeySetting formatAttributeAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTRIBUTE_AMOUNT);
        public UILocaleKeySetting formatAttributeAmountRate = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTRIBUTE_RATE);
        public UILocaleKeySetting formatResistanceAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_RESISTANCE_AMOUNT);
        public UILocaleKeySetting formatArmorAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ARMOR_AMOUNT);
        public UILocaleKeySetting formatDamageAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_DAMAGE_WITH_ELEMENTAL);
        public UILocaleKeySetting formatSkillLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_LEVEL);
        public string randomAmountSeparator = "~";
        public string randomMinMaxAmountSeparator = "-";

        [Header("UI Elements")]
        public TextWrapper uiTextAllRandomBonus;

        protected override void UpdateData()
        {
            StringBuilder builder = new StringBuilder();
            WriteCharacterStats(builder);
            WriteCharacterStatsRate(builder);
            WriteAttributes(builder);
            WriteAttributeRates(builder);
            WriteResistances(builder);
            WriteArmors(builder);
            WriteDamages(builder);
            WriteSkills(builder);
            if (uiTextAllRandomBonus != null)
                uiTextAllRandomBonus.text = builder.ToString();
        }

        private void WriteCharacterStats(StringBuilder builder)
        {
            // Hp
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_HP.ToString()), Data.randomCharacterStats.hpApplyRate, Data.randomCharacterStats.minHp, Data.randomCharacterStats.maxHp);
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_HP_RECOVERY.ToString()), Data.randomCharacterStats.hpRecoveryApplyRate, Data.randomCharacterStats.minHpRecovery, Data.randomCharacterStats.maxHpRecovery);
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_HP_LEECH_RATE.ToString()), Data.randomCharacterStats.hpLeechRateApplyRate, Data.randomCharacterStats.minHpLeechRate, Data.randomCharacterStats.maxHpLeechRate);
            // Mp
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_MP.ToString()), Data.randomCharacterStats.mpApplyRate, Data.randomCharacterStats.minMp, Data.randomCharacterStats.maxMp);
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_MP_RECOVERY.ToString()), Data.randomCharacterStats.mpRecoveryApplyRate, Data.randomCharacterStats.minMpRecovery, Data.randomCharacterStats.maxMpRecovery);
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_MP_LEECH_RATE.ToString()), Data.randomCharacterStats.mpLeechRateApplyRate, Data.randomCharacterStats.minMpLeechRate, Data.randomCharacterStats.maxMpLeechRate);
            // Stamina
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_STAMINA.ToString()), Data.randomCharacterStats.staminaApplyRate, Data.randomCharacterStats.minStamina, Data.randomCharacterStats.maxStamina);
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_STAMINA_RECOVERY.ToString()), Data.randomCharacterStats.staminaRecoveryApplyRate, Data.randomCharacterStats.minStaminaRecovery, Data.randomCharacterStats.maxStaminaRecovery);
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_STAMINA_LEECH_RATE.ToString()), Data.randomCharacterStats.staminaLeechRateApplyRate, Data.randomCharacterStats.minStaminaLeechRate, Data.randomCharacterStats.maxStaminaLeechRate);
            // Food
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_FOOD.ToString()), Data.randomCharacterStats.foodApplyRate, Data.randomCharacterStats.minFood, Data.randomCharacterStats.maxFood);
            // Water
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_WATER.ToString()), Data.randomCharacterStats.waterApplyRate, Data.randomCharacterStats.minWater, Data.randomCharacterStats.maxWater);
            // Accuracy
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_ACCURACY.ToString()), Data.randomCharacterStats.accuracyApplyRate, Data.randomCharacterStats.minAccuracy, Data.randomCharacterStats.maxAccuracy);
            // Evasion
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_EVASION.ToString()), Data.randomCharacterStats.evasionApplyRate, Data.randomCharacterStats.minEvasion, Data.randomCharacterStats.maxEvasion);
            // CriRate
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_CRITICAL_RATE.ToString()), Data.randomCharacterStats.criRateApplyRate, Data.randomCharacterStats.minCriRate, Data.randomCharacterStats.maxCriRate);
            // CriDmgRate
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_CRITICAL_DAMAGE_RATE.ToString()), Data.randomCharacterStats.criDmgRateApplyRate, Data.randomCharacterStats.minCriDmgRate, Data.randomCharacterStats.maxCriDmgRate);
            // BlockRate
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_BLOCK_RATE.ToString()), Data.randomCharacterStats.blockRateApplyRate, Data.randomCharacterStats.minBlockRate, Data.randomCharacterStats.maxBlockRate);
            // BlockDmgRate
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_BLOCK_DAMAGE_RATE.ToString()), Data.randomCharacterStats.blockDmgRateApplyRate, Data.randomCharacterStats.minBlockDmgRate, Data.randomCharacterStats.maxBlockDmgRate);
            // MoveSpeed
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_MOVE_SPEED.ToString()), Data.randomCharacterStats.moveSpeedApplyRate, Data.randomCharacterStats.minMoveSpeed, Data.randomCharacterStats.maxMoveSpeed);
            // AtkSpeed
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_ATTACK_SPEED.ToString()), Data.randomCharacterStats.atkSpeedApplyRate, Data.randomCharacterStats.minAtkSpeed, Data.randomCharacterStats.maxAtkSpeed);
            // WeightLimit
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_WEIGHT.ToString()), Data.randomCharacterStats.weightLimitApplyRate, Data.randomCharacterStats.minWeightLimit, Data.randomCharacterStats.maxWeightLimit);
            // SlotLimit
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_SLOT.ToString()), Data.randomCharacterStats.slotLimitApplyRate, Data.randomCharacterStats.minSlotLimit, Data.randomCharacterStats.maxSlotLimit);
            // GoldRate
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_GOLD_RATE.ToString()), Data.randomCharacterStats.goldRateApplyRate, Data.randomCharacterStats.minGoldRate, Data.randomCharacterStats.maxGoldRate);
            // ExpRate
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_EXP_RATE.ToString()), Data.randomCharacterStats.expRateApplyRate, Data.randomCharacterStats.minExpRate, Data.randomCharacterStats.maxExpRate);
        }

        private void WriteCharacterStatsRate(StringBuilder builder)
        {
            // Hp
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_HP_RATE.ToString()), Data.randomCharacterStats.hpApplyRate, Data.randomCharacterStats.minHp, Data.randomCharacterStats.maxHp, 100f);
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_HP_RECOVERY_RATE.ToString()), Data.randomCharacterStats.hpRecoveryApplyRate, Data.randomCharacterStats.minHpRecovery, Data.randomCharacterStats.maxHpRecovery, 100f);
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_HP_LEECH_RATE_RATE.ToString()), Data.randomCharacterStats.hpLeechRateApplyRate, Data.randomCharacterStats.minHpLeechRate, Data.randomCharacterStats.maxHpLeechRate, 100f);
            // Mp
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_MP_RATE.ToString()), Data.randomCharacterStats.mpApplyRate, Data.randomCharacterStats.minMp, Data.randomCharacterStats.maxMp, 100f);
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_MP_RECOVERY_RATE.ToString()), Data.randomCharacterStats.mpRecoveryApplyRate, Data.randomCharacterStats.minMpRecovery, Data.randomCharacterStats.maxMpRecovery, 100f);
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_MP_LEECH_RATE_RATE.ToString()), Data.randomCharacterStats.mpLeechRateApplyRate, Data.randomCharacterStats.minMpLeechRate, Data.randomCharacterStats.maxMpLeechRate, 100f);
            // Stamina
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_STAMINA_RATE.ToString()), Data.randomCharacterStats.staminaApplyRate, Data.randomCharacterStats.minStamina, Data.randomCharacterStats.maxStamina, 100f);
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_STAMINA_RECOVERY_RATE.ToString()), Data.randomCharacterStats.staminaRecoveryApplyRate, Data.randomCharacterStats.minStaminaRecovery, Data.randomCharacterStats.maxStaminaRecovery, 100f);
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_STAMINA_LEECH_RATE_RATE.ToString()), Data.randomCharacterStats.staminaLeechRateApplyRate, Data.randomCharacterStats.minStaminaLeechRate, Data.randomCharacterStats.maxStaminaLeechRate, 100f);
            // Food
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_FOOD_RATE.ToString()), Data.randomCharacterStats.foodApplyRate, Data.randomCharacterStats.minFood, Data.randomCharacterStats.maxFood, 100f);
            // Water
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_WATER_RATE.ToString()), Data.randomCharacterStats.waterApplyRate, Data.randomCharacterStats.minWater, Data.randomCharacterStats.maxWater, 100f);
            // Accuracy
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_ACCURACY_RATE.ToString()), Data.randomCharacterStats.accuracyApplyRate, Data.randomCharacterStats.minAccuracy, Data.randomCharacterStats.maxAccuracy, 100f);
            // Evasion
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_EVASION_RATE.ToString()), Data.randomCharacterStats.evasionApplyRate, Data.randomCharacterStats.minEvasion, Data.randomCharacterStats.maxEvasion, 100f);
            // CriRate
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_CRITICAL_RATE_RATE.ToString()), Data.randomCharacterStats.criRateApplyRate, Data.randomCharacterStats.minCriRate, Data.randomCharacterStats.maxCriRate, 100f);
            // CriDmgRate
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_CRITICAL_DAMAGE_RATE_RATE.ToString()), Data.randomCharacterStats.criDmgRateApplyRate, Data.randomCharacterStats.minCriDmgRate, Data.randomCharacterStats.maxCriDmgRate, 100f);
            // BlockRate
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_BLOCK_RATE_RATE.ToString()), Data.randomCharacterStats.blockRateApplyRate, Data.randomCharacterStats.minBlockRate, Data.randomCharacterStats.maxBlockRate, 100f);
            // BlockDmgRate
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_BLOCK_DAMAGE_RATE_RATE.ToString()), Data.randomCharacterStats.blockDmgRateApplyRate, Data.randomCharacterStats.minBlockDmgRate, Data.randomCharacterStats.maxBlockDmgRate, 100f);
            // MoveSpeed
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_MOVE_SPEED_RATE.ToString()), Data.randomCharacterStats.moveSpeedApplyRate, Data.randomCharacterStats.minMoveSpeed, Data.randomCharacterStats.maxMoveSpeed, 100f);
            // AtkSpeed
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_ATTACK_SPEED_RATE.ToString()), Data.randomCharacterStats.atkSpeedApplyRate, Data.randomCharacterStats.minAtkSpeed, Data.randomCharacterStats.maxAtkSpeed, 100f);
            // WeightLimit
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_WEIGHT_RATE.ToString()), Data.randomCharacterStats.weightLimitApplyRate, Data.randomCharacterStats.minWeightLimit, Data.randomCharacterStats.maxWeightLimit, 100f);
            // SlotLimit
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_SLOT_RATE.ToString()), Data.randomCharacterStats.slotLimitApplyRate, Data.randomCharacterStats.minSlotLimit, Data.randomCharacterStats.maxSlotLimit, 100f);
            // GoldRate
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_GOLD_RATE_RATE.ToString()), Data.randomCharacterStats.goldRateApplyRate, Data.randomCharacterStats.minGoldRate, Data.randomCharacterStats.maxGoldRate, 100f);
            // ExpRate
            WriteEntry(builder, LanguageManager.GetText(UIFormatKeys.UI_FORMAT_EXP_RATE_RATE.ToString()), Data.randomCharacterStats.expRateApplyRate, Data.randomCharacterStats.minExpRate, Data.randomCharacterStats.maxExpRate, 100f);
        }

        private void WriteAttributes(StringBuilder builder)
        {
            if (Data.randomAttributeAmounts == null || Data.randomAttributeAmounts.Length == 0)
                return;
            foreach (AttributeRandomAmount entry in Data.randomAttributeAmounts)
            {
                if (entry.attribute == null)
                    continue;
                WriteEntry(builder, LanguageManager.GetText(formatAttributeAmount), entry.attribute.Title, entry.applyRate, entry.minAmount, entry.maxAmount);
            }
        }

        private void WriteAttributeRates(StringBuilder builder)
        {
            if (Data.randomAttributeAmountRates == null || Data.randomAttributeAmountRates.Length == 0)
                return;
            foreach (AttributeRandomAmount entry in Data.randomAttributeAmountRates)
            {
                if (entry.attribute == null)
                    continue;
                WriteEntry(builder, LanguageManager.GetText(formatAttributeAmountRate), entry.attribute.Title, entry.applyRate, entry.minAmount, entry.maxAmount, 100f);
            }
        }

        private void WriteResistances(StringBuilder builder)
        {
            if (Data.randomResistanceAmounts == null || Data.randomResistanceAmounts.Length == 0)
                return;
            DamageElement tempDamageElement;
            foreach (ResistanceRandomAmount entry in Data.randomResistanceAmounts)
            {
                tempDamageElement = entry.damageElement;
                if (tempDamageElement == null)
                    tempDamageElement = GameInstance.Singleton.DefaultDamageElement;
                WriteEntry(builder, LanguageManager.GetText(formatResistanceAmount), entry.damageElement.Title, entry.applyRate, entry.minAmount, entry.maxAmount);
            }
        }

        private void WriteArmors(StringBuilder builder)
        {
            if (Data.randomArmorAmounts == null || Data.randomArmorAmounts.Length == 0)
                return;
            DamageElement tempDamageElement;
            foreach (ArmorRandomAmount entry in Data.randomArmorAmounts)
            {
                tempDamageElement = entry.damageElement;
                if (tempDamageElement == null)
                    tempDamageElement = GameInstance.Singleton.DefaultDamageElement;
                WriteEntry(builder, LanguageManager.GetText(formatArmorAmount), entry.damageElement.Title, entry.applyRate, entry.minAmount, entry.maxAmount);
            }
        }

        private void WriteDamages(StringBuilder builder)
        {
            if (Data.randomDamageAmounts == null || Data.randomDamageAmounts.Length == 0)
                return;
            DamageElement tempDamageElement;
            foreach (DamageRandomAmount entry in Data.randomDamageAmounts)
            {
                tempDamageElement = entry.damageElement;
                if (tempDamageElement == null)
                    tempDamageElement = GameInstance.Singleton.DefaultDamageElement;
                WriteEntry(builder, LanguageManager.GetText(formatDamageAmount), entry.damageElement.Title, entry.applyRate, entry.minAmount, entry.maxAmount);
            }
        }

        private void WriteSkills(StringBuilder builder)
        {
            if (Data.randomSkillLevels == null || Data.randomSkillLevels.Length == 0)
                return;
            foreach (SkillRandomLevel entry in Data.randomSkillLevels)
            {
                if (entry.skill == null)
                    continue;
                WriteEntry(builder, LanguageManager.GetText(formatSkillLevel), entry.skill.Title, entry.applyRate, entry.minLevel, entry.maxLevel);
            }
        }

        private void WriteEntry(StringBuilder builder, string format, float applyRate, float minValue, float maxValue, float multiplier = 1f)
        {
            if (applyRate <= 0f && minValue == 0 && maxValue == 0)
                return;
            if (builder.Length > 0)
                builder.Append('\n');
            builder.Append(ZString.Format(format,
                ZString.Concat((minValue * multiplier).ToString("N2"), randomAmountSeparator, (maxValue * multiplier).ToString("N2"))));
        }

        private void WriteEntry(StringBuilder builder, string format, string title, float applyRate, float minValue, float maxValue, float multiplier = 1f)
        {
            if (applyRate <= 0f && minValue == 0 && maxValue == 0)
                return;
            if (builder.Length > 0)
                builder.Append('\n');
            builder.Append(ZString.Format(format, title,
                ZString.Concat((minValue * multiplier).ToString("N2"), randomAmountSeparator, (maxValue * multiplier).ToString("N2"))));
        }

        private void WriteEntry(StringBuilder builder, string format, string title, float applyRate, MinMaxFloat minValue, MinMaxFloat maxValue, float multiplier = 1f)
        {
            if (applyRate <= 0f && minValue.min == 0 && minValue.max == 0 && maxValue.min == 0 && maxValue.max == 0)
                return;
            if (builder.Length > 0)
                builder.Append('\n');
            builder.Append(ZString.Format(format, title,
                ZString.Concat((minValue.min * multiplier).ToString("N2"), randomMinMaxAmountSeparator, (minValue.max * multiplier).ToString("N2")),
                ZString.Concat((maxValue.min * multiplier).ToString("N2"), randomMinMaxAmountSeparator, (maxValue.max * multiplier).ToString("N2"))));
        }
    }
}
