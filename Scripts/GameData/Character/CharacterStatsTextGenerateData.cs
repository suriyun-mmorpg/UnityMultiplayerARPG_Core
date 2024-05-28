using Cysharp.Text;
using System.Text;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial class CharacterStatsTextGenerateData
    {
        public CharacterStats data;
        public bool isRate;
        public bool isBonus;
        public string hpStatsFormat;
        public string hpRecoveryStatsFormat;
        public string hpLeechRateStatsFormat;
        public string mpStatsFormat;
        public string mpRecoveryStatsFormat;
        public string mpLeechRateStatsFormat;
        public string staminaStatsFormat;
        public string staminaRecoveryStatsFormat;
        public string staminaLeechRateStatsFormat;
        public string foodStatsFormat;
        public string waterStatsFormat;
        public string accuracyStatsFormat;
        public string evasionStatsFormat;
        public string criRateStatsFormat;
        public string criDmgRateStatsFormat;
        public string blockRateStatsFormat;
        public string blockDmgRateStatsFormat;
        public string moveSpeedStatsFormat;
        public string atkSpeedStatsFormat;
        public string weightLimitStatsFormat;
        public string slotLimitStatsFormat;
        public string goldRateStatsFormat;
        public string expRateStatsFormat;
        public string itemDropRateStatsFormat;
        public string jumpHeightStatsFormat;
        public string headDamageAbsorbsStatsFormat;
        public string bodyDamageAbsorbsStatsFormat;
        public string fallDamageAbsorbsStatsFormat;
        public string gravityRateStatsFormat;
        public TextWrapper uiTextHp;
        public TextWrapper uiTextHpRecovery;
        public TextWrapper uiTextHpLeechRate;
        public TextWrapper uiTextMp;
        public TextWrapper uiTextMpRecovery;
        public TextWrapper uiTextMpLeechRate;
        public TextWrapper uiTextStamina;
        public TextWrapper uiTextStaminaRecovery;
        public TextWrapper uiTextStaminaLeechRate;
        public TextWrapper uiTextFood;
        public TextWrapper uiTextWater;
        public TextWrapper uiTextAccuracy;
        public TextWrapper uiTextEvasion;
        public TextWrapper uiTextCriRate;
        public TextWrapper uiTextCriDmgRate;
        public TextWrapper uiTextBlockRate;
        public TextWrapper uiTextBlockDmgRate;
        public TextWrapper uiTextMoveSpeed;
        public TextWrapper uiTextAtkSpeed;
        public TextWrapper uiTextWeightLimit;
        public TextWrapper uiTextSlotLimit;
        public TextWrapper uiTextGoldRate;
        public TextWrapper uiTextExpRate;
        public TextWrapper uiTextItemDropRate;
        public TextWrapper uiTextJumpHeight;
        public TextWrapper uiTextHeadDamageAbsorbs;
        public TextWrapper uiTextBodyDamageAbsorbs;
        public TextWrapper uiTextFallDamageAbsorbs;
        public TextWrapper uiTextGravityRate;

        public string numberFormatSimple = "N0";
        public string numberFormatRate = "N2";

        public string GetText()
        {
            StringBuilder statsStringBuilder = new StringBuilder();
            // Hp
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(hpStatsFormat), data.hp, uiTextHp);

            // Hp Recovery
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(hpRecoveryStatsFormat), data.hpRecovery, uiTextHpRecovery);

            // Hp Leech Rate
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(hpLeechRateStatsFormat), data.hpLeechRate, uiTextHpLeechRate);

            // Mp
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(mpStatsFormat), data.mp, uiTextMp);

            // Mp Recovery
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(mpRecoveryStatsFormat), data.mpRecovery, uiTextMpRecovery);

            // Mp Leech Rate
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(mpLeechRateStatsFormat), data.mpLeechRate, uiTextMpLeechRate);

            // Stamina
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(staminaStatsFormat), data.stamina, uiTextStamina);

            // Stamina Recovery
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(staminaRecoveryStatsFormat), data.staminaRecovery, uiTextStaminaRecovery);

            // Stamina Leech Rate
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(staminaLeechRateStatsFormat), data.staminaLeechRate, uiTextStaminaLeechRate);

            // Food
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(foodStatsFormat), data.food, uiTextFood);

            // Water
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(waterStatsFormat), data.water, uiTextWater);

            // Accuracy
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(accuracyStatsFormat), data.accuracy, uiTextAccuracy);

            // Evasion
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(evasionStatsFormat), data.evasion, uiTextEvasion);

            // Cri Rate
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(criRateStatsFormat), data.criRate, uiTextCriRate);

            // Cri Dmg Rate
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(criDmgRateStatsFormat), data.criDmgRate, uiTextCriDmgRate);

            // Block Rate
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(blockRateStatsFormat), data.blockRate, uiTextBlockRate);

            // Block Dmg Rate
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(blockDmgRateStatsFormat), data.blockDmgRate, uiTextBlockDmgRate);

            // Move Speed
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(moveSpeedStatsFormat), data.moveSpeed, uiTextMoveSpeed);

            // Attack Speed
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(atkSpeedStatsFormat), data.atkSpeed, uiTextAtkSpeed);

            // Weight
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(weightLimitStatsFormat), data.weightLimit, uiTextWeightLimit);

            // Slot
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(slotLimitStatsFormat), data.slotLimit, uiTextSlotLimit);

            // Gold Rate
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(goldRateStatsFormat), data.goldRate, uiTextGoldRate);

            // Exp Rate
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(expRateStatsFormat), data.expRate, uiTextExpRate);

            // Item Drop Rate
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(itemDropRateStatsFormat), data.itemDropRate, uiTextItemDropRate);

            // Jump Height
            GetSingleStatsText(statsStringBuilder, isRate || false, LanguageManager.GetText(jumpHeightStatsFormat), data.jumpHeight, uiTextJumpHeight);

            // Head Damage Absorbs
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(headDamageAbsorbsStatsFormat), data.headDamageAbsorbs, uiTextHeadDamageAbsorbs);

            // Body Damage Absorbs
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(bodyDamageAbsorbsStatsFormat), data.bodyDamageAbsorbs, uiTextBodyDamageAbsorbs);

            // Fall Damage Absorbs
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(fallDamageAbsorbsStatsFormat), data.fallDamageAbsorbs, uiTextFallDamageAbsorbs);

            // Gravity Rate
            GetSingleStatsText(statsStringBuilder, isRate || true, LanguageManager.GetText(gravityRateStatsFormat), data.gravityRate, uiTextGravityRate);

            // Dev Extension
            // How to implement it?:
            // /*
            //  * - Add `customStat1` to `CharacterStats` partial class file
            //  * - Add `customStat1StatsFormat` to `CharacterStatsTextGenerateData`
            //  * - Add `uiTextCustomStat1` to `CharacterStatsTextGenerateData`
            //  */
            // [DevExtMethods("GetText")]
            // public void GetText_Ext(StringBuilder statsString)
            // {
            //   string tempValue;
            //   string statsStringPart;
            //   tempValue = isRate ? (data.customStat1 * 100).ToString("N2") : data.customStat1.ToString("N0");
            //   statsStringPart = ZString.Format(LanguageManager.GetText(customStat1StatsFormat), tempValue);
            //   if (data.customStat1 != 0)
            //   {
            //       if (statsString.Length > 0)
            //           statsString.Append('\n');
            //       statsString.Append(statsStringPart);
            //   }
            //   if (uiTextCustomStat1 != null)
            //       uiTextCustomStat1.text = statsStringPart;
            // }
            this.InvokeInstanceDevExtMethods("GetText", statsStringBuilder);

            return statsStringBuilder.ToString();
        }

        public void GetSingleStatsText(StringBuilder builder, bool isRateStats, string format, float value, TextWrapper textComponent)
        {
            // Determine the correct format string based on whether the stat is a rate
            string numberFormat = isRateStats ? numberFormatRate : numberFormatSimple;

            // Calculate the value to display, adjusting for rates if necessary
            string tempValue = isRateStats ? (value * 100).ToString(numberFormat) : value.ToString(numberFormat);

            // Construct the display string
            string statsStringPart = ZString.Concat(isBonus ? "+" : string.Empty, ZString.Format(
                format,
                tempValue));

            // Append the stat text to the builder if the value is not zero
            if (value != 0)
            {
                if (builder.Length > 0)
                    builder.Append('\n');
                builder.Append(statsStringPart);
            }

            // Set the text component if it's provided
            if (textComponent != null)
                textComponent.text = statsStringPart;
        }
    }
}
