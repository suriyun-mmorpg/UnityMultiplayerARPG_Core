using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestIncreaseCharacterAttributeAmountMessage : INetSerializable
    {
        public int dataId;

        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(dataId);
        }
    }
}
