using System.Collections.Generic;
using UnityEngine;

public interface ICharacterData
{
    string Id { get; set; }
    string CharacterName { get; set; }
    string PrototypeId { get; set; }
    int Level { get; set; }
    int Exp { get; set; }
    int CurrentHp { get; set; }
    int CurrentMp { get; set; }
    int StatPoint { get; set; }
    int SkillPoint { get; set; }
    int Gold { get; set; }
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
    IList<CharacterAttributeLevel> AttributeLevels { get; set; }
    IList<CharacterSkillLevel> SkillLevels { get; set; }
    IList<CharacterBuff> Buffs { get; set; }
    IList<CharacterItem> EquipItems { get; set; }
    IList<CharacterItem> NonEquipItems { get; set; }
}
