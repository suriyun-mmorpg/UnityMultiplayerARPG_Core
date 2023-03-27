using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterStats : UISelectionEntry<CharacterStats>
    {
        public enum DisplayType
        {
            Simple,
            Rate
        }

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
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyItemDropRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_DROP_RATE);

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
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyItemDropRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_DROP_RATE_RATE);

        [Header("UI Elements")]
        public TextWrapper uiTextStats;
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
        public DisplayType displayType;
        public bool isBonus;

        protected override void UpdateData()
        {
            CharacterStatsTextGenerateData generateTextData;
            string statsString;

            // Dev Extension
            // How to implement it?:
            // /*
            //  * - Add `customStat1` to `CharacterStats` partial class file
            //  * - Add `customStat1StatsFormat` to `CharacterStatsTextGenerateData`
            //  * - Add `uiTextCustomStat1` to `CharacterStatsTextGenerateData`
            //  * - Add `formatKeyCustomStat1Stats` to `UICharacterStats` partial class file
            //  * - Add `formatKeyCustomStat1RateStats` to `UICharacterStats` partial class file
            //  * - Add `uiTextCustomStat1` to `UICharacterStats`
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
            switch (displayType)
            {
                case DisplayType.Rate:
                    generateTextData = new CharacterStatsTextGenerateData()
                    {
                        data = Data,
                        isRate = true,
                        isBonus = isBonus,
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
                        itemDropRateStatsFormat = formatKeyItemDropRateRateStats,
                        uiTextHp = uiTextHp,
                        uiTextHpRecovery = uiTextHpRecovery,
                        uiTextHpLeechRate = uiTextHpLeechRate,
                        uiTextMp = uiTextMp,
                        uiTextMpRecovery = uiTextMpRecovery,
                        uiTextMpLeechRate = uiTextMpLeechRate,
                        uiTextStamina = uiTextStamina,
                        uiTextStaminaRecovery = uiTextStaminaRecovery,
                        uiTextStaminaLeechRate = uiTextStaminaLeechRate,
                        uiTextFood = uiTextFood,
                        uiTextWater = uiTextWater,
                        uiTextAccuracy = uiTextAccuracy,
                        uiTextEvasion = uiTextEvasion,
                        uiTextCriRate = uiTextCriRate,
                        uiTextCriDmgRate = uiTextCriDmgRate,
                        uiTextBlockRate = uiTextBlockRate,
                        uiTextBlockDmgRate = uiTextBlockDmgRate,
                        uiTextMoveSpeed = uiTextMoveSpeed,
                        uiTextAtkSpeed = uiTextAtkSpeed,
                        uiTextWeightLimit = uiTextWeightLimit,
                        uiTextSlotLimit = uiTextSlotLimit,
                        uiTextGoldRate = uiTextGoldRate,
                        uiTextExpRate = uiTextExpRate,
                        uiTextItemDropRate = uiTextItemDropRate,
                    };
                    this.InvokeInstanceDevExtMethods("SetRateStatsGenerateTextData", generateTextData);
                    statsString = generateTextData.GetText();
                    break;
                default:
                    generateTextData = new CharacterStatsTextGenerateData()
                    {
                        data = Data,
                        isRate = false,
                        isBonus = isBonus,
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
                        itemDropRateStatsFormat = formatKeyItemDropRateStats,
                        uiTextHp = uiTextHp,
                        uiTextHpRecovery = uiTextHpRecovery,
                        uiTextHpLeechRate = uiTextHpLeechRate,
                        uiTextMp = uiTextMp,
                        uiTextMpRecovery = uiTextMpRecovery,
                        uiTextMpLeechRate = uiTextMpLeechRate,
                        uiTextStamina = uiTextStamina,
                        uiTextStaminaRecovery = uiTextStaminaRecovery,
                        uiTextStaminaLeechRate = uiTextStaminaLeechRate,
                        uiTextFood = uiTextFood,
                        uiTextWater = uiTextWater,
                        uiTextAccuracy = uiTextAccuracy,
                        uiTextEvasion = uiTextEvasion,
                        uiTextCriRate = uiTextCriRate,
                        uiTextCriDmgRate = uiTextCriDmgRate,
                        uiTextBlockRate = uiTextBlockRate,
                        uiTextBlockDmgRate = uiTextBlockDmgRate,
                        uiTextMoveSpeed = uiTextMoveSpeed,
                        uiTextAtkSpeed = uiTextAtkSpeed,
                        uiTextWeightLimit = uiTextWeightLimit,
                        uiTextSlotLimit = uiTextSlotLimit,
                        uiTextGoldRate = uiTextGoldRate,
                        uiTextExpRate = uiTextExpRate,
                        uiTextItemDropRate = uiTextItemDropRate,
                    };
                    this.InvokeInstanceDevExtMethods("SetStatsGenerateTextData", generateTextData);
                    statsString = generateTextData.GetText();
                    break;
            }

            // All stats text
            if (uiTextStats != null)
            {
                uiTextStats.SetGameObjectActive(!string.IsNullOrEmpty(statsString));
                uiTextStats.text = statsString;
            }
        }
    }
}
