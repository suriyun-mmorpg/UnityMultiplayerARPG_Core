using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial struct GameMessage : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
        }
    }
}
