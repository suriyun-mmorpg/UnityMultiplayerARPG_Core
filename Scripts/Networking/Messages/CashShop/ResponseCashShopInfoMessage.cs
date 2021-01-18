using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseCashShopInfoMessage : INetSerializable
    {
        public UITextKeys error;
        public int cash;
        public int[] cashShopItemIds;

        public void Deserialize(NetDataReader reader)
        {
            error = (UITextKeys)reader.GetPackedUShort();
            if (error == UITextKeys.NONE)
            {
                cash = reader.GetInt();
                cashShopItemIds = reader.GetArray<int>();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)error);
            if (error == UITextKeys.NONE)
            {
                writer.Put(cash);
                writer.PutArray<int>(cashShopItemIds);
            }
        }
    }
}
