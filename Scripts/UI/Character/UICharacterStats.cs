using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterStats : UISelectionEntry<CharacterStats>
    {
        [Header("Format")]
        [Tooltip("Hp Stats Format => {0} = {Amount}, {1} = {Label}")]
        public string hpStatsFormat = "{1}: {0}";
        [Tooltip("Mp Stats Format => {0} = {Amount}, {1} = {Label}")]
        public string mpStatsFormat = "{1}: {0}";
        [Tooltip("Armor Stats Format => {0} = {Amount}, {1} = {Label}")]
        public string armorStatsFormat = "{1}: {0}";
        [Tooltip("Accuracy Stats Format => {0} = {Amount}, {1} = {Label}")]
        public string accuracyStatsFormat = "{1}: {0}";
        [Tooltip("Evasion Format => {0} = {Amount}, {1} = {Label}")]
        public string evasionStatsFormat = "{1}: {0}";
        [Tooltip("Cri Rate Stats Format => {0} = {Amount}, {1} = {Label}")]
        public string criRateStatsFormat = "{1}: {0}%";
        [Tooltip("Cri Dmg Rate Stats Format => {0} = {Amount}, {1} = {Label}")]
        public string criDmgRateStatsFormat = "{1}: {0}%";
        [Tooltip("Block Rate Stats Format => {0} = {Amount}, {1} = {Label}")]
        public string blockRateStatsFormat = "{1}: {0}%";
        [Tooltip("Block Dmg Rate Stats Format => {0} = {Amount}, {1} = {Label}")]
        public string blockDmgRateStatsFormat = "{1}: {0}%";
        [Tooltip("Move Speed Stats Format => {0} = {Move Speed}, {1} = {Label}")]
        public string moveSpeedStatsFormat = "{1}: {0}";
        [Tooltip("Attack Speed Stats Format => {0} = {Attack Speed}, {1} = {Label}")]
        public string atkSpeedStatsFormat = "{1}: {0}";
        [Tooltip("Weight Limit Stats Format => {0} = {Weight Limit}, {1} = {Label}")]
        public string weightLimitStatsFormat = "{1}: {0}";
        [Tooltip("Stamina Stats Format => {0} = {Amount}, {1} = {Label}")]
        public string staminaStatsFormat = "{1}: {0}";
        [Tooltip("Food Stats Format => {0} = {Amount}, {1} = {Label}")]
        public string foodStatsFormat = "{1}: {0}";
        [Tooltip("Water Stats Format => {0} = {Amount}, {1} = {Label}")]
        public string waterStatsFormat = "{1}: {0}";

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
                hpStatsFormat,
                mpStatsFormat,
                armorStatsFormat,
                accuracyStatsFormat,
                evasionStatsFormat,
                criRateStatsFormat,
                criDmgRateStatsFormat,
                blockRateStatsFormat,
                blockDmgRateStatsFormat,
                moveSpeedStatsFormat,
                atkSpeedStatsFormat,
                weightLimitStatsFormat,
                staminaStatsFormat,
                foodStatsFormat,
                waterStatsFormat,
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
