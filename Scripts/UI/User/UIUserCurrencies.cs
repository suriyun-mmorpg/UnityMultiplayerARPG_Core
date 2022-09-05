using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIUserCurrencies : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Gold Amount}")]
        public UILocaleKeySetting formatKeyUserGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD);
        [Tooltip("Format => {0} = {Gold Amount}")]
        public UILocaleKeySetting formatKeyTotalGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD);
        [Tooltip("Format => {0} = {Cash Amount}")]
        public UILocaleKeySetting formatKeyUserCash = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CASH);

        [Header("UI Elements")]
        public TextWrapper uiTextUserGold;
        public TextWrapper uiTextTotalGold;
        public TextWrapper uiTextUserCash;

        private void Update()
        {
            int amount;
            if (uiTextUserGold != null)
            {
                amount = GameInstance.PlayingCharacter == null ? 0 : GameInstance.PlayingCharacter.UserGold;
                uiTextUserGold.text = ZString.Format(
                    LanguageManager.GetText(formatKeyUserGold),
                    amount.ToString("N0"));
            }

            if (uiTextTotalGold != null)
            {
                amount = GameInstance.PlayingCharacter == null ? 0 : (GameInstance.PlayingCharacter.UserGold + GameInstance.PlayingCharacter.Gold);
                uiTextTotalGold.text = ZString.Format(
                    LanguageManager.GetText(formatKeyTotalGold),
                    amount.ToString("N0"));
            }

            if (uiTextUserCash != null)
            {
                amount = GameInstance.PlayingCharacter == null ? 0 : GameInstance.PlayingCharacter.UserCash;
                uiTextUserCash.text = ZString.Format(
                    LanguageManager.GetText(formatKeyUserCash),
                    amount.ToString("N0"));
            }
        }
    }
}
