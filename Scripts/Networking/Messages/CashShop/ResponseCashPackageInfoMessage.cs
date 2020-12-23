using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseCashPackageInfoMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            InternalServerError,
            NotAvailable,
            NotLoggedIn,
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
                cashPackageIds = reader.GetArray<int>();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
            if (error == Error.None)
            {
                writer.Put(cash);
                writer.PutArray<int>(cashPackageIds);
            }
        }
    }
}
