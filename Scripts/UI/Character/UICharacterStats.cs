using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterStats : UISelectionEntry<CharacterStats>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyHpStats = UILocaleKeys.UI_FORMAT_HP.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyMpStats = UILocaleKeys.UI_FORMAT_MP.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyArmorStats = UILocaleKeys.UI_FORMAT_ARMOR.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyAccuracyStats = UILocaleKeys.UI_FORMAT_ACCURACY.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyEvasionStats = UILocaleKeys.UI_FORMAT_EVASION.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyCriRateStats = UILocaleKeys.UI_FORMAT_CRITICAL_RATE.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyCriDmgRateStats = UILocaleKeys.UI_FORMAT_CRITICAL_DAMAGE_RATE.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyBlockRateStats = UILocaleKeys.UI_FORMAT_BLOCK_RATE.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyBlockDmgRateStats = UILocaleKeys.UI_FORMAT_BLOCK_DAMAGE_RATE.ToString();
        [Tooltip("Format => {0} = {Move Speed}")]
        public string formatKeyMoveSpeedStats = UILocaleKeys.UI_FORMAT_MOVE_SPEED.ToString();
        [Tooltip("Format => {0} = {Attack Speed}")]
        public string formatKeyAtkSpeedStats = UILocaleKeys.UI_FORMAT_ATTACK_SPEED.ToString();
        [Tooltip("Format => {0} = {Weight Limit}")]
        public string formatKeyWeightLimitStats = UILocaleKeys.UI_FORMAT_WEIGHT.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyStaminaStats = UILocaleKeys.UI_FORMAT_STAMINA.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyFoodStats = UILocaleKeys.UI_FORMAT_FOOD.ToString();
        [Tooltip("Format => {0} = {Amount}")]
        public string formatKeyWaterStats = UILocaleKeys.UI_FORMAT_WATER.ToString();

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
