using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public struct ResponseCashPackageBuyValidationMessage : INetSerializable
    {
        public UITextKeys message;
        public List<CashPackageItemInfo> items;
        public string transactionID;
        public int cash;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            if (!message.IsError())
            {
                items = reader.GetList<CashPackageItemInfo>();
                transactionID = reader.GetString();
                cash = reader.GetPackedInt();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            if (!message.IsError())
            {
                writer.PutList(items);
                writer.Put(transactionID);
                writer.PutPackedInt(cash);
            }
        }
    }
}
