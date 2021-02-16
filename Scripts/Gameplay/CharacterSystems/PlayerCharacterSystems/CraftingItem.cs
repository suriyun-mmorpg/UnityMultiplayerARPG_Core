using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct CraftingItem : INetSerializable
    {
        public int dataId;
        public short amount;
        public float craftRemainsDuration;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(dataId);
            writer.Put(amount);
            writer.Put(craftRemainsDuration);
        }

        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetInt();
            amount = reader.GetShort();
            craftRemainsDuration = reader.GetFloat();
        }
    }

    [System.Serializable]
    public class SyncListCraftingItem : LiteNetLibSyncList<CraftingItem>
    {
    }
}
