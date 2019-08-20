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
            members = new SocialCharacterData[reader.GetPackedUInt()];
            SocialCharacterData tempEntry;
            for (int i = 0; i < members.Length; ++i)
            {
                tempEntry = new SocialCharacterData();
                tempEntry.id = reader.GetString();
                tempEntry.characterName = reader.GetString();
                tempEntry.dataId = reader.GetInt();
                tempEntry.level = reader.GetShort();
                members[i] = tempEntry;
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
            writer.PutPackedUInt((uint)members.Length);
            SocialCharacterData tempEntry;
            for (int i = 0; i < members.Length; ++i)
            {
                tempEntry = members[i];
                writer.Put(tempEntry.id);
                writer.Put(tempEntry.characterName);
                writer.Put(tempEntry.dataId);
                writer.Put(tempEntry.level);
            }
        }
    }
}
