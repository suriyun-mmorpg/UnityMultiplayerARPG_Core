using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public struct ResponseGetSocialGroupMembersMessage : INetSerializable
    {
        public SocialCharacterData[] members;

        public void Deserialize(NetDataReader reader)
        {
            members = reader.GetArray<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutArray(members);
        }
    }
}
