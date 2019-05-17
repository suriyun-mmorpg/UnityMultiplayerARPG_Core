using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIBaseBank : UIBase
    {
        [Header("Generic Info Format")]
        [Tooltip("Amount Format => {0} = {Amount}, {1} = {Gold Label}")]
        public string amountFormat = "{1}: {0}";

        [Header("UI Elements")]
        public TextWrapper uiTextAmount;

        private void Update()
        {
            if (uiTextAmount != null)
                uiTextAmount.text = string.Format(amountFormat, GetAmount().ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_LABEL_GOLD.ToString()));
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
