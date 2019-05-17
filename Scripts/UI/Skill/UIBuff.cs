using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIBuff : UISelectionEntry<BuffTuple>
    {
        public Buff Buff { get { return Data.buff; } }
        public short Level { get { return Data.targetLevel; } }

        [Tooltip("Duration Format => {0} = {Duration}, {1} = {Duration Label}")]
        public string durationFormat = "{1}: {0}";
        [Tooltip("Recovery Hp Format => {0} = {Recovery amount}, {1} = {Recovery Hp Label}")]
        public string recoveryHpFormat = "{1}: {0}";
        [Tooltip("Recovery Mp Format => {0} = {Recovery amount}, {1} = {Recovery Mp Label}")]
        public string recoveryMpFormat = "{1}: {0}";
        [Tooltip("Recovery Stamina Format => {0} = {Recovery amount}, {1} = {Recovery Stamina Label}")]
        public string recoveryStaminaFormat = "{1}: {0}";
        [Tooltip("Recovery Food Format => {0} = {Recovery amount}, {1} = {Recovery Food Label}")]
        public string recoveryFoodFormat = "{1}: {0}";
        [Tooltip("Recovery Water Format => {0} = {Recovery amount}, {1} = {Recovery Water Label}")]
        public string recoveryWaterFormat = "{1}: {0}";

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
                    durationFormat,
                    duration.ToString("N0"),
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_BUFF_DURATION.ToString()));
            }

            if (uiTextRecoveryHp != null)
            {
                int recoveryHp = Buff.GetRecoveryHp(Level);
                uiTextRecoveryHp.gameObject.SetActive(recoveryHp != 0);
                uiTextRecoveryHp.text = string.Format(
                    recoveryHpFormat,
                    recoveryHp.ToString("N0"),
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_HP.ToString()));
            }

            if (uiTextRecoveryMp != null)
            {
                int recoveryMp = Buff.GetRecoveryMp(Level);
                uiTextRecoveryMp.gameObject.SetActive(recoveryMp != 0);
                uiTextRecoveryMp.text = string.Format(
                    recoveryMpFormat,
                    recoveryMp.ToString("N0"),
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_MP.ToString()));
            }

            if (uiTextRecoveryStamina != null)
            {
                int recoveryStamina = Buff.GetRecoveryStamina(Level);
                uiTextRecoveryStamina.gameObject.SetActive(recoveryStamina != 0);
                uiTextRecoveryStamina.text = string.Format(
                    recoveryStaminaFormat,
                    recoveryStamina.ToString("N0"),
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_STAMINA.ToString()));
            }

            if (uiTextRecoveryFood != null)
            {
                int recoveryFood = Buff.GetRecoveryFood(Level);
                uiTextRecoveryFood.gameObject.SetActive(recoveryFood != 0);
                uiTextRecoveryFood.text = string.Format(
                    recoveryFoodFormat,
                    recoveryFood.ToString("N0"),
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_FOOD.ToString()));
            }

            if (uiTextRecoveryWater != null)
            {
                int recoveryWater = Buff.GetRecoveryWater(Level);
                uiTextRecoveryWater.gameObject.SetActive(recoveryWater != 0);
                uiTextRecoveryWater.text = string.Format(
                    recoveryWaterFormat,
                    recoveryWater.ToString("N0"),
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_WATER.ToString()));
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
