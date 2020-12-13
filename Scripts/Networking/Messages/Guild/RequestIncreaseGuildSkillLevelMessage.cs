using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestIncreaseGuildSkillLevelMessage : INetSerializable
    {
        public string characterId;
        public int dataId;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            dataId = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.PutPackedInt(dataId);
        }
    }
}
