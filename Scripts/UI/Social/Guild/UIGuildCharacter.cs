using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildCharacter : UISocialCharacter
    {
        /// <summary>
        /// Format => {0} = {Role Name}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Role Name}")]
        public string formatGuildRole = "{0}";
        
        [Header("UI Elements")]
        public TextWrapper uiGuildRole;

        public byte GuildRole { get; private set; }

        public void Setup(SocialCharacterEntityTuple data, byte guildRole, GuildRoleData guildRoleData)
        {
            Data = data;
            GuildRole = guildRole;

            if (uiGuildRole != null)
                uiGuildRole.text = string.Format(formatGuildRole, guildRoleData.roleName);
        }
    }
}
