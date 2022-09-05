using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIBaseBank : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Gold Amount}")]
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD);

        [Header("UI Elements")]
        public TextWrapper uiTextAmount;

        private void Update()
        {
            if (uiTextAmount != null)
            {
                uiTextAmount.text = ZString.Format(
                    LanguageManager.GetText(formatKeyAmount),
                    GetAmount().ToString("N0"));
            }
        }

        public void OnClickDeposit()
        {
            UISceneGlobal.Singleton.ShowInputDialog(LanguageManager.GetText(UITextKeys.UI_BANK_DEPOSIT.ToString()), LanguageManager.GetText(UITextKeys.UI_BANK_DEPOSIT_DESCRIPTION.ToString()), OnDepositConfirm, 0, null, 0);
        }

        public void OnClickWithdraw()
        {
            UISceneGlobal.Singleton.ShowInputDialog(LanguageManager.GetText(UITextKeys.UI_BANK_WITHDRAW.ToString()), LanguageManager.GetText(UITextKeys.UI_BANK_WITHDRAW_DESCRIPTION.ToString()), OnWithdrawConfirm, 0, null, 0);
        }

        public abstract void OnDepositConfirm(int amount);
        public abstract void OnWithdrawConfirm(int amount);
        public abstract int GetAmount();
    }
}
