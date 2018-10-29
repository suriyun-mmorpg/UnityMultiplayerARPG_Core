using System.Collections;
using System.Collections.Generic;
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
public class CharacterHotkey
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
}

public class NetFieldCharacterHotkey : LiteNetLibNetField<CharacterHotkey>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterHotkey();
        newValue.hotkeyId = reader.GetString();
        newValue.type = (HotkeyType)reader.GetByte();
        newValue.dataId = reader.GetInt();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.hotkeyId);
        writer.Put((byte)Value.type);
        writer.Put(Value.dataId);
    }

    public override bool IsValueChanged(CharacterHotkey newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterHotkey : LiteNetLibSyncList<NetFieldCharacterHotkey, CharacterHotkey>
{
}
