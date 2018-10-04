using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIGuildCharacter : UISocialCharacter
    {
        [Header("Display Format")]
        [Tooltip("Guild Role Format => {0} = {Role name}")]
        public string guildRoleFormat = "{0}";
        
        [Header("UI Elements")]
        public TextWrapper uiGuildRole;

        public byte GuildRole { get; private set; }

        public void Setup(SocialCharacterEntityTuple data, byte guildRole, GuildRole guildRoleData)
        {
            Data = data;
            GuildRole = guildRole;

            if (uiGuildRole != null)
                uiGuildRole.text = string.Format(guildRoleFormat, guildRoleData.roleName);
        }
    }
}
