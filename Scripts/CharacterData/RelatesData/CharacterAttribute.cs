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

        public static CharacterAttribute Create(Attribute attribute, short amount = 1)
        {
            return Create(attribute.DataId, amount);
        }

        public static CharacterAttribute Create(int dataId, short amount = 1)
        {
            CharacterAttribute newAttribute = new CharacterAttribute();
            newAttribute.dataId = dataId;
            newAttribute.amount = amount;
            return newAttribute;
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
