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
        if (string.IsNullOrEmpty(dirtyDataId) || !dirtyDataId.Equals(dataId))
        {
            dirtyDataId = dataId;
            if (cacheSkill == null)
                cacheSkill = GameInstance.Skills.TryGetValue(dataId, out cacheSkill) ? cacheSkill : null;
            if (cacheItem == null)
                cacheItem = GameInstance.Items.TryGetValue(dataId, out cacheItem) ? cacheItem : null;
        }
    }
}

public class NetFieldCharacterHotkey : LiteNetLibNetField<CharacterHotkey>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterHotkey();
        newValue.type = (HotkeyTypes)reader.GetByte();
        newValue.dataId = reader.GetString();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put((byte)Value.type);
        writer.Put(Value.dataId);
    }

    public override bool IsValueChanged(CharacterHotkey newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterHotkey : LiteNetLibSyncList<NetFieldCharacterHotkey, CharacterHotkey> { }

