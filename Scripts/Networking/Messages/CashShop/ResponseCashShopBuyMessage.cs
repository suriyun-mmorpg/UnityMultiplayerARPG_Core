using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseCashShopBuyMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            InternalServerError,
            NotAvailable,
            NotLoggedIn,
            ItemNotFound,
            NotEnoughCash,
            CannotCarryAllRewards,
        }
        public Error error;
        public int dataId;
        public int cash;

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            if (error == Error.None)
            {
                dataId = reader.GetInt();
                cash = reader.GetInt();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
            if (error == Error.None)
            {
                writer.Put(dataId);
                writer.Put(cash);
            }
        }
    }
}
