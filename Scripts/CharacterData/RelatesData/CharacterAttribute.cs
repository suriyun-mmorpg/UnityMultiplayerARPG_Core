using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial struct CharacterAttribute
    {
        [System.NonSerialized]
        private int _dirtyDataId;

        [System.NonSerialized]
        private Attribute _cacheAttribute;
        /*
        ~CharacterAttribute()
        {
            ClearCachedData();
        }
        */
        private void ClearCachedData()
        {
            _cacheAttribute = null;
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
            if (!GameInstance.Attributes.TryGetValue(dataId, out _cacheAttribute))
                _cacheAttribute = null;
        }

        public Attribute GetAttribute()
        {
            MakeCache();
            return _cacheAttribute;
        }

        public static CharacterAttribute Create(Attribute attribute, int amount = 0)
        {
            return Create(attribute.DataId, amount);
        }
    }

    [System.Serializable]
    public class SyncListCharacterAttribute : LiteNetLibSyncList<CharacterAttribute>
    {
    }
}
