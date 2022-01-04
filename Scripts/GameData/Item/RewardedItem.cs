using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct RewardedItem : INetSerializable
    {
        public BaseItem item;
        public short level;
        public short amount;
        public short randomSeed;

        public void SetItemByDataId(int dataId)
        {
            item = GameInstance.Items.ContainsKey(dataId) ? GameInstance.Items[dataId] : null;
        }

        public void Deserialize(NetDataReader reader)
        {
            SetItemByDataId(reader.GetPackedInt());
            level = reader.GetPackedShort();
            amount = reader.GetPackedShort();
            randomSeed = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(item != null ? item.DataId : 0);
            writer.PutPackedShort(level);
            writer.PutPackedShort(amount);
            writer.PutPackedShort(randomSeed);
        }
    }
}
