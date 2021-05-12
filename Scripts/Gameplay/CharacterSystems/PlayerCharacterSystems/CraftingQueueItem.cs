using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct CraftingQueueItem : INetSerializable
    {
        public uint crafterId;
        public int dataId;
        public short amount;
        public float craftRemainsDuration;

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUInt(crafterId);
            writer.PutPackedInt(dataId);
            writer.PutPackedShort(amount);
            writer.Put(craftRemainsDuration);
        }

        public void Deserialize(NetDataReader reader)
        {
            crafterId = reader.GetPackedUInt();
            dataId = reader.GetPackedInt();
            amount = reader.GetPackedShort();
            craftRemainsDuration = reader.GetFloat();
        }
    }

    [System.Serializable]
    public class SyncListCraftingQueueItem : LiteNetLibSyncList<CraftingQueueItem>
    {
    }
}
