using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildCreate : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD);
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGoldNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD_NOT_ENOUGH);

        [Header("UI Elements")]
        public InputFieldWrapper inputFieldGuildName;
        public TextWrapper textRequireGold;
        public UIItemAmounts uiRequireItems;

        protected virtual void OnEnable()
        {
            IPlayerCharacterData owningCharacter = GameInstance.PlayingCharacter;
            SocialSystemSetting systemSetting = GameInstance.Singleton.SocialSystemSetting;
            if (textRequireGold != null)
            {
                int gold = owningCharacter.Gold.Increase(owningCharacter.UserGold);
                textRequireGold.text = string.Format(
                    gold >= systemSetting.CreateGuildRequiredGold ?
                        LanguageManager.GetText(formatKeyRequireGold) :
                        LanguageManager.GetText(formatKeyRequireGoldNotEnough),
                    gold.ToString("N0"),
                    systemSetting.CreateGuildRequiredGold.ToString("N0"));
            }

            if (uiRequireItems != null)
            {
                uiRequireItems.showAsRequirement = true;
                uiRequireItems.Data = systemSetting.CacheCreateGuildRequireItems;
            }
        }

        public void OnClickCreate()
        {
            GameInstance.ClientGuildHandlers.RequestCreateGuild(new RequestCreateGuildMessage()
            {
                guildName = inputFieldGuildName.text,
            }, ClientGuildActions.ResponseCreateGuild);
            Hide();
        }
    }
}
