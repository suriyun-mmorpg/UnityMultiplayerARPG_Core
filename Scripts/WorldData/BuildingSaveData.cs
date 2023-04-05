using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial class BuildingSaveData : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            this.DeserializeBuildingSaveData(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            this.SerializeBuildingSaveData(writer);
        }
    }
}
