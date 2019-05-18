using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIBuff : UISelectionEntry<BuffTuple>
    {
        public Buff Buff { get { return Data.buff; } }
        public short Level { get { return Data.targetLevel; } }
        
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Duration}")]
        public string formatKeyDuration = UILocaleKeys.UI_FORMAT_BUFF_DURATION.ToString();
        [Tooltip("Format => {0} = {Recovery Amount}")]
        public string formatKeyRecoveryHp = UILocaleKeys.UI_FORMAT_BUFF_RECOVERY_HP.ToString();
        [Tooltip("Format => {0} = {Recovery Amount}")]
        public string formatKeyRecoveryMp = UILocaleKeys.UI_FORMAT_BUFF_RECOVERY_MP.ToString();
        [Tooltip("Format => {0} = {Recovery Amount}")]
        public string formatKeyRecoveryStamina = UILocaleKeys.UI_FORMAT_BUFF_RECOVERY_STAMINA.ToString();
        [Tooltip("Format => {0} = {Recovery Amount}")]
        public string formatKeyRecoveryFood = UILocaleKeys.UI_FORMAT_BUFF_RECOVERY_FOOD.ToString();
        [Tooltip("Format => {0} = {Recovery Amount}")]
        public string formatKeyRecoveryWater = UILocaleKeys.UI_FORMAT_BUFF_RECOVERY_WATER.ToString();

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
                    LanguageManager.GetText(formatKeyDuration),
                    duration.ToString("N0"));
            }

            if (uiTextRecoveryHp != null)
            {
                int recoveryHp = Buff.GetRecoveryHp(Level);
                uiTextRecoveryHp.gameObject.SetActive(recoveryHp != 0);
                uiTextRecoveryHp.text = string.Format(
                    LanguageManager.GetText(formatKeyRecoveryHp),
                    recoveryHp.ToString("N0"));
            }

            if (uiTextRecoveryMp != null)
            {
                int recoveryMp = Buff.GetRecoveryMp(Level);
                uiTextRecoveryMp.gameObject.SetActive(recoveryMp != 0);
                uiTextRecoveryMp.text = string.Format(
                    LanguageManager.GetText(formatKeyRecoveryMp),
                    recoveryMp.ToString("N0"));
            }

            if (uiTextRecoveryStamina != null)
            {
                int recoveryStamina = Buff.GetRecoveryStamina(Level);
                uiTextRecoveryStamina.gameObject.SetActive(recoveryStamina != 0);
                uiTextRecoveryStamina.text = string.Format(
                    LanguageManager.GetText(formatKeyRecoveryStamina),
                    recoveryStamina.ToString("N0"));
            }

            if (uiTextRecoveryFood != null)
            {
                int recoveryFood = Buff.GetRecoveryFood(Level);
                uiTextRecoveryFood.gameObject.SetActive(recoveryFood != 0);
                uiTextRecoveryFood.text = string.Format(
                    LanguageManager.GetText(formatKeyRecoveryFood),
                    recoveryFood.ToString("N0"));
            }

            if (uiTextRecoveryWater != null)
            {
                int recoveryWater = Buff.GetRecoveryWater(Level);
                uiTextRecoveryWater.gameObject.SetActive(recoveryWater != 0);
                uiTextRecoveryWater.text = string.Format(
                    LanguageManager.GetText(formatKeyRecoveryWater),
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
