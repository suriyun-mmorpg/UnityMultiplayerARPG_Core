using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterCurrency : INetSerializable
    {
        [System.NonSerialized]
        private int _dirtyDataId;
        [System.NonSerialized]
        private Currency _cacheCurrency;

        private void MakeCache()
        {
            if (_dirtyDataId == dataId)
                return;
            _dirtyDataId = dataId;
            if (!GameInstance.Currencies.TryGetValue(dataId, out _cacheCurrency))
                _cacheCurrency = null;
        }

        public Currency GetCurrency()
        {
            MakeCache();
            return _cacheCurrency;
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
