using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterStats : UISelectionEntry<CharacterStats>
    {
        [Header("Format")]
        [Tooltip("Hp Stats Format => {0} = {Amount}")]
        public string hpStatsFormat = "Hp: {0}";
        [Tooltip("Mp Stats Format => {0} = {Amount}")]
        public string mpStatsFormat = "Mp: {0}";
        [Tooltip("Armor Stats Format => {0} = {Amount}")]
        public string armorStatsFormat = "Armor: {0}";
        [Tooltip("Accuracy Stats Format => {0} = {Amount}")]
        public string accuracyStatsFormat = "Acc: {0}";
        [Tooltip("Evasion Format => {0} = {Amount}")]
        public string evasionStatsFormat = "Eva: {0}";
        [Tooltip("Cri Rate Stats Format => {0} = {Amount}")]
        public string criRateStatsFormat = "Critical: {0}%";
        [Tooltip("Cri Dmg Rate Stats Format => {0} = {Amount}")]
        public string criDmgRateStatsFormat = "Cri Dmg: {0}%";
        [Tooltip("Block Rate Stats Format => {0} = {Amount}")]
        public string blockRateStatsFormat = "Block: {0}%";
        [Tooltip("Block Dmg Rate Stats Format => {0} = {Amount}")]
        public string blockDmgRateStatsFormat = "Block Dmg: {0}%";
        [Tooltip("Move Speed Stats Format => {0} = {Move Speed}")]
        public string moveSpeedStatsFormat = "Move Speed: {0}";
        [Tooltip("Attack Speed Stats Format => {0} = {Attack Speed}")]
        public string atkSpeedStatsFormat = "Attack Speed: {0}";
        [Tooltip("Weight Limit Stats Format => {0} = {Weight Limit}")]
        public string weightLimitStatsFormat = "Weight Limit: {0}";
        [Tooltip("Stamina Stats Format => {0} = {Amount}")]
        public string staminaStatsFormat = "Stamina: {0}";
        [Tooltip("Food Stats Format => {0} = {Amount}")]
        public string foodStatsFormat = "Food: {0}";
        [Tooltip("Water Stats Format => {0} = {Amount}")]
        public string waterStatsFormat = "Water: {0}";

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
