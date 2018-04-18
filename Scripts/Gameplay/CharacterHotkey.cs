using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

public enum HotkeyTypes : byte
{
    None,
    Skill,
    Item,
}

[System.Serializable]
public struct CharacterHotkey
{
    public string hotkeyId;
    public HotkeyTypes type;
    public string dataId;
    [System.NonSerialized]
    private HotkeyTypes dirtyType;
    [System.NonSerialized]
    private string dirtyDataId;
    [System.NonSerialized]
    private Skill cacheSkill;
    [System.NonSerialized]
    private Item cacheItem;

    private void MakeCache()
    {
        if (type == HotkeyTypes.None || string.IsNullOrEmpty(dataId))
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
            if (type == HotkeyTypes.Skill)
                cacheSkill = GameInstance.Skills.TryGetValue(dataId, out cacheSkill) ? cacheSkill : null;
            if (type == HotkeyTypes.Item)
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
        newValue.type = (HotkeyTypes)reader.GetByte();
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
    public int IndexOf(string hotkeyId)
    {
        CharacterHotkey tempHotkey;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempHotkey = list[i];
            if (!string.IsNullOrEmpty(tempHotkey.hotkeyId) &&
                tempHotkey.hotkeyId.Equals(hotkeyId))
            {
                index = i;
                break;
            }
        }
        return index;
    }
}

