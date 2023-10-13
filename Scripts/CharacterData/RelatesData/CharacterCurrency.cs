using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterCurrency
    {
        [System.NonSerialized]
        private int _dirtyDataId;

        [System.NonSerialized]
        private Currency _cacheCurrency;

        ~CharacterCurrency()
        {
            ClearCachedData();
        }

        private void ClearCachedData()
        {
            _cacheCurrency = null;
        }

        private bool IsRecaching()
        {
            return _dirtyDataId != dataId;
        }

        private void MakeAsCached()
        {
            _dirtyDataId = dataId;
        }

        private void MakeCache()
        {
            if (!IsRecaching())
                return;
            MakeAsCached();
            ClearCachedData();
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
