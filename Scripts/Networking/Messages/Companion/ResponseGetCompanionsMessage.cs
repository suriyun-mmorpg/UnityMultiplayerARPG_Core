using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseGetCompanionsMessage : INetSerializable
    {
        public CharacterCompanion[] companions;

        public void Deserialize(NetDataReader reader)
        {
            companions = reader.GetArray<CharacterCompanion>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutArray(companions);
        }
    }
}
