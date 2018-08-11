using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponseCashPackageInfoMessage : BaseAckMessage
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

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            cash = reader.GetInt();
            var size = reader.GetInt();
            cashPackageIds = new int[size];
            for (var i = 0; i < size; ++i)
            {
                cashPackageIds[i] = reader.GetInt();
            }
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.Put(cash);
            writer.Put(cashPackageIds.Length);
            foreach (var cashShopItemId in cashPackageIds)
            {
                writer.Put(cashShopItemId);
            }
        }
    }
}
