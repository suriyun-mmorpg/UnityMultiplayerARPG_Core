using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseCashShopInfoMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            NotAvailable,
            UserNotFound,
            InternalServerError,
        }
        public Error error;
        public int cash;
        public int[] cashShopItemIds;

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            if (error == Error.None)
            {
                cash = reader.GetInt();
                cashShopItemIds = reader.GetArray<int>();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
            if (error == Error.None)
            {
                writer.Put(cash);
                writer.PutArray<int>(cashShopItemIds);
            }
        }
    }
}
