using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestCashShopBuyMessage : INetSerializable
    {
        public int dataId;
        public CashShopItemCurrencyType currencyType;
        public int amount;

        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetInt();
            currencyType = (CashShopItemCurrencyType)reader.GetByte();
            amount = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(dataId);
            writer.Put((byte)currencyType);
            writer.Put(amount);
        }
    }
}
