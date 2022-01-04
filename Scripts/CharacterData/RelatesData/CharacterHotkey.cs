using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
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
        public string relateId;

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

        public CharacterHotkey Clone()
        {
            return new CharacterHotkey()
            {
                hotkeyId = hotkeyId,
                type = type,
                relateId = relateId,
            };
        }
    }

    [System.Serializable]
    public class SyncListCharacterHotkey : LiteNetLibSyncList<CharacterHotkey>
    {
    }
}
