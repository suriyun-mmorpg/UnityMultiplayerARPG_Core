using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildRole : UISelectionEntry<GuildRoleData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Role Name}")]
        public UILocaleKeySetting formatKeyRoleName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Share Exp Percentage}")]
        public UILocaleKeySetting formatKeyShareExpPercentage = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SHARE_EXP_PERCENTAGE);

        [Header("UI Elements")]
        public TextWrapper textRoleName;
        public TextWrapper textCanInvite;
        public TextWrapper textCanKick;
        public TextWrapper textShareExpPercentage;

        protected override void UpdateData()
        {
            if (textRoleName != null)
            {
                textRoleName.text = string.Format(
                    LanguageManager.GetText(formatKeyRoleName), Data.roleName);
            }

            if (textCanInvite != null)
            {
                textCanInvite.text = Data.canInvite ?
                    LanguageManager.GetText(UITextKeys.UI_GUILD_ROLE_CAN_INVITE.ToString()) :
                    LanguageManager.GetText(UITextKeys.UI_GUILD_ROLE_CANNOT_INVITE.ToString());
            }

            if (textCanKick != null)
            {
                textCanKick.text = Data.canKick ?
                    LanguageManager.GetText(UITextKeys.UI_GUILD_ROLE_CAN_KICK.ToString()) :
                    LanguageManager.GetText(UITextKeys.UI_GUILD_ROLE_CANNOT_KICK.ToString());
            }

            if (textShareExpPercentage != null)
            {
                textShareExpPercentage.text = string.Format(
                    LanguageManager.GetText(formatKeyShareExpPercentage),
                    Data.shareExpPercentage.ToString("N0"));
            }
        }
    }
}
