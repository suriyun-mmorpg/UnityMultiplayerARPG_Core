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

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyHpStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_HP);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyMpStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MP);
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
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyStaminaStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STAMINA);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyFoodStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_FOOD);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyWaterStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_WATER);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyHpRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_HP_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyMpRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_MP_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyAccuracyRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ACCURACY_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyEvasionRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_EVASION_RATE);
        [Tooltip("Format => {0} = {Amount * 10000}")]
        public UILocaleKeySetting formatKeyCriRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRITICAL_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 10000}")]
        public UILocaleKeySetting formatKeyCriDmgRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRITICAL_DAMAGE_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 10000}")]
        public UILocaleKeySetting formatKeyBlockRateRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BLOCK_RATE_RATE);
        [Tooltip("Format => {0} = {Amount * 10000}")]
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
        public UILocaleKeySetting formatKeyStaminaRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STAMINA_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyFoodRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_FOOD_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyWaterRateStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_WATER_RATE);

        [Header("UI Elements")]
        public TextWrapper uiTextStats;
        public TextWrapper uiTextHp;
        public TextWrapper uiTextMp;
        public TextWrapper uiTextArmor;
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
        public TextWrapper uiTextStamina;
        public TextWrapper uiTextFood;
        public TextWrapper uiTextWater;
        public DisplayType displayType;
        public bool isBonus;

        protected override void UpdateData()
        {
            string statsString;

            switch (displayType)
            {
                case DisplayType.Rate:
                    statsString = CharacterStats.GetText(
                        Data,
                        true,
                        isBonus,
                        formatKeyHpRateStats,
                        formatKeyMpRateStats,
                        formatKeyAccuracyRateStats,
                        formatKeyEvasionRateStats,
                        formatKeyCriRateRateStats,
                        formatKeyCriDmgRateRateStats,
                        formatKeyBlockRateRateStats,
                        formatKeyBlockDmgRateRateStats,
                        formatKeyMoveSpeedRateStats,
                        formatKeyAtkSpeedRateStats,
                        formatKeyWeightLimitRateStats,
                        formatKeySlotLimitRateStats,
                        formatKeyStaminaRateStats,
                        formatKeyFoodRateStats,
                        formatKeyWaterRateStats,
                        uiTextHp,
                        uiTextMp,
                        uiTextArmor,
                        uiTextAccuracy,
                        uiTextEvasion,
                        uiTextCriRate,
                        uiTextCriDmgRate,
                        uiTextBlockRate,
                        uiTextBlockDmgRate,
                        uiTextMoveSpeed,
                        uiTextAtkSpeed,
                        uiTextWeightLimit,
                        uiTextSlotLimit,
                        uiTextStamina,
                        uiTextFood,
                        uiTextWater);
                    break;
                default:
                    statsString = CharacterStats.GetText(
                        Data,
                        false,
                        isBonus,
                        formatKeyHpStats,
                        formatKeyMpStats,
                        formatKeyAccuracyStats,
                        formatKeyEvasionStats,
                        formatKeyCriRateStats,
                        formatKeyCriDmgRateStats,
                        formatKeyBlockRateStats,
                        formatKeyBlockDmgRateStats,
                        formatKeyMoveSpeedStats,
                        formatKeyAtkSpeedStats,
                        formatKeyWeightLimitStats,
                        formatKeySlotLimitStats,
                        formatKeyStaminaStats,
                        formatKeyFoodStats,
                        formatKeyWaterStats,
                        uiTextHp,
                        uiTextMp,
                        uiTextArmor,
                        uiTextAccuracy,
                        uiTextEvasion,
                        uiTextCriRate,
                        uiTextCriDmgRate,
                        uiTextBlockRate,
                        uiTextBlockDmgRate,
                        uiTextMoveSpeed,
                        uiTextAtkSpeed,
                        uiTextWeightLimit,
                        uiTextSlotLimit,
                        uiTextStamina,
                        uiTextFood,
                        uiTextWater);
                    break;
            }

            // All stats text
            if (uiTextStats != null)
            {
                uiTextStats.gameObject.SetActive(!string.IsNullOrEmpty(statsString));
                uiTextStats.text = statsString;
            }
        }
    }
}
