using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterHotkey : INetSerializable
    {
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
