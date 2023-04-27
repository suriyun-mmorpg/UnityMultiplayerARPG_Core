using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterAttribute
    {
        [System.NonSerialized]
        private int _dirtyDataId;
        [System.NonSerialized]
        private Attribute _cacheAttribute;

        private void MakeCache()
        {
            if (_dirtyDataId == dataId)
                return;
            _dirtyDataId = dataId;
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
