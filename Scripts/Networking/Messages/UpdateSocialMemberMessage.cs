using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct UpdateSocialMemberMessage : INetSerializable
    {
        public enum UpdateType : byte
        {
            Add,
            Update,
            Remove,
            Clear,
        }
        public UpdateType type;
        public int id;
        public bool isOnline;
        public SocialCharacterData data;

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            id = reader.GetInt();
            if (type != UpdateType.Clear)
            {
                // Get social member data
                data.id = reader.GetString();
                switch (type)
                {
                    case UpdateType.Add:
                    case UpdateType.Update:
                        data.characterName = reader.GetString();
                        data.dataId = reader.GetInt();
                        data.level = reader.GetShort();
                        isOnline = reader.GetBool();
                        // Read extra data
                        if (isOnline)
                        {
                            data.currentHp = reader.GetInt();
                            data.maxHp = reader.GetInt();
                            data.currentMp = reader.GetInt();
                            data.maxMp = reader.GetInt();
                        }
                        break;
                }
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(id);
            if (type != UpdateType.Clear)
            {
                // Put social member data
                writer.Put(data.id);
                switch (type)
                {
                    case UpdateType.Add:
                    case UpdateType.Update:
                        writer.Put(data.characterName);
                        writer.Put(data.dataId);
                        writer.Put(data.level);
                        writer.Put(isOnline);
                        // Put extra data
                        if (isOnline)
                        {
                            writer.Put(data.currentHp);
                            writer.Put(data.maxHp);
                            writer.Put(data.currentMp);
                            writer.Put(data.maxMp);
                        }
                        break;
                }
            }
        }
    }
}
