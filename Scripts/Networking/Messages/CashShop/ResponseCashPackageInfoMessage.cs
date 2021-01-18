using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseCashPackageInfoMessage : INetSerializable
    {
        public UITextKeys error;
        public int cash;
        public int[] cashPackageIds;

        public void Deserialize(NetDataReader reader)
        {
            error = (UITextKeys)reader.GetPackedUShort();
            if (error == UITextKeys.NONE)
            {
                cash = reader.GetInt();
                cashPackageIds = reader.GetArray<int>();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)error);
            if (error == UITextKeys.NONE)
            {
                writer.Put(cash);
                writer.PutArray<int>(cashPackageIds);
            }
        }
    }
}
