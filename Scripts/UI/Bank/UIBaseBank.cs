using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIBaseBank : UIBase
    {
        /// <summary>
        /// Format => {0} = {Gold Label}, {1} = {Amount}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Gold Label}, {1} = {Amount}")]
        public string formatAmount = "{0}: {1}";

        [Header("UI Elements")]
        public TextWrapper uiTextAmount;

        private void Update()
        {
            if (uiTextAmount != null)
            {
                uiTextAmount.text = string.Format(formatAmount,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_GOLD.ToString()),
                    GetAmount().ToString("N0"));
            }
        }

        public void OnClickDeposit()
        {
            UISceneGlobal.Singleton.ShowInputDialog(LanguageManager.GetText(UILocaleKeys.UI_BANK_DEPOSIT.ToString()), LanguageManager.GetText(UILocaleKeys.UI_BANK_DEPOSIT_DESCRIPTION.ToString()), OnDepositConfirm, 0, null, 0);
        }

        public void OnClickWithdraw()
        {
            UISceneGlobal.Singleton.ShowInputDialog(LanguageManager.GetText(UILocaleKeys.UI_BANK_WITHDRAW.ToString()), LanguageManager.GetText(UILocaleKeys.UI_BANK_WITHDRAW_DESCRIPTION.ToString()), OnWithdrawConfirm, 0, null, 0);
        }

        public abstract void OnDepositConfirm(int amount);
        public abstract void OnWithdrawConfirm(int amount);
        public abstract int GetAmount();
    }
}
