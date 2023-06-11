using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct UpdateMapInfoMessage : INetSerializable
    {
        public string mapId;
        public string className;

        public void Deserialize(NetDataReader reader)
        {
            mapId = reader.GetString();
            className = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(mapId);
            writer.Put(className);
        }
    }
}
