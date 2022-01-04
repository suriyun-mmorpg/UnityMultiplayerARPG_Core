using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class CharacterCurrency : INetSerializable
    {
        public static readonly CharacterCurrency Empty = new CharacterCurrency();
        public int dataId;
        public int amount;

        [System.NonSerialized]
        private int dirtyDataId;
        [System.NonSerialized]
        private Currency cacheCurrency;

        private void MakeCache()
        {
            if (dirtyDataId != dataId)
            {
                dirtyDataId = dataId;
                cacheCurrency = null;
                GameInstance.Currencies.TryGetValue(dataId, out cacheCurrency);
            }
        }

        public Currency GetCurrency()
        {
            MakeCache();
            return cacheCurrency;
        }

        public CharacterCurrency Clone()
        {
            return new CharacterCurrency()
            {
                dataId = dataId,
                amount = amount,
            };
        }

        public static CharacterCurrency Create(Currency currency, int amount = 0)
        {
            return Create(currency.DataId, amount);
        }

        public static CharacterCurrency Create(int dataId, int amount = 0)
        {
            return new CharacterCurrency()
            {
                dataId = dataId,
                amount = amount,
            };
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(dataId);
            writer.PutPackedInt(amount);
        }

        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetPackedInt();
            amount = reader.GetPackedInt();
        }
    }

    [System.Serializable]
    public class SyncListCharacterCurrency : LiteNetLibSyncList<CharacterCurrency>
    {
    }
}
