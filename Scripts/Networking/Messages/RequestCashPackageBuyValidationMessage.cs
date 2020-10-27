using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    public class RequestCashPackageBuyValidationMessage : INetSerializable
    {
        public int dataId;
        public RuntimePlatform platform;
        public string receipt;

        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetInt();
            platform = (RuntimePlatform)reader.GetByte();
            receipt = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(dataId);
            writer.Put((byte)platform);
            writer.Put(receipt);
        }
    }
}
