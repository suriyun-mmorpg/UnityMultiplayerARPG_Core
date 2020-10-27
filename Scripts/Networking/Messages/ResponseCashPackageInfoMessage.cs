using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponseCashPackageInfoMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            NotAvailable,
            UserNotFound,
        }
        public Error error;
        public int cash;
        public int[] cashPackageIds;

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            if (error == Error.None)
            {
                cash = reader.GetInt();
                int size = reader.GetInt();
                cashPackageIds = new int[size];
                for (int i = 0; i < size; ++i)
                {
                    cashPackageIds[i] = reader.GetInt();
                }
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
            if (error == Error.None)
            {
                writer.Put(cash);
                writer.Put(cashPackageIds.Length);
                foreach (int cashShopItemId in cashPackageIds)
                {
                    writer.Put(cashShopItemId);
                }
            }
        }
    }
}
