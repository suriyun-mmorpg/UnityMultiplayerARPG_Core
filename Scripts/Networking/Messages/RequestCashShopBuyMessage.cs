using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class RequestCashShopBuyMessage : BaseAckMessage
    {
        public int dataId;

        public override void DeserializeData(NetDataReader reader)
        {
            dataId = reader.GetInt();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(dataId);
        }
    }
}
