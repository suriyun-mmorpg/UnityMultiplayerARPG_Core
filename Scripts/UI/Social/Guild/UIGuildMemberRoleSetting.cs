using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildMemberRoleSetting : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Character Name}")]
        public string formatKeyName = UILocaleKeys.UI_FORMAT_SIMPLE.ToString();
        [Tooltip("Format => {0} = {Level}")]
        public string formatKeyLevel = UILocaleKeys.UI_FORMAT_LEVEL.ToString();

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
            {
                uiTextName.text = string.Format(
                    LanguageManager.GetText(formatKeyName),
                    string.IsNullOrEmpty(member.characterName) ? LanguageManager.GetUnknowTitle() : member.characterName);
            }

            if (uiTextLevel != null)
            {
                uiTextLevel.text = string.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    member.level.ToString("N0"));
            }

            if (dropdownRoles != null)
            {
                List<DropdownWrapper.OptionData> options = new List<DropdownWrapper.OptionData>();
                options.Add(new DropdownWrapper.OptionData(LanguageManager.GetText(UILocaleKeys.UI_LABEL_NONE.ToString())));
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
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_LABEL_WARNING.ToString()), LanguageManager.GetText(UILocaleKeys.UI_INVALID_GUILD_ROLE.ToString()));
                return;
            }
            BasePlayerCharacterController.OwningCharacter.RequestSetGuildMemberRole(characterId, (byte)dropdownRoles.value);
            Hide();
        }
    }
}
