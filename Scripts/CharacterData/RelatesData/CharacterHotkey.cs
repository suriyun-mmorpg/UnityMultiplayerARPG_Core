using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

public enum HotkeyType : byte
{
    None,
    Skill,
    Item,
    SystemKey,
}

[System.Serializable]
public class CharacterHotkey : INetSerializable
{
    public static readonly CharacterHotkey Empty = new CharacterHotkey();
    public string hotkeyId;
    public HotkeyType type;
    public string id;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(hotkeyId);
        writer.Put((byte)type);
        writer.Put(id);
    }

    public void Deserialize(NetDataReader reader)
    {
        hotkeyId = reader.GetString();
        type = (HotkeyType)reader.GetByte();
        id = reader.GetString();
    }
}

[System.Serializable]
public class SyncListCharacterHotkey : LiteNetLibSyncList<CharacterHotkey>
{
}
