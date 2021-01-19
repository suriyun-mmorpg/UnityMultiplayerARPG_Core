using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseCashPackageInfoMessage : INetSerializable
    {
        public UITextKeys message;
        public int cash;
        public int[] cashPackageIds;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            if (message == UITextKeys.NONE)
            {
                cash = reader.GetInt();
                cashPackageIds = reader.GetArray<int>();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            if (message == UITextKeys.NONE)
            {
                writer.Put(cash);
                writer.PutArray<int>(cashPackageIds);
            }
        }
    }
}
