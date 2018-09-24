using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        }
        public Error error;
        public int cash;

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            cash = reader.GetInt();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.Put(cash);
        }
    }
}
