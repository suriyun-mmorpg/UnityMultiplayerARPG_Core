using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public struct ResponseCashPackageInfoMessage : INetSerializable
    {
        public UITextKeys message;
        public int cash;
        public List<int> cashPackageIds;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            if (message == UITextKeys.NONE)
            {
                cash = reader.GetPackedInt();
                cashPackageIds = reader.GetList<int>();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            if (message == UITextKeys.NONE)
            {
                writer.PutPackedInt(cash);
                writer.PutList(cashPackageIds);
            }
        }
    }
}
