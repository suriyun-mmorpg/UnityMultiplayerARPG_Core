using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIBaseEquipmentBonus<T> : UISelectionEntry<T>
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
        [Tooltip("Attribute Amount Format => {0} = {Attribute title}, {1} = {Amount}")]
        public string attributeAmountFormat = "{0}: {1}";
        [Tooltip("Resistance Amount Format => {0} = {Resistance title}, {1} = {Amount}")]
        public string resistanceAmountFormat = "{0}: {1}";
        [Tooltip("Damage Amount Format => {0} = {Element title}, {1} = {Min damage}, {2} = {Max damage}")]
        public string damageAmountFormat = "{0}: {1}~{2}";
        [Tooltip("Skill Level Format => {0} = {Skill title}, {1} = {Level}")]
        public string skillLevelFormat = "{0}: {1}";

        [Header("UI Elements")]
        public TextWrapper uiTextAllBonus;

        public string GetEquipmentBonusText(EquipmentBonus equipmentBonus)
        {
            string result = CharacterStats.GetText(
                equipmentBonus.stats,
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
                waterStatsFormat);
            foreach (AttributeAmount entry in equipmentBonus.attributes)
            {
                if (entry.attribute == null || entry.amount == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(attributeAmountFormat, entry.attribute.Title, entry.amount.ToString("N0"));
            }
            foreach (ResistanceAmount entry in equipmentBonus.resistances)
            {
                if (entry.damageElement == null || entry.amount == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(resistanceAmountFormat, entry.damageElement.Title, (entry.amount * 100f).ToString("N0"));
            }
            foreach (DamageAmount entry in equipmentBonus.damages)
            {
                if (entry.damageElement == null || (entry.amount.min == 0 && entry.amount.max == 0))
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(damageAmountFormat, entry.damageElement.Title, entry.amount.min.ToString("N0"), entry.amount.max.ToString("N0"));
            }
            foreach (SkillLevel entry in equipmentBonus.skills)
            {
                if (entry.skill == null || entry.level == 0)
                    continue;
                if (!string.IsNullOrEmpty(result))
                    result += "\n";
                result += string.Format(skillLevelFormat, entry.skill.Title, entry.level.ToString("N0"));
            }
            return result;
        }
    }
}
