using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemAmount : INetSerializable
    {
        public BaseItem item;
        public short amount;

        public void SetItemByDataId(int dataId)
        {
            item = GameInstance.Items.ContainsKey(dataId) ? GameInstance.Items[dataId] : null;
        }

        public void Deserialize(NetDataReader reader)
        {
            SetItemByDataId(reader.GetPackedInt());
            amount = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(item != null ? item.DataId : 0);
            writer.PutPackedShort(amount);
        }
    }
}
