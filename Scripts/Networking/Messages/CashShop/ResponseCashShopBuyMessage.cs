using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseCashShopBuyMessage : INetSerializable
    {
        public UITextKeys message;
        public int dataId;
        public int cash;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            if (message == UITextKeys.NONE)
            {
                dataId = reader.GetInt();
                cash = reader.GetInt();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            if (message == UITextKeys.NONE)
            {
                writer.Put(dataId);
                writer.Put(cash);
            }
        }
    }
}
