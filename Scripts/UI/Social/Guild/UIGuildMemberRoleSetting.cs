using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildMemberRoleSetting : UIBase
    {
        /// <summary>
        /// Format => {0} = {Character Name}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Character Name}")]
        public string formatName = "{0}";
        /// <summary>
        /// Format => {0} = {Level Label}, {1} = {Level}
        /// </summary>
        [Tooltip("Format => {0} = {Level Label}, {1} = {Level}")]
        public string formatLevel = "{0}: {1}";

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
                uiTextName.text = string.Format(formatName, string.IsNullOrEmpty(member.characterName) ? LanguageManager.GetUnknowTitle() : member.characterName);

            if (uiTextLevel != null)
            {
                uiTextLevel.text = string.Format(
                    formatLevel,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_LEVEL.ToString()),
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
