using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIGuildRole : UISelectionEntry<GuildRole>
    {
        [Header("Display Format")]
        [Tooltip("Role Name Format => {0} = {Role name}")]
        public string roleNameFormat = "{0}";
        [Tooltip("Share Exp Percentage Format => {0} = {Share exp percentage}")]
        public string shareExpPercentageFormat = "{0}";

        [Header("UI Elements")]
        public TextWrapper textRoleName;
        public Toggle toggleCanInvite;
        public Toggle toggleCanKick;
        public TextWrapper textShareExpPercentage;

        protected override void UpdateData()
        {
            if (textRoleName != null)
                textRoleName.text = string.Format(roleNameFormat, Data.roleName);

            if (toggleCanInvite != null)
            {
                toggleCanInvite.interactable = false;
                toggleCanInvite.isOn = Data.canInvite;
            }

            if (toggleCanKick != null)
            {
                toggleCanKick.interactable = false;
                toggleCanKick.isOn = Data.canKick;
            }

            if (textShareExpPercentage != null)
                textShareExpPercentage.text = string.Format(shareExpPercentageFormat, Data.shareExpPercentage.ToString("N0"));
        }
    }
}
