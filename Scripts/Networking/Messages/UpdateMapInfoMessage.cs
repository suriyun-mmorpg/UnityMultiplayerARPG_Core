using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct UpdateMapInfoMessage : INetSerializable
    {
        public string mapId;

        public void Deserialize(NetDataReader reader)
        {
            mapId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(mapId);
        }
    }
}
