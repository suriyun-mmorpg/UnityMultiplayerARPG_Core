using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UIBuff : UISelectionEntry<BuffLevelTuple>
    {
        [Tooltip("Duration Format => {0} = {Duration}")]
        public string durationFormat = "Duration: {0}";
        [Tooltip("Recovery Hp Format => {0} = {Recovery amount}")]
        public string recoveryHpFormat = "Recovery Hp: {0}";
        [Tooltip("Recovery Mp Format => {0} = {Recovery amount}")]
        public string recoveryMpFormat = "Recovery Mp: {0}";
        [Tooltip("Recovery Stamina Format => {0} = {Recovery amount}")]
        public string recoveryStaminaFormat = "Recovery Stamina: {0}";
        [Tooltip("Recovery Food Format => {0} = {Recovery amount}")]
        public string recoveryFoodFormat = "Recovery Food: {0}";
        [Tooltip("Recovery Water Format => {0} = {Recovery amount}")]
        public string recoveryWaterFormat = "Recovery Water: {0}";

        [Header("UI Elements")]
        public Text textDuration;
        public Text textRecoveryHp;
        public Text textRecoveryMp;
        public Text textRecoveryStamina;
        public Text textRecoveryFood;
        public Text textRecoveryWater;
        public UICharacterStats uiBuffStats;
        public UIAttributeAmounts uiBuffAttributes;
        public UIResistanceAmounts uiBuffResistances;
        public UIDamageElementAmounts uiBuffDamages;

        protected override void UpdateData()
        {
            var buff = Data.buff;
            var level = Data.targetLevel;

            if (textDuration != null)
            {
                var duration = buff.GetDuration(level);
                textDuration.gameObject.SetActive(duration != 0);
                textDuration.text = string.Format(durationFormat, duration.ToString("N0"));
            }

            if (textRecoveryHp != null)
            {
                var recoveryHp = buff.GetRecoveryHp(level);
                textRecoveryHp.gameObject.SetActive(recoveryHp != 0);
                textRecoveryHp.text = string.Format(recoveryHpFormat, recoveryHp.ToString("N0"));
            }

            if (textRecoveryMp != null)
            {
                var recoveryMp = buff.GetRecoveryMp(level);
                textRecoveryMp.gameObject.SetActive(recoveryMp != 0);
                textRecoveryMp.text = string.Format(recoveryMpFormat, recoveryMp.ToString("N0"));
            }

            if (textRecoveryStamina != null)
            {
                var recoveryStamina = buff.GetRecoveryStamina(level);
                textRecoveryStamina.gameObject.SetActive(recoveryStamina != 0);
                textRecoveryStamina.text = string.Format(recoveryStaminaFormat, recoveryStamina.ToString("N0"));
            }

            if (textRecoveryFood != null)
            {
                var recoveryFood = buff.GetRecoveryFood(level);
                textRecoveryFood.gameObject.SetActive(recoveryFood != 0);
                textRecoveryFood.text = string.Format(recoveryFoodFormat, recoveryFood.ToString("N0"));
            }

            if (textRecoveryWater != null)
            {
                var recoveryWater = buff.GetRecoveryWater(level);
                textRecoveryWater.gameObject.SetActive(recoveryWater != 0);
                textRecoveryWater.text = string.Format(recoveryWaterFormat, recoveryWater.ToString("N0"));
            }

            if (uiBuffStats != null)
                uiBuffStats.Data = buff.GetIncreaseStats(level);

            if (uiBuffAttributes != null)
                uiBuffAttributes.Data = GameDataHelpers.MakeAttributeAmountsDictionary(buff.increaseAttributes, new Dictionary<Attribute, short>(), level, 1f);

            if (uiBuffResistances != null)
                uiBuffResistances.Data = GameDataHelpers.MakeResistanceAmountsDictionary(buff.increaseResistances, new Dictionary<DamageElement, float>(), level, 1f);

            if (uiBuffDamages != null)
                uiBuffDamages.Data = GameDataHelpers.MakeDamageAmountsDictionary(buff.increaseDamages, new Dictionary<DamageElement, MinMaxFloat>(), level, 1f);
        }
    }
}
