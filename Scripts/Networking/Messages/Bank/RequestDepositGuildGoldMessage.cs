using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestDepositGuildGoldMessage : INetSerializable
    {
        public string characterId;
        public int gold;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            gold = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.PutPackedInt(gold);
        }
    }
}
