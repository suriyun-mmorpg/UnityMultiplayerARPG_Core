using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        [Tooltip("Weight Limit Stats Format => {0} = {Weight Limit}")]
        public string weightLimitStatsFormat = "Weight Limit: {0}";
        [Tooltip("Stamina Stats Format => {0} = {Amount}")]
        public string staminaStatsFormat = "Hp: {0}";
        [Tooltip("Food Stats Format => {0} = {Amount}")]
        public string foodStatsFormat = "Mp: {0}";
        [Tooltip("Water Stats Format => {0} = {Amount}")]
        public string waterStatsFormat = "Mp: {0}";

        [Header("UI Elements")]
        public Text textStats;
        public Text textHp;
        public Text textMp;
        public Text textArmor;
        public Text textAccuracy;
        public Text textEvasion;
        public Text textCriRate;
        public Text textCriDmgRate;
        public Text textBlockRate;
        public Text textBlockDmgRate;
        public Text textWeightLimit;
        public Text textStamina;
        public Text textFood;
        public Text textWater;

        protected override void UpdateData()
        {
            var statsString = "";
            var statsStringPart = "";

            // Hp
            statsStringPart = string.Format(hpStatsFormat, Data.hp.ToString("N0"));
            if (Data.hp != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (textHp != null)
                textHp.text = statsStringPart;

            // Mp
            statsStringPart = string.Format(mpStatsFormat, Data.mp.ToString("N0"));
            if (Data.mp != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (textMp != null)
                textMp.text = statsStringPart;

            // Armor
            statsStringPart = string.Format(armorStatsFormat, Data.armor.ToString("N0"));
            if (Data.armor != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (textArmor != null)
                textArmor.text = statsStringPart;

            // Accuracy
            statsStringPart = string.Format(accuracyStatsFormat, Data.accuracy.ToString("N0"));
            if (Data.accuracy != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (textAccuracy != null)
                textAccuracy.text = statsStringPart;

            // Evasion
            statsStringPart = string.Format(evasionStatsFormat, Data.evasion.ToString("N0"));
            if (Data.evasion != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (textEvasion != null)
                textEvasion.text = statsStringPart;

            // Cri Rate
            statsStringPart = string.Format(criRateStatsFormat, (Data.criRate * 100).ToString("N2"));
            if (Data.criRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (textCriRate != null)
                textCriRate.text = statsStringPart;

            // Cri Dmg Rate
            statsStringPart = string.Format(criDmgRateStatsFormat, (Data.criDmgRate * 100).ToString("N2"));
            if (Data.criDmgRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (textCriDmgRate != null)
                textCriDmgRate.text = statsStringPart;

            // Block Rate
            statsStringPart = string.Format(blockRateStatsFormat, (Data.blockRate * 100).ToString("N2"));
            if (Data.blockRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (textBlockRate != null)
                textBlockRate.text = statsStringPart;

            // Block Dmg Rate
            statsStringPart = string.Format(blockDmgRateStatsFormat, (Data.blockDmgRate * 100).ToString("N2"));
            if (Data.blockDmgRate != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (textBlockDmgRate != null)
                textBlockDmgRate.text = statsStringPart;

            // Weight
            statsStringPart = string.Format(weightLimitStatsFormat, Data.weightLimit.ToString("N2"));
            if (Data.weightLimit != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (textWeightLimit != null)
                textWeightLimit.text = statsStringPart;

            // Stamina
            statsStringPart = string.Format(staminaStatsFormat, Data.stamina.ToString("N0"));
            if (Data.stamina != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (textStamina != null)
                textStamina.text = statsStringPart;

            // Food
            statsStringPart = string.Format(foodStatsFormat, Data.food.ToString("N0"));
            if (Data.food != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (textFood != null)
                textFood.text = statsStringPart;

            // Water
            statsStringPart = string.Format(waterStatsFormat, Data.water.ToString("N0"));
            if (Data.water != 0)
            {
                if (!string.IsNullOrEmpty(statsString))
                    statsString += "\n";
                statsString += statsStringPart;
            }
            if (textWater != null)
                textWater.text = statsStringPart;

            // All stats text
            if (textStats != null)
            {
                textStats.gameObject.SetActive(!string.IsNullOrEmpty(statsString));
                textStats.text = statsString;
            }
        }
    }
}
