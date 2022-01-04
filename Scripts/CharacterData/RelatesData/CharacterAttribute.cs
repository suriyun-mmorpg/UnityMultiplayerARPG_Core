using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class CharacterAttribute : INetSerializable
    {
        public static readonly CharacterAttribute Empty = new CharacterAttribute();
        public int dataId;
        public short amount;

        [System.NonSerialized]
        private int dirtyDataId;
        [System.NonSerialized]
        private Attribute cacheAttribute;

        private void MakeCache()
        {
            if (dirtyDataId != dataId)
            {
                dirtyDataId = dataId;
                cacheAttribute = null;
                GameInstance.Attributes.TryGetValue(dataId, out cacheAttribute);
            }
        }

        public Attribute GetAttribute()
        {
            MakeCache();
            return cacheAttribute;
        }

        public CharacterAttribute Clone()
        {
            return new CharacterAttribute()
            {
                dataId = dataId,
                amount = amount,
            };
        }

        public static CharacterAttribute Create(Attribute attribute, short amount = 0)
        {
            return Create(attribute.DataId, amount);
        }

        public static CharacterAttribute Create(int dataId, short amount = 0)
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
            writer.PutPackedShort(amount);
        }

        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetPackedInt();
            amount = reader.GetPackedShort();
        }
    }

    [System.Serializable]
    public class SyncListCharacterAttribute : LiteNetLibSyncList<CharacterAttribute>
    {
    }
}
