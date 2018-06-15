using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerCharacterData : CharacterData, IPlayerCharacterData
{
    public short statPoint;
    public short skillPoint;
    public int gold;
    public string currentMapName;
    public Vector3 currentPosition;
    public string respawnMapName;
    public Vector3 respawnPosition;
    public int lastUpdate;
    public List<CharacterHotkey> hotkeys = new List<CharacterHotkey>();
    public List<CharacterQuest> quests = new List<CharacterQuest>();

    public short StatPoint { get { return statPoint; } set { statPoint = value; } }
    public short SkillPoint { get { return skillPoint; } set { skillPoint = value; } }
    public int Gold { get { return gold; } set { gold = value; } }
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
