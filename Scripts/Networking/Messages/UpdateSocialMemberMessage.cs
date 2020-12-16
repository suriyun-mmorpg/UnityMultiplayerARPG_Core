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
            id = reader.GetPackedInt();
            if (type != UpdateType.Clear)
            {
                // Get social member data
                data.id = reader.GetString();
                switch (type)
                {
                    case UpdateType.Add:
                    case UpdateType.Update:
                        data.characterName = reader.GetString();
                        data.dataId = reader.GetPackedInt();
                        data.level = reader.GetPackedShort();
                        isOnline = reader.GetBool();
                        // Read extra data
                        if (isOnline)
                        {
                            data.currentHp = reader.GetPackedInt();
                            data.maxHp = reader.GetPackedInt();
                            data.currentMp = reader.GetPackedInt();
                            data.maxMp = reader.GetPackedInt();
                        }
                        break;
                }
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.PutPackedInt(id);
            if (type != UpdateType.Clear)
            {
                // Put social member data
                writer.Put(data.id);
                switch (type)
                {
                    case UpdateType.Add:
                    case UpdateType.Update:
                        writer.Put(data.characterName);
                        writer.PutPackedInt(data.dataId);
                        writer.PutPackedShort(data.level);
                        writer.Put(isOnline);
                        // Put extra data
                        if (isOnline)
                        {
                            writer.PutPackedInt(data.currentHp);
                            writer.PutPackedInt(data.maxHp);
                            writer.PutPackedInt(data.currentMp);
                            writer.PutPackedInt(data.maxMp);
                        }
                        break;
                }
            }
        }
    }
}
