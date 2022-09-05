using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class UIBaseEquipmentBonus<T> : UISelectionEntry<T>
    {
        [Header("String Formats (Stats)")]
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyHpStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_HP);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyHpRecoveryStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_HP_RECOVERY);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyHpLeechRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_HP_LEECH_RATE);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyMpStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MP);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyMpRecoveryStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MP_RECOVERY);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyMpLeechRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MP_LEECH_RATE);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyStaminaStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STAMINA);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyStaminaRecoveryStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STAMINA_RECOVERY);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyStaminaLeechRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STAMINA_LEECH_RATE);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyFoodStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_FOOD);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyWaterStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_WATER);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyAccuracyStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ACCURACY);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyEvasionStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_EVASION);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyCriRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRITICAL_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyCriDmgRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRITICAL_DAMAGE_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyBlockRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BLOCK_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyBlockDmgRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BLOCK_DAMAGE_RATE);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyMoveSpeedStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MOVE_SPEED);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyAtkSpeedStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTACK_SPEED);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyWeightLimitStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_WEIGHT);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeySlotLimitStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SLOT);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyGoldRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyExpRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_EXP_RATE);

        [Header("String Formats (Stats Rate)")]
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyHpRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_HP_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyHpRecoveryRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_HP_RECOVERY_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyHpLeechRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_HP_LEECH_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyMpRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MP_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyMpRecoveryRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MP_RECOVERY_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyMpLeechRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MP_LEECH_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyStaminaRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STAMINA_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyStaminaRecoveryRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STAMINA_RECOVERY_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyStaminaLeechRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STAMINA_LEECH_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyFoodRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_FOOD_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyWaterRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_WATER_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyAccuracyRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ACCURACY_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyEvasionRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_EVASION_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyCriRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRITICAL_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyCriDmgRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRITICAL_DAMAGE_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyBlockRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BLOCK_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyBlockDmgRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BLOCK_DAMAGE_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyMoveSpeedRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MOVE_SPEED_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyAtkSpeedRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTACK_SPEED_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyWeightLimitRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_WEIGHT_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeySlotLimitRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SLOT_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyGoldRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyExpRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_EXP_RATE_RATE);

        [Header("String Formats (Attribute/Damage Element/Skill)")]
        [Tooltip("Format => {0} = {Attribute Title}, {1} = {Amount}")]
        public UILocaleKeySetting formatKeyAttributeAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTRIBUTE_AMOUNT);
        [Tooltip("Format => {0} = {Attribute Title}, {1} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyAttributeAmountRate = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ATTRIBUTE_RATE);
        [Tooltip("Format => {0} = {Damage Element Title}, {1} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyResistanceAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_RESISTANCE_AMOUNT);
        [Tooltip("Format => {0} = {Damage Element Title}, {1} = {Min Damage}, {2} = {Max Damage}")]
        public UILocaleKeySetting formatKeyDamageAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_DAMAGE_WITH_ELEMENTAL);
        [Tooltip("Format => {0} = {Damage Element Title}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyArmorAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ARMOR_AMOUNT);
        [Tooltip("Format => {0} = {Skill Title}, {1} = {Level}")]
        public UILocaleKeySetting formatKeySkillLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_LEVEL);

        [Header("UI Elements")]
        public TextWrapper uiTextAllBonus;

        public string GetEquipmentBonusText(EquipmentBonus equipmentBonus)
        {
            using (Utf16ValueStringBuilder result = ZString.CreateStringBuilder(false))
            {
                CharacterStatsTextGenerateData generateTextData;
                // Dev Extension
                // How to implement it?:
                // /*
                //  * - Add `customStat1` to `CharacterStats` partial class file
                //  * - Add `customStat1StatsFormat` to `CharacterStatsTextGenerateData`
                //  * - Add `uiTextCustomStat1` to `CharacterStatsTextGenerateData`
                //  * - Add `formatKeyCustomStat1Stats` to `UIBaseEquipmentBonus` partial class file
                //  * - Add `formatKeyCustomStat1RateStats` to `UIBaseEquipmentBonus` partial class file
                //  * - Add `uiTextCustomStat1` to `UIBaseEquipmentBonus`
                //  */
                // [DevExtMethods("SetStatsGenerateTextData")]
                // public void SetStatsGenerateTextData_Ext(CharacterStatsTextGenerateData generateTextData)
                // {
                //   generateTextData.customStat1StatsFormat = formatKeyCustomStat1Stats;
                //   generateTextData.uiTextCustomStat1 = uiTextCustomStat1;
                // }
                // [DevExtMethods("SetRateStatsGenerateTextData")]
                // public void SetRateStatsGenerateTextData_Ext(CharacterStatsTextGenerateData generateTextData)
                // {
                //   generateTextData.customStat1StatsFormat = formatKeyCustomStat1RateStats;
                //   generateTextData.uiTextCustomStat1 = uiTextCustomStat1;
                // }
                // Non-rate stats
                generateTextData = new CharacterStatsTextGenerateData()
                {
                    data = equipmentBonus.stats,
                    isRate = false,
                    isBonus = true,
                    hpStatsFormat = formatKeyHpStats,
                    hpRecoveryStatsFormat = formatKeyHpRecoveryStats,
                    hpLeechRateStatsFormat = formatKeyHpLeechRateStats,
                    mpStatsFormat = formatKeyMpStats,
                    mpRecoveryStatsFormat = formatKeyMpRecoveryStats,
                    mpLeechRateStatsFormat = formatKeyMpLeechRateStats,
                    staminaStatsFormat = formatKeyStaminaStats,
                    staminaRecoveryStatsFormat = formatKeyStaminaRecoveryStats,
                    staminaLeechRateStatsFormat = formatKeyStaminaLeechRateStats,
                    foodStatsFormat = formatKeyFoodStats,
                    waterStatsFormat = formatKeyWaterStats,
                    accuracyStatsFormat = formatKeyAccuracyStats,
                    evasionStatsFormat = formatKeyEvasionStats,
                    criRateStatsFormat = formatKeyCriRateStats,
                    criDmgRateStatsFormat = formatKeyCriDmgRateStats,
                    blockRateStatsFormat = formatKeyBlockRateStats,
                    blockDmgRateStatsFormat = formatKeyBlockDmgRateStats,
                    moveSpeedStatsFormat = formatKeyMoveSpeedStats,
                    atkSpeedStatsFormat = formatKeyAtkSpeedStats,
                    weightLimitStatsFormat = formatKeyWeightLimitStats,
                    slotLimitStatsFormat = formatKeySlotLimitStats,
                    goldRateStatsFormat = formatKeyGoldRateStats,
                    expRateStatsFormat = formatKeyExpRateStats,
                };
                this.InvokeInstanceDevExtMethods("SetStatsGenerateTextData", generateTextData);
                string statsText = generateTextData.GetText();
                // Rate stats
                generateTextData = new CharacterStatsTextGenerateData()
                {
                    data = equipmentBonus.statsRate,
                    isRate = true,
                    isBonus = true,
                    hpStatsFormat = formatKeyHpRateStats,
                    hpRecoveryStatsFormat = formatKeyHpRecoveryRateStats,
                    hpLeechRateStatsFormat = formatKeyHpLeechRateRateStats,
                    mpStatsFormat = formatKeyMpRateStats,
                    mpRecoveryStatsFormat = formatKeyMpRecoveryRateStats,
                    mpLeechRateStatsFormat = formatKeyMpLeechRateRateStats,
                    staminaStatsFormat = formatKeyStaminaRateStats,
                    staminaRecoveryStatsFormat = formatKeyStaminaRecoveryRateStats,
                    staminaLeechRateStatsFormat = formatKeyStaminaLeechRateRateStats,
                    foodStatsFormat = formatKeyFoodRateStats,
                    waterStatsFormat = formatKeyWaterRateStats,
                    accuracyStatsFormat = formatKeyAccuracyRateStats,
                    evasionStatsFormat = formatKeyEvasionRateStats,
                    criRateStatsFormat = formatKeyCriRateRateStats,
                    criDmgRateStatsFormat = formatKeyCriDmgRateRateStats,
                    blockRateStatsFormat = formatKeyBlockRateRateStats,
                    blockDmgRateStatsFormat = formatKeyBlockDmgRateRateStats,
                    moveSpeedStatsFormat = formatKeyMoveSpeedRateStats,
                    atkSpeedStatsFormat = formatKeyAtkSpeedRateStats,
                    weightLimitStatsFormat = formatKeyWeightLimitRateStats,
                    slotLimitStatsFormat = formatKeySlotLimitRateStats,
                    goldRateStatsFormat = formatKeyGoldRateRateStats,
                    expRateStatsFormat = formatKeyExpRateRateStats,
                };
                this.InvokeInstanceDevExtMethods("SetRateStatsGenerateTextData", generateTextData);
                string rateStatsText = generateTextData.GetText();

                if (!string.IsNullOrEmpty(statsText))
                    result.Append(statsText);

                if (!string.IsNullOrEmpty(rateStatsText))
                {
                    if (result.Length > 0)
                        result.Append('\n');
                    result.Append(rateStatsText);
                }

                // Attributes
                foreach (AttributeAmount entry in equipmentBonus.attributes)
                {
                    if (entry.attribute == null || entry.amount == 0)
                        continue;
                    if (result.Length > 0)
                        result.Append('\n');
                    result.AppendFormat(
                        LanguageManager.GetText(formatKeyAttributeAmount),
                        entry.attribute.Title,
                        entry.amount.ToBonusString("N0"));
                }
                foreach (AttributeAmount entry in equipmentBonus.attributesRate)
                {
                    if (entry.attribute == null || entry.amount == 0)
                        continue;
                    if (result.Length > 0)
                        result.Append('\n');
                    result.AppendFormat(
                        LanguageManager.GetText(formatKeyAttributeAmountRate),
                        entry.attribute.Title,
                        (entry.amount * 100).ToBonusString("N2"));
                }

                DamageElement tempElement;
                // Resistances
                foreach (ResistanceAmount entry in equipmentBonus.resistances)
                {
                    if (entry.amount == 0)
                        continue;
                    if (result.Length > 0)
                        result.Append('\n');
                    tempElement = entry.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : entry.damageElement;
                    result.AppendFormat(
                        LanguageManager.GetText(formatKeyResistanceAmount),
                        tempElement.Title,
                        (entry.amount * 100).ToBonusString("N2"));
                }

                // Damages
                foreach (DamageAmount entry in equipmentBonus.damages)
                {
                    if (entry.amount.min == 0 && entry.amount.max == 0)
                        continue;
                    if (result.Length > 0)
                        result.Append('\n');
                    tempElement = entry.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : entry.damageElement;
                    result.AppendFormat(
                        LanguageManager.GetText(formatKeyDamageAmount),
                        tempElement.Title,
                        entry.amount.min.ToBonusString("N0"),
                        entry.amount.max.ToString("N0"));
                }

                // Armors
                foreach (ArmorAmount entry in equipmentBonus.armors)
                {
                    if (entry.amount == 0)
                        continue;
                    if (result.Length > 0)
                        result.Append('\n');
                    tempElement = entry.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : entry.damageElement;
                    result.AppendFormat(
                        LanguageManager.GetText(formatKeyArmorAmount),
                        tempElement.Title,
                        entry.amount.ToBonusString("N0"));
                }

                // Skills
                foreach (SkillLevel entry in equipmentBonus.skills)
                {
                    if (entry.skill == null || entry.level == 0)
                        continue;
                    if (result.Length > 0)
                        result.Append('\n');
                    result.AppendFormat(
                        LanguageManager.GetText(formatKeySkillLevel),
                        entry.skill.Title,
                        entry.level.ToBonusString("N0"));
                }

                return result.ToString();
            }
        }
    }
}
