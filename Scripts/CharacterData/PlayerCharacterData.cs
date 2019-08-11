using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public partial class PlayerCharacterData : CharacterData, IPlayerCharacterData
{
    private int factionId;
    private short statPoint;
    private short skillPoint;
    private int gold;
    private int userGold;
    private int userCash;
    private int partyId;
    private int guildId;
    private byte guildRole;
    private int sharedGuildExp;
    private string currentMapName;
    private Vector3 currentPosition;
    private string respawnMapName;
    private Vector3 respawnPosition;
    private int lastUpdate;
    private List<CharacterHotkey> hotkeys = new List<CharacterHotkey>();
    private List<CharacterQuest> quests = new List<CharacterQuest>();

    public int FactionId { get { return factionId; } set { factionId = value; } }
    public short StatPoint { get { return statPoint; } set { statPoint = value; } }
    public short SkillPoint { get { return skillPoint; } set { skillPoint = value; } }
    public int Gold { get { return gold; } set { gold = value; } }
    public int UserGold { get { return userGold; } set { userGold = value; } }
    public int UserCash { get { return userCash; } set { userCash = value; } }
    public int PartyId { get { return partyId; } set { partyId = value; } }
    public int GuildId { get { return guildId; } set { guildId = value; } }
    public byte GuildRole { get { return guildRole; } set { guildRole = value; } }
    public int SharedGuildExp { get { return sharedGuildExp; } set { sharedGuildExp = value; } }
    public string CurrentMapName { get { return currentMapName; } set { currentMapName = value; } }
    public Vector3 CurrentPosition { get { return currentPosition; } set { currentPosition = value; } }
    public string RespawnMapName { get { return respawnMapName; } set { respawnMapName = value; } }
    public Vector3 RespawnPosition { get { return respawnPosition; } set { respawnPosition = value; } }
    public int LastUpdate { get { return lastUpdate; } set { lastUpdate = value; } }

    public IList<CharacterHotkey> Hotkeys
    {
        get { return hotkeys; }
        set
        {
            hotkeys = new List<CharacterHotkey>();
            hotkeys.AddRange(value);
        }
    }

    public IList<CharacterQuest> Quests
    {
        get { return quests; }
        set
        {
            quests = new List<CharacterQuest>();
            quests.AddRange(value);
        }
    }
}

public class PlayerCharacterDataLastUpdateComparer : IComparer<PlayerCharacterData>
{
    private int sortMultiplier = 1;
    public PlayerCharacterDataLastUpdateComparer Asc()
    {
        sortMultiplier = 1;
        return this;
    }

    public PlayerCharacterDataLastUpdateComparer Desc()
    {
        sortMultiplier = -1;
        return this;
    }

    public int Compare(PlayerCharacterData x, PlayerCharacterData y)
    {
        return x.LastUpdate.CompareTo(y.LastUpdate) * sortMultiplier;
    }
}
