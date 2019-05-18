using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildCreate : UIBase
    {
        /// <summary>
        /// Format => {0} = {Require Gold Label}, {1} = {Amount}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Require Gold Label}, {1} = {Amount}")]
        public string formatRequireGold = "{0}: {1}";

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
                    formatRequireGold,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_REQUIRE_GOLD.ToString()),
                    systemSetting.CreateGuildRequiredGold.ToString("N0"));
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
