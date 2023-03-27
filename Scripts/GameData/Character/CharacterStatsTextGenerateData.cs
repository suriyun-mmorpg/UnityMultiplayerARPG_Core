using Cysharp.Text;

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

        public string GetText()
        {
            using (Utf16ValueStringBuilder statsString = ZString.CreateStringBuilder(false))
            {
                string statsStringPart;
                string tempValue;

                // Hp
                if (isBonus)
                    tempValue = isRate ? (data.hp * 100).ToBonusString("N2") : data.hp.ToBonusString("N0");
                else
                    tempValue = isRate ? (data.hp * 100).ToString("N2") : data.hp.ToString("N0");
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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
                statsStringPart = ZString.Format(
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

                // EtemDrop Rate
                if (isBonus)
                    tempValue = isRate ? (data.itemDropRate * 100).ToBonusString("N2") : (data.itemDropRate * 100).ToBonusString("N0");
                else
                    tempValue = isRate ? (data.itemDropRate * 100).ToString("N2") : (data.itemDropRate * 100).ToString("N0");
                statsStringPart = ZString.Format(
                    LanguageManager.GetText(itemDropRateStatsFormat),
                    tempValue);
                if (data.itemDropRate != 0)
                {
                    if (statsString.Length > 0)
                        statsString.Append('\n');
                    statsString.Append(statsStringPart);
                }
                if (uiTextItemDropRate != null)
                    uiTextItemDropRate.text = statsStringPart;

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
                //   if (isBonus)
                //       tempValue = isRate ? (data.customStat1 * 100).ToBonusString("N2") : data.customStat1.ToBonusString("N0");
                //   else
                //       tempValue = isRate ? (data.customStat1 * 100).ToString("N2") : data.customStat1.ToString("N0");
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
                this.InvokeInstanceDevExtMethods("GetText", statsString);

                return statsString.ToString();
            }
        }
    }
}
