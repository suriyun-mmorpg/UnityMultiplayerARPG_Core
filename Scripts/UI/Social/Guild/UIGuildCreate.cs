using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildCreate : UIBase
    {
        [Header("Display Format")]
        [Tooltip("Require Gold Format => {0} = {Amount}, {1} = {Require Gold Label}")]
        public string requireGoldFormat = "{1}: {0}";

        [Header("UI Elements")]
        public InputFieldWrapper inputFieldGuildName;
        public TextWrapper textRequireGold;
        public UIItemAmounts uiRequireItems;

        public override void Show()
        {
            SocialSystemSetting systemSetting = GameInstance.Singleton.SocialSystemSetting;
            if (textRequireGold != null)
            {
                textRequireGold.text = string.Format(
                    requireGoldFormat,
                    systemSetting.CreateGuildRequiredGold.ToString("N0"),
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_REQUIRE_GOLD.ToString()));
            }

            if (uiRequireItems != null)
            {
                uiRequireItems.showAsRequirement = true;
                uiRequireItems.Data = systemSetting.CreateGuildRequireItems;
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
