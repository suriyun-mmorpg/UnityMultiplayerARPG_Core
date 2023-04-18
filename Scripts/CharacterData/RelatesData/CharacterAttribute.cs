using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterAttribute : INetSerializable
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

        public CharacterAttribute Clone()
        {
            return new CharacterAttribute()
            {
                dataId = dataId,
                amount = amount,
            };
        }

        public static CharacterAttribute Create(Attribute attribute, int amount = 0)
        {
            return Create(attribute.DataId, amount);
        }

        public static CharacterAttribute Create(int dataId, int amount = 0)
        {
            return new CharacterAttribute()
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
    public class SyncListCharacterAttribute : LiteNetLibSyncList<CharacterAttribute>
    {
    }
}
