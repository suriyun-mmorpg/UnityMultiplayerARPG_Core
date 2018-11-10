using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponseCashPackageBuyValidationMessage : BaseAckMessage
    {
        public enum Error : byte
        {
            None,
            NotAvailable,
            UserNotFound,
            PackageNotFound,
            Invalid,
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
