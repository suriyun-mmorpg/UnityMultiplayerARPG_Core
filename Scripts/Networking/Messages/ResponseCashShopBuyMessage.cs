using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponseCashShopBuyMessage : BaseAckMessage
    {
        public enum Error : byte
        {
            None,
            NotAvailable,
            UserNotFound,
            ItemNotFound,
            NotEnoughCash,
            CannotCarryAllRewards,
        }
        public Error error;
        public int dataId;
        public int cash;

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            if (error == Error.None)
            {
                dataId = reader.GetInt();
                cash = reader.GetInt();
            }
        }

        public override void SerializeData(NetDataWriter writer)
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
