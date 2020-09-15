using UnityEngine;
using System.Collections;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemDropData : INetSerializable
    {
        public bool putOnPlaceholder;
        public int dataId;
        public short level;
        public short amount;

        public void Deserialize(NetDataReader reader)
        {
            putOnPlaceholder = reader.GetBool();
            dataId = reader.GetPackedInt();
            level = reader.GetPackedShort();
            amount = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(putOnPlaceholder);
            writer.PutPackedInt(dataId);
            writer.PutPackedShort(level);
            writer.PutPackedShort(amount);
        }
    }

    [System.Serializable]
    public class SyncFieldItemDropData : LiteNetLibSyncField<ItemDropData>
    {
    }
}