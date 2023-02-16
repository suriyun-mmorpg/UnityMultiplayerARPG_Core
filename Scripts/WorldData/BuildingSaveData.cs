using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct BuildingSaveData : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            reader.DeserializeBuildingSaveData(ref this);
        }

        public void Serialize(NetDataWriter writer)
        {
            this.SerializeBuildingSaveData(writer);
        }
    }
}
