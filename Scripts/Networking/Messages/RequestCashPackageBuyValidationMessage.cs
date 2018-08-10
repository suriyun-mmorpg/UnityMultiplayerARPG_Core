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
        // TODO: Add validation data

        public override void DeserializeData(NetDataReader reader)
        {
            dataId = reader.GetInt();
            platform = (RuntimePlatform)reader.GetByte();
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(dataId);
            writer.Put((byte)platform);
        }
    }
}
