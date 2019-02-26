using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIBaseBank : UIBase
    {
        [Header("Generic Info Format")]
        [Tooltip("Amount Format => {0} = {Amount}")]
        public string amountFormat = "{0}";

        [Header("Input Dialog Settings")]
        public string depositInputTitle = "Deposit";
        public string depositInputDescription = "";
        public string withdrawInputTitle = "Withdraw";
        public string withdrawInputDescription = "";

        [Header("UI Elements")]
        public TextWrapper uiTextAmount;

        private void Update()
        {
            if (uiTextAmount != null)
                uiTextAmount.text = string.Format(amountFormat, GetAmount().ToString("N0"));
        }

        public void OnClickDeposit()
        {
            UISceneGlobal.Singleton.ShowInputDialog(depositInputTitle, depositInputDescription, OnDepositConfirm, 0, null, 0);
        }

        public void OnClickWithdraw()
        {
            UISceneGlobal.Singleton.ShowInputDialog(withdrawInputTitle, withdrawInputDescription, OnWithdrawConfirm, 0, null, 0);
        }

        public abstract void OnDepositConfirm(int amount);
        public abstract void OnWithdrawConfirm(int amount);
        public abstract int GetAmount();
    }
}
