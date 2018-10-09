using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class UpdateSocialMemberMessage : ILiteNetLibMessage
    {
        public enum UpdateType : byte
        {
            Add,
            Update,
            Remove,
        }
        public UpdateType type;
        public int id;
        public SocialCharacterData data = new SocialCharacterData();

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            id = reader.GetInt();
            switch (type)
            {
                case UpdateType.Add:
                case UpdateType.Update:
                    data.id = reader.GetString();
                    data.characterName = reader.GetString();
                    data.dataId = reader.GetInt();
                    data.level = reader.GetInt();
                    data.isOnline = reader.GetBool();
                    // Read extra data
                    if (data.isOnline)
                    {
                        data.currentHp = reader.GetInt();
                        data.maxHp = reader.GetInt();
                        data.currentMp = reader.GetInt();
                        data.maxMp = reader.GetInt();
                    }
                    break;
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(id);
            switch (type)
            {
                case UpdateType.Add:
                case UpdateType.Update:
                    writer.Put(data.id);
                    writer.Put(data.characterName);
                    writer.Put(data.dataId);
                    writer.Put(data.level);
                    writer.Put(data.isOnline);
                    // Put extra data
                    if (data.isOnline)
                    {
                        writer.Put(data.currentHp);
                        writer.Put(data.maxHp);
                        writer.Put(data.currentMp);
                        writer.Put(data.maxMp);
                    }
                    break;
            }
        }

        public string CharacterId { get { return data.id; } set { data.id = value; } }
    }
}
