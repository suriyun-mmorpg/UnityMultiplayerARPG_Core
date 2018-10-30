using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UIBuff : UISelectionEntry<BuffTuple>
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
        public TextWrapper uiTextDuration;
        public Text textRecoveryHp;
        public TextWrapper uiTextRecoveryHp;
        public Text textRecoveryMp;
        public TextWrapper uiTextRecoveryMp;
        public Text textRecoveryStamina;
        public TextWrapper uiTextRecoveryStamina;
        public Text textRecoveryFood;
        public TextWrapper uiTextRecoveryFood;
        public Text textRecoveryWater;
        public TextWrapper uiTextRecoveryWater;
        public UICharacterStats uiBuffStats;
        public UIAttributeAmounts uiBuffAttributes;
        public UIResistanceAmounts uiBuffResistances;
        public UIDamageElementAmounts uiBuffDamages;

        protected override void UpdateData()
        {
            MigrateUIComponents();

            var buff = Data.buff;
            var level = Data.targetLevel;

            if (uiTextDuration != null)
            {
                var duration = buff.GetDuration(level);
                uiTextDuration.gameObject.SetActive(duration != 0);
                uiTextDuration.text = string.Format(durationFormat, duration.ToString("N0"));
            }

            if (uiTextRecoveryHp != null)
            {
                var recoveryHp = buff.GetRecoveryHp(level);
                uiTextRecoveryHp.gameObject.SetActive(recoveryHp != 0);
                uiTextRecoveryHp.text = string.Format(recoveryHpFormat, recoveryHp.ToString("N0"));
            }

            if (uiTextRecoveryMp != null)
            {
                var recoveryMp = buff.GetRecoveryMp(level);
                uiTextRecoveryMp.gameObject.SetActive(recoveryMp != 0);
                uiTextRecoveryMp.text = string.Format(recoveryMpFormat, recoveryMp.ToString("N0"));
            }

            if (uiTextRecoveryStamina != null)
            {
                var recoveryStamina = buff.GetRecoveryStamina(level);
                uiTextRecoveryStamina.gameObject.SetActive(recoveryStamina != 0);
                uiTextRecoveryStamina.text = string.Format(recoveryStaminaFormat, recoveryStamina.ToString("N0"));
            }

            if (uiTextRecoveryFood != null)
            {
                var recoveryFood = buff.GetRecoveryFood(level);
                uiTextRecoveryFood.gameObject.SetActive(recoveryFood != 0);
                uiTextRecoveryFood.text = string.Format(recoveryFoodFormat, recoveryFood.ToString("N0"));
            }

            if (uiTextRecoveryWater != null)
            {
                var recoveryWater = buff.GetRecoveryWater(level);
                uiTextRecoveryWater.gameObject.SetActive(recoveryWater != 0);
                uiTextRecoveryWater.text = string.Format(recoveryWaterFormat, recoveryWater.ToString("N0"));
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

        [ContextMenu("Migrate UI Components")]
        public void MigrateUIComponents()
        {
            uiTextDuration = MigrateUIHelpers.SetWrapperToText(textDuration, uiTextDuration);
            uiTextRecoveryHp = MigrateUIHelpers.SetWrapperToText(textRecoveryHp, uiTextRecoveryHp);
            uiTextRecoveryMp = MigrateUIHelpers.SetWrapperToText(textRecoveryMp, uiTextRecoveryMp);
            uiTextRecoveryStamina = MigrateUIHelpers.SetWrapperToText(textRecoveryStamina, uiTextRecoveryStamina);
            uiTextRecoveryFood = MigrateUIHelpers.SetWrapperToText(textRecoveryFood, uiTextRecoveryFood);
            uiTextRecoveryWater = MigrateUIHelpers.SetWrapperToText(textRecoveryWater, uiTextRecoveryWater);
        }
    }
}
