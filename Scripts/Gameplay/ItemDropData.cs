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
        public int level;
        public int amount;

        public void Deserialize(NetDataReader reader)
        {
            putOnPlaceholder = reader.GetBool();
            dataId = reader.GetPackedInt();
            level = reader.GetPackedInt();
            amount = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(putOnPlaceholder);
            writer.PutPackedInt(dataId);
            writer.PutPackedInt(level);
            writer.PutPackedInt(amount);
        }
    }

    [System.Serializable]
    public class SyncFieldItemDropData : LiteNetLibSyncField<ItemDropData>
    {
    }
}