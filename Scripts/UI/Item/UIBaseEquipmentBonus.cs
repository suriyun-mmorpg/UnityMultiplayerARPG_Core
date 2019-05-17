using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIBaseEquipmentBonus<T> : UISelectionEntry<T>
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
        public string evasionStatsFormat = "{1}: {0}, {1} = {Label}";
        [Tooltip("Cri Rate Stats Format => {0} = {Amount}, {1} = {Label}")]
        public string criRateStatsFormat = "{1}: {0}%, {1} = {Label}";
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
