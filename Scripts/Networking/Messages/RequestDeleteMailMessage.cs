using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class RequestDeleteMailMessage : INetSerializable
    {
        public string id;

        public void Deserialize(NetDataReader reader)
        {
            id = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
        }
    }
}
