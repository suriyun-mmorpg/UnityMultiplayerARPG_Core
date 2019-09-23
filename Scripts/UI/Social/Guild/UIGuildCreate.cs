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

        public override void Show()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            SocialSystemSetting systemSetting = GameInstance.Singleton.SocialSystemSetting;
            if (textRequireGold != null)
            {
                int currentAmount = 0;
                if (owningCharacter != null)
                    currentAmount = owningCharacter.Gold;
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

            base.Show();
        }

        public void OnClickCreate()
        {
            BasePlayerCharacterController.OwningCharacter.RequestCreateGuild(
                inputFieldGuildName != null ? inputFieldGuildName.text : string.Empty);
            Hide();
        }
    }
}
