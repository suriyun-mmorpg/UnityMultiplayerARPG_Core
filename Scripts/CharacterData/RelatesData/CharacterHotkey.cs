using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

public enum HotkeyType : byte
{
    None,
    Skill,
    Item,
}

[System.Serializable]
public class CharacterHotkey : INetSerializable
{
    public static readonly CharacterHotkey Empty = new CharacterHotkey();
    public string hotkeyId;
    public HotkeyType type;
    public int dataId;
    [System.NonSerialized]
    private HotkeyType dirtyType;
    [System.NonSerialized]
    private int dirtyDataId;
    [System.NonSerialized]
    private Skill cacheSkill;
    [System.NonSerialized]
    private Item cacheItem;

    private void MakeCache()
    {
        if (type == HotkeyType.None)
        {
            cacheSkill = null;
            cacheItem = null;
            return;
        }
        if (dirtyDataId != dataId || type != dirtyType)
        {
            dirtyDataId = dataId;
            dirtyType = type;
            cacheSkill = null;
            cacheItem = null;
            if (type == HotkeyType.Skill)
                GameInstance.Skills.TryGetValue(dataId, out cacheSkill);
            if (type == HotkeyType.Item)
                GameInstance.Items.TryGetValue(dataId, out cacheItem);
        }
    }

    public Skill GetSkill()
    {
        MakeCache();
        return cacheSkill;
    }

    public Item GetItem()
    {
        MakeCache();
        return cacheItem;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(hotkeyId);
        writer.Put((byte)type);
        writer.Put(dataId);
    }

    public void Deserialize(NetDataReader reader)
    {
        hotkeyId = reader.GetString();
        type = (HotkeyType)reader.GetByte();
        dataId = reader.GetInt();
    }
}

[System.Serializable]
public class SyncListCharacterHotkey : LiteNetLibSyncList<CharacterHotkey>
{
}
