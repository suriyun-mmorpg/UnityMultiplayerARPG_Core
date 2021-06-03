using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class CharacterCompanion : INetSerializable
    {
        public static readonly CharacterCompanion Empty = new CharacterCompanion();
        public int dataId;
        public string name;
        public short level;
        public int exp;
        public int nameChangeCount;

        public static CharacterCompanion Create(int dataId, string name = "", short level = 1, int exp = 0)
        {
            return new CharacterCompanion()
            {
                dataId = dataId,
                name = name,
                level = level,
                exp = exp,
                nameChangeCount = 0,
            };
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(dataId);
            if (dataId != 0)
            {
                writer.Put(name);
                writer.PutPackedShort(level);
                writer.PutPackedInt(exp);
                writer.PutPackedInt(nameChangeCount);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetPackedInt();
            if (dataId != 0)
            {
                name = reader.GetString();
                level = reader.GetPackedShort();
                exp = reader.GetPackedInt();
                nameChangeCount = reader.GetPackedInt();
            }
        }
    }

    [System.Serializable]
    public class SyncFieldCharacterCompanion : LiteNetLibSyncField<CharacterCompanion>
    {
    }
}
