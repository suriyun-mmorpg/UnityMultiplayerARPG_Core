using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseGetOnlineCharacterDataMessage : INetSerializable
    {
        public UITextKeys message;
        public PlayerCharacterData character;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            if (!message.IsError())
                character = reader.Get(() => new PlayerCharacterData());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            if (!message.IsError())
                writer.Put(character);
        }
    }
}
