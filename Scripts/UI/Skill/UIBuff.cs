using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIBuff : UISelectionEntry<BuffTuple>
    {
        public Buff Buff { get { return Data.buff; } }
        public short Level { get { return Data.targetLevel; } }

        /// <summary>
        /// Format => {0} = {Duration Label}, {1} = {Duration}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Duration Label}, {1} = {Duration}")]
        public string formatDuration = "{0}: {1}";
        /// <summary>
        /// Format => {0} = {Recovery Hp Label}, {1} = {Recovery Amount}
        /// </summary>
        [Tooltip("Format => {0} = {Recovery Hp Label}, {1} = {Recovery Amount}")]
        public string formatRecoveryHp = "{0}: {1}";
        /// <summary>
        /// Format => {0} = {Recovery Mp Label}, {1} = {Recovery Amount}
        /// </summary>
        [Tooltip("Format => {0} = {Recovery Mp Label}, {1} = {Recovery Amount}")]
        public string formatRecoveryMp = "{0}: {1}";
        /// <summary>
        /// Format => {0} = {Recovery Stamina Label}, {1} = {Recovery Amount}
        /// </summary>
        [Tooltip("Format => {0} = {Recovery Stamina Label}, {1} = {Recovery Amount}")]
        public string formatRecoveryStamina = "{0}: {1}";
        /// <summary>
        /// Format => {0} = {Recovery Food Label}, {1} = {Recovery Amount}
        /// </summary>
        [Tooltip("Format => {0} = {Recovery Food Label}, {1} = {Recovery Amount}")]
        public string formatRecoveryFood = "{0}: {1}";
        /// <summary>
        /// Format => {0} = {Recovery Water Label}, {1} = {Recovery Amount}
        /// </summary>
        [Tooltip("Format => {0} = {Recovery Water Label}, {1} = {Recovery Amount}")]
        public string formatRecoveryWater = "{0}: {1}";

        [Header("UI Elements")]
        public TextWrapper uiTextDuration;
        public TextWrapper uiTextRecoveryHp;
        public TextWrapper uiTextRecoveryMp;
        public TextWrapper uiTextRecoveryStamina;
        public TextWrapper uiTextRecoveryFood;
        public TextWrapper uiTextRecoveryWater;
        public UICharacterStats uiBuffStats;
        public UIAttributeAmounts uiBuffAttributes;
        public UIResistanceAmounts uiBuffResistances;
        public UIDamageElementAmounts uiBuffDamages;

        protected override void UpdateData()
        {
            if (uiTextDuration != null)
            {
                float duration = Buff.GetDuration(Level);
                uiTextDuration.gameObject.SetActive(duration != 0);
                uiTextDuration.text = string.Format(
                    formatDuration,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_BUFF_DURATION.ToString()),
                    duration.ToString("N0"));
            }

            if (uiTextRecoveryHp != null)
            {
                int recoveryHp = Buff.GetRecoveryHp(Level);
                uiTextRecoveryHp.gameObject.SetActive(recoveryHp != 0);
                uiTextRecoveryHp.text = string.Format(
                    formatRecoveryHp,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_HP.ToString()),
                    recoveryHp.ToString("N0"));
            }

            if (uiTextRecoveryMp != null)
            {
                int recoveryMp = Buff.GetRecoveryMp(Level);
                uiTextRecoveryMp.gameObject.SetActive(recoveryMp != 0);
                uiTextRecoveryMp.text = string.Format(
                    formatRecoveryMp,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_MP.ToString()),
                    recoveryMp.ToString("N0"));
            }

            if (uiTextRecoveryStamina != null)
            {
                int recoveryStamina = Buff.GetRecoveryStamina(Level);
                uiTextRecoveryStamina.gameObject.SetActive(recoveryStamina != 0);
                uiTextRecoveryStamina.text = string.Format(
                    formatRecoveryStamina,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_STAMINA.ToString()),
                    recoveryStamina.ToString("N0"));
            }

            if (uiTextRecoveryFood != null)
            {
                int recoveryFood = Buff.GetRecoveryFood(Level);
                uiTextRecoveryFood.gameObject.SetActive(recoveryFood != 0);
                uiTextRecoveryFood.text = string.Format(
                    formatRecoveryFood,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_FOOD.ToString()),
                    recoveryFood.ToString("N0"));
            }

            if (uiTextRecoveryWater != null)
            {
                int recoveryWater = Buff.GetRecoveryWater(Level);
                uiTextRecoveryWater.gameObject.SetActive(recoveryWater != 0);
                uiTextRecoveryWater.text = string.Format(
                    formatRecoveryWater,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_WATER.ToString()),
                    recoveryWater.ToString("N0"));
            }

            if (uiBuffStats != null)
                uiBuffStats.Data = Buff.GetIncreaseStats(Level);

            if (uiBuffAttributes != null)
                uiBuffAttributes.Data = GameDataHelpers.CombineAttributes(Buff.increaseAttributes, new Dictionary<Attribute, short>(), Level, 1f);

            if (uiBuffResistances != null)
                uiBuffResistances.Data = GameDataHelpers.CombineResistances(Buff.increaseResistances, new Dictionary<DamageElement, float>(), Level, 1f);

            if (uiBuffDamages != null)
                uiBuffDamages.Data = GameDataHelpers.CombineDamages(Buff.increaseDamages, new Dictionary<DamageElement, MinMaxFloat>(), Level, 1f);
        }
    }
}
