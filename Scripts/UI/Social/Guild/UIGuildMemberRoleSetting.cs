using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIGuildMemberRoleSetting : UIBase
    {
        [Header("Display Format")]
        [Tooltip("Name Format => {0} = {Character name}")]
        public string nameFormat = "{0}";
        [Tooltip("Level Format => {0} = {Level}")]
        public string levelFormat = "Lv: {0}";

        [Header("UI Elements")]
        public TextWrapper uiTextName;
        public TextWrapper uiTextLevel;
        public DropdownWrapper dropdownRoles;

        private string characterId;

        public void Show(GuildRoleData[] roles, SocialCharacterData member, byte guildRole)
        {
            base.Show();

            characterId = member.id;

            if (uiTextName != null)
                uiTextName.text = string.Format(nameFormat, string.IsNullOrEmpty(member.characterName) ? "Unknow" : member.characterName);

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(levelFormat, member.level.ToString("N0"));

            if (dropdownRoles != null)
            {
                var options = new List<DropdownWrapper.OptionData>();
                options.Add(new DropdownWrapper.OptionData("None"));
                for (var i = 1; i < roles.Length; ++i)
                {
                    options.Add(new DropdownWrapper.OptionData(roles[i].roleName));
                }
                dropdownRoles.options = options;
                dropdownRoles.value = guildRole;
            }
        }

        public void OnClickSetting()
        {
            byte role = (byte)(dropdownRoles != null ? dropdownRoles.value : 0);
            if (role == 0)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Warning", "Invalid role");
                return;
            }
            BasePlayerCharacterController.OwningCharacter.RequestSetGuildMemberRole(characterId, (byte)dropdownRoles.value);
            Hide();
        }
    }
}
