using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIGuildRole : UISelectionEntry<GuildRoleData>
    {
        [Header("Display Format")]
        [Tooltip("Role Name Format => {0} = {Role name}")]
        public string roleNameFormat = "{0}";
        [Tooltip("Share Exp Percentage Format => {0} = {Share exp percentage}")]
        public string shareExpPercentageFormat = "Share Exp: {0}%";
        public string messageCanInvite = "Can Invite";
        public string messageCannotInvite = "Cannot Invite";
        public string messageCanKick = "Can Kick";
        public string messageCannotKick = "Cannot Kick";

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
                textCanInvite.text = Data.canInvite ? messageCanInvite : messageCannotInvite;

            if (textCanKick != null)
                textCanKick.text = Data.canKick ? messageCanKick : messageCannotKick;

            if (textShareExpPercentage != null)
                textShareExpPercentage.text = string.Format(shareExpPercentageFormat, Data.shareExpPercentage.ToString("N0"));
        }
    }
}
