using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterStats : UISelectionEntry<CharacterStats>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatHpStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatMpStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatArmorStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatAccuracyStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatEvasionStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatCriRateStats = "{0}: {1}%";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatCriDmgRateStats = "{0}: {1}%";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatBlockRateStats = "{0}: {1}%";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatBlockDmgRateStats = "{0}: {1}%";
        [Tooltip("Format => {0} = {Label}, {1} = {Move Speed}")]
        public string formatMoveSpeedStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Attack Speed}")]
        public string formatAtkSpeedStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Weight Limit}")]
        public string formatWeightLimitStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatStaminaStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatFoodStats = "{0}: {1}";
        [Tooltip("Format => {0} = {Label}, {1} = {Amount}")]
        public string formatWaterStats = "{0}: {1}";

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
                formatHpStats,
                formatMpStats,
                formatArmorStats,
                formatAccuracyStats,
                formatEvasionStats,
                formatCriRateStats,
                formatCriDmgRateStats,
                formatBlockRateStats,
                formatBlockDmgRateStats,
                formatMoveSpeedStats,
                formatAtkSpeedStats,
                formatWeightLimitStats,
                formatStaminaStats,
                formatFoodStats,
                formatWaterStats,
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
