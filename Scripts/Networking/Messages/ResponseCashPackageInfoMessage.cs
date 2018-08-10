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
        public int[] cashPackageIds;

        public override void DeserializeData(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
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
            writer.Put(cashPackageIds.Length);
            foreach (var cashShopItemId in cashPackageIds)
            {
                writer.Put(cashShopItemId);
            }
        }
    }
}
