using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseWithdrawUserGoldMessage : INetSerializable
    {
        public UITextKeys error;

        public void Deserialize(NetDataReader reader)
        {
            error = (UITextKeys)reader.GetPackedUShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)error);
        }
    }
}
