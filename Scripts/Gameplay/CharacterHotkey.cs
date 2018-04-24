using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

public enum HotkeyType : byte
{
    None,
    Skill,
    Item,
}

[System.Serializable]
public struct CharacterHotkey
{
    public string hotkeyId;
    public HotkeyType type;
    public string dataId;
    [System.NonSerialized]
    private HotkeyType dirtyType;
    [System.NonSerialized]
    private string dirtyDataId;
    [System.NonSerialized]
    private Skill cacheSkill;
    [System.NonSerialized]
    private Item cacheItem;

    private void MakeCache()
    {
        if (type == HotkeyType.None || string.IsNullOrEmpty(dataId))
        {
            cacheSkill = null;
            cacheItem = null;
            return;
        }
        if (string.IsNullOrEmpty(dirtyDataId) || !dirtyDataId.Equals(dataId) || type != dirtyType)
        {
            dirtyDataId = dataId;
            dirtyType = type;
            cacheSkill = null;
            cacheItem = null;
            if (type == HotkeyType.Skill)
                cacheSkill = GameInstance.Skills.TryGetValue(dataId, out cacheSkill) ? cacheSkill : null;
            if (type == HotkeyType.Item)
                cacheItem = GameInstance.Items.TryGetValue(dataId, out cacheItem) ? cacheItem : null;
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
        newValue.dataId = reader.GetString();
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

