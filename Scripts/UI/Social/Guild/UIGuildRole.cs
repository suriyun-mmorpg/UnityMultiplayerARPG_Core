using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildRole : UISelectionEntry<GuildRoleData>
    {
        [Header("Display Format")]
        [Tooltip("Role Name Format => {0} = {Role name}")]
        public string roleNameFormat = "{0}";
        [Tooltip("Share Exp Percentage Format => {0} = {Share exp percentage}, {1} = {Share exp Label}")]
        public string shareExpPercentageFormat = "{1}: {0}%";

        [Header("UI Elements")]
        public TextWrapper textRoleName;
        public TextWrapper textCanInvite;
        public TextWrapper textCanKick;
        public TextWrapper textShareExpPercentage;

        protected override void UpdateData()
        {
            if (textRoleName != null)
                textRoleName.text = string.Format(roleNameFormat, Data.roleName);

            if (textCanInvite != null)
            {
                textCanInvite.text = Data.canInvite ?
                    LanguageManager.GetText(UILocaleKeys.UI_GUILD_ROLE_CAN_INVITE.ToString()) :
                    LanguageManager.GetText(UILocaleKeys.UI_GUILD_ROLE_CANNOT_INVITE.ToString());
            }

            if (textCanKick != null)
            {
                textCanKick.text = Data.canKick ?
                    LanguageManager.GetText(UILocaleKeys.UI_GUILD_ROLE_CAN_KICK.ToString()) :
                    LanguageManager.GetText(UILocaleKeys.UI_GUILD_ROLE_CANNOT_KICK.ToString());
            }

            if (textShareExpPercentage != null)
            {
                textShareExpPercentage.text = string.Format(
                    shareExpPercentageFormat,
                    Data.shareExpPercentage.ToString("N0"),
                     LanguageManager.GetText(UILocaleKeys.UI_GUILD_ROLE_SHARE_EXP.ToString()));
            }
        }
    }
}
