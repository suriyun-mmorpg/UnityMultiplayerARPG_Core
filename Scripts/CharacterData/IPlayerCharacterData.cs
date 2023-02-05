using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial interface IPlayerCharacterData : ICharacterData
    {
        /// <summary>
        /// User Account ID
        /// </summary>
        string UserId { get; set; }
        /// <summary>
        /// Current Faction
        /// </summary>
        int FactionId { get; set; }
        /// <summary>
        /// Stat point which uses for increase attribute amount
        /// </summary>
        float StatPoint { get; set; }
        /// <summary>
        /// Skill point which uses for increase skill level
        /// </summary>
        float SkillPoint { get; set; }
        /// <summary>
        /// Gold which uses for buy things
        /// </summary>
        int Gold { get; set; }
        /// <summary>
        /// Gold which store in the bank
        /// </summary>
        int UserGold { get; set; }
        /// <summary>
        /// Cash which uses for buy special items
        /// </summary>
        int UserCash { get; set; }
        /// <summary>
        /// Joined party id
        /// </summary>
        int PartyId { get; set; }
        /// <summary>
        /// Joined guild id
        /// </summary>
        int GuildId { get; set; }
        /// <summary>
        /// Current guild role
        /// </summary>
        byte GuildRole { get; set; }
        /// <summary>
        /// Shared exp to guild
        /// </summary>
        int SharedGuildExp { get; set; }
        /// <summary>
        /// Current Map Name will be work with MMORPG system only
        /// For Lan game it will be scene name which set in game instance
        /// </summary>
        string CurrentMapName { get; set; }
        Vector3 CurrentPosition { get; set; }
        Vector3 CurrentRotation { get; set; }
        /// <summary>
        /// Respawn Map Name will be work with MMORPG system only
        /// For Lan game it will be scene name which set in game instance
        /// </summary>
        string RespawnMapName { get; set; }
        Vector3 RespawnPosition { get; set; }
        int MountDataId { get; set; }
        long LastDeadTime { get; set; }
        long UnmuteTime { get; set; }
        long LastUpdate { get; set; }
        IList<CharacterHotkey> Hotkeys { get; set; }
        IList<CharacterQuest> Quests { get; set; }
        IList<CharacterCurrency> Currencies { get; set; }
    }
}
