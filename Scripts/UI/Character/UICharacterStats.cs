using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterStats : UISelectionEntry<CharacterStats>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyHpStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_HP);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyMpStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_MP);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyArmorStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_ARMOR);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyAccuracyStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_ACCURACY);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyEvasionStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_EVASION);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyCriRateStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_CRITICAL_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyCriDmgRateStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_CRITICAL_DAMAGE_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyBlockRateStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_BLOCK_RATE);
        [Tooltip("Format => {0} = {Amount * 100}")]
        public UILocaleKeySetting formatKeyBlockDmgRateStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_BLOCK_DAMAGE_RATE);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyMoveSpeedStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_MOVE_SPEED);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyAtkSpeedStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_ATTACK_SPEED);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyWeightLimitStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_WEIGHT);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyStaminaStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_STAMINA);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyFoodStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_FOOD);
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyWaterStats = new UILocaleKeySetting(UILocaleKeys.UI_FORMAT_WATER);

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
        public TextWrapper uiTextStamina;
        public TextWrapper uiTextFood;
        public TextWrapper uiTextWater;

        protected override void UpdateData()
        {
            string statsString = CharacterStats.GetText(
                Data,
                formatKeyHpStats,
                formatKeyMpStats,
                formatKeyArmorStats,
                formatKeyAccuracyStats,
                formatKeyEvasionStats,
                formatKeyCriRateStats,
                formatKeyCriDmgRateStats,
                formatKeyBlockRateStats,
                formatKeyBlockDmgRateStats,
                formatKeyMoveSpeedStats,
                formatKeyAtkSpeedStats,
                formatKeyWeightLimitStats,
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
                uiTextStamina,
                uiTextFood,
                uiTextWater);

            // All stats text
            if (uiTextStats != null)
            {
                uiTextStats.gameObject.SetActive(!string.IsNullOrEmpty(statsString));
                uiTextStats.text = statsString;
            }
        }
    }
}
