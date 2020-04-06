using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct UpdateSocialMembersMessage : INetSerializable
    {
        public int id;
        public SocialCharacterData[] members;

        public void Deserialize(NetDataReader reader)
        {
            id = reader.GetInt();
            members = reader.GetArray<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
            writer.Put(members);
        }
    }
}
