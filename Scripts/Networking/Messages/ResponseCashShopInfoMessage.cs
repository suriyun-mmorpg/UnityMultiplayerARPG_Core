using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponseCashShopInfoMessage : BaseAckMessage
    {
        public enum Error : byte
        {
            None,
            NotAvailable,
            UserNotFound,
        }
        public Error error;
        public int cash;
        public int[] cashShopItemIds;

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            if (error == Error.None)
            {
                cash = reader.GetInt();
                int size = reader.GetInt();
                cashShopItemIds = new int[size];
                for (int i = 0; i < size; ++i)
                {
                    cashShopItemIds[i] = reader.GetInt();
                }
            }
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)error);
            if (error == Error.None)
            {
                writer.Put(cash);
                writer.Put(cashShopItemIds.Length);
                foreach (int cashShopItemId in cashShopItemIds)
                {
                    writer.Put(cashShopItemId);
                }
            }
        }
    }
}
