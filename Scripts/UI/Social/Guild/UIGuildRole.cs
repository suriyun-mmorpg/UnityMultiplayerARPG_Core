using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildRole : UISelectionEntry<GuildRoleData>
    {
        /// <summary>
        /// Format => {0} = {Role Name}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Role Name}")]
        public string formatRoleName = "{0}";
        /// <summary>
        /// Format => {0} = {Share Exp Label}, {1} = {Share Exp Percentage}
        /// </summary>
        [Tooltip("Format => {0} = {Share Exp Label}, {1} = {Share Exp Percentage}")]
        public string formatShareExpPercentage = "{0}: {1}%";

        [Header("UI Elements")]
        public TextWrapper textRoleName;
        public TextWrapper textCanInvite;
        public TextWrapper textCanKick;
        public TextWrapper textShareExpPercentage;

        protected override void UpdateData()
        {
            if (textRoleName != null)
                textRoleName.text = string.Format(formatRoleName, Data.roleName);

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
                    formatShareExpPercentage,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_SHARE_EXP.ToString()),
                    Data.shareExpPercentage.ToString("N0"));
            }
        }
    }
}
