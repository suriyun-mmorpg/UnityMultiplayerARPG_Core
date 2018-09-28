using System.Collections.Generic;
using UnityEngine;

public partial interface IPlayerCharacterData : ICharacterData
{
    short StatPoint { get; set; }
    short SkillPoint { get; set; }
    int Gold { get; set; }
    int PartyId { get; set; }
    int GuildId { get; set; }
    int GuildRole { get; set; }
    /// <summary>
    /// Current Map Name will be work with MMORPG system only
    /// For Lan game it will be scene name which set in game instance
    /// </summary>
    string CurrentMapName { get; set; }
    Vector3 CurrentPosition { get; set; }
    /// <summary>
    /// Respawn Map Name will be work with MMORPG system only
    /// For Lan game it will be scene name which set in game instance
    /// </summary>
    string RespawnMapName { get; set; }
    Vector3 RespawnPosition { get; set; }
    int LastUpdate { get; set; }
    IList<CharacterHotkey> Hotkeys { get; set; }
    IList<CharacterQuest> Quests { get; set; }
}
