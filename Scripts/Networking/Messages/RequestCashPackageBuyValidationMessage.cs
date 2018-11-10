using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class RequestCashPackageBuyValidationMessage : BaseAckMessage
    {
        public int dataId;
        public RuntimePlatform platform;
        public string receipt;

        public override void DeserializeData(NetDataReader reader)
        {
            dataId = reader.GetInt();
            platform = (RuntimePlatform)reader.GetByte();
            receipt = reader.GetString();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(dataId);
            writer.Put((byte)platform);
            writer.Put(receipt);
        }
    }
}
