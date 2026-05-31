using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public struct RequestCashPackageBuyValidationMessage : INetSerializable
    {
        public List<CashPackageItemInfo> items;
        public RuntimePlatform platform;
        public string transactionID;
        public string receipt;
        public string appleJwsRepresentation;

        public void Deserialize(NetDataReader reader)
        {
            items = reader.GetList<CashPackageItemInfo>();
            platform = (RuntimePlatform)reader.GetByte();
            transactionID = reader.GetString();
            receipt = reader.GetString();
            appleJwsRepresentation = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(items);
            writer.Put((byte)platform);
            writer.Put(transactionID);
            writer.Put(receipt);
            writer.Put(appleJwsRepresentation);
        }
    }
}
