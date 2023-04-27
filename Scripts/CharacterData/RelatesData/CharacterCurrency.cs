using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterCurrency
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

        public static CharacterCurrency Create(Currency currency, int amount = 0)
        {
            return Create(currency.DataId, amount);
        }
    }

    [System.Serializable]
    public class SyncListCharacterCurrency : LiteNetLibSyncList<CharacterCurrency>
    {
    }
}
