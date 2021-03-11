using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseCashShopBuyMessage : INetSerializable
    {
        public UITextKeys message;
        public int dataId;
        public int userCash;
        public int userGold;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            if (message == UITextKeys.NONE)
            {
                dataId = reader.GetInt();
                userCash = reader.GetInt();
                userGold = reader.GetInt();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            if (message == UITextKeys.NONE)
            {
                writer.Put(dataId);
                writer.Put(userCash);
                writer.Put(userGold);
            }
        }
    }
}
