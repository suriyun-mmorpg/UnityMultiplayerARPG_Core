using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseGetGuildRequestsMessage : INetSerializable
    {
        public UITextKeys message;
        public SocialCharacterData[] guildRequests;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            guildRequests = reader.GetArray<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.PutArray(guildRequests);
        }
    }
}
