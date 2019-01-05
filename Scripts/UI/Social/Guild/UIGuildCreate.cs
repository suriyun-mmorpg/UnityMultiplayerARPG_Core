using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildCreate : UIBase
    {
        [Header("Display Format")]
        [Tooltip("Require Gold Format => {0} = {Amount}")]
        public string requireGoldFormat = "Require Gold: {0}";

        [Header("UI Elements")]
        public InputFieldWrapper inputFieldGuildName;
        public TextWrapper textRequireGold;
        public UIItemAmounts uiRequireItems;

        public override void Show()
        {
            SocialSystemSetting systemSetting = GameInstance.Singleton.SocialSystemSetting;
            if (textRequireGold != null)
                textRequireGold.text = string.Format(requireGoldFormat, systemSetting.CreateGuildRequiredGold.ToString("N0"));
            if (uiRequireItems != null)
                uiRequireItems.Data = systemSetting.CreateGuildRequireItems;
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
