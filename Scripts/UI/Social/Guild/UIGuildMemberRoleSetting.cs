using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildMemberRoleSetting : UIBase
    {
        [Header("Display Format")]
        [Tooltip("Name Format => {0} = {Character name}")]
        public string nameFormat = "{0}";
        [Tooltip("Level Format => {0} = {Level}, {1} = {Level Label}")]
        public string levelFormat = "{1}: {0}";

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
                uiTextName.text = string.Format(nameFormat, string.IsNullOrEmpty(member.characterName) ? LanguageManager.GetUnknowTitle() : member.characterName);

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(levelFormat, member.level.ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_LEVEL.ToString()));

            if (dropdownRoles != null)
            {
                List<DropdownWrapper.OptionData> options = new List<DropdownWrapper.OptionData>();
                options.Add(new DropdownWrapper.OptionData(LanguageManager.GetText(UILocaleKeys.UI_NONE.ToString())));
                for (int i = 1; i < roles.Length; ++i)
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
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_WARNING.ToString()), LanguageManager.GetText(UILocaleKeys.UI_INVALID_GUILD_ROLE.ToString()));
                return;
            }
            BasePlayerCharacterController.OwningCharacter.RequestSetGuildMemberRole(characterId, (byte)dropdownRoles.value);
            Hide();
        }
    }
}
