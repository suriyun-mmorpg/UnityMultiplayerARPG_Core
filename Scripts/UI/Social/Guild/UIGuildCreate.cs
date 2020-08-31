using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildCreate : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD);

        [Header("UI Elements")]
        public InputFieldWrapper inputFieldGuildName;
        public TextWrapper textRequireGold;
        public UIItemAmounts uiRequireItems;

        protected virtual void OnEnable()
        {
            SocialSystemSetting systemSetting = GameInstance.Singleton.SocialSystemSetting;
            if (textRequireGold != null)
            {
                int currentAmount = 0;
                if (BasePlayerCharacterController.OwningCharacter)
                    currentAmount = BasePlayerCharacterController.OwningCharacter.Gold;
                textRequireGold.text = string.Format(
                    LanguageManager.GetText(formatKeyRequireGold),
                    currentAmount,
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
            BasePlayerCharacterController.OwningCharacter.RequestCreateGuild(
                inputFieldGuildName != null ? inputFieldGuildName.text : string.Empty);
            Hide();
        }
    }
}
