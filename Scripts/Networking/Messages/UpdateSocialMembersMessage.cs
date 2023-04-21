using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial struct UpdateSocialMembersMessage : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            id = reader.GetPackedInt();
            members = reader.GetList<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(id);
            writer.PutList(members);
        }
    }
}
