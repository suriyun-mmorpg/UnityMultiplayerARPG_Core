using LiteNetLib.Utils;
using LiteNetLibManager;

public enum HotkeyType : byte
{
    None,
    Skill,
    Item,
}

[System.Serializable]
public class CharacterHotkey : INetSerializableWithElement
{
    public static readonly CharacterHotkey Empty = new CharacterHotkey();
    public string hotkeyId;
    public HotkeyType type;
    public string relateId;

    [System.NonSerialized]
    private LiteNetLibElement element;
    public LiteNetLibElement Element
    {
        get { return element; }
        set { element = value; }
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(hotkeyId);
        writer.Put((byte)type);
        writer.Put(relateId);
    }

    public void Deserialize(NetDataReader reader)
    {
        hotkeyId = reader.GetString();
        type = (HotkeyType)reader.GetByte();
        relateId = reader.GetString();
    }
}

[System.Serializable]
public class SyncListCharacterHotkey : LiteNetLibSyncList<CharacterHotkey>
{
}
