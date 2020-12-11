using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public struct ResponseGetStorageItemsMessage : INetSerializable
    {
        public enum Error : byte
        {
            None,
            NotAvailable,
            NotAllowed,
            CharacterNotFound,
            InternalServerError,
        }
        public Error error;
        public List<CharacterItem> storageItems;

        public void Deserialize(NetDataReader reader)
        {
            error = (Error)reader.GetByte();
            storageItems = reader.GetList<CharacterItem>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)error);
            writer.PutList(storageItems);
        }
    }
}
