using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial class StorageCharacterItem : INetSerializable
    {
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)storageType);
            writer.Put(storageOwnerId);
            writer.Put(characterItem);
        }

        public void Deserialize(NetDataReader reader)
        {
            storageType = (StorageType)reader.GetByte();
            storageOwnerId = reader.GetString();
            characterItem = reader.Get<CharacterItem>();
        }
    }
}
