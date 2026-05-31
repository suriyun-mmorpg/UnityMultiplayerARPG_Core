using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct CashPackageItemInfo : INetSerializable
    {
        public int dataId;
        public int quantity;

        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetPackedInt();
            quantity = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(dataId);
            writer.PutPackedInt(quantity);
        }
    }
}
