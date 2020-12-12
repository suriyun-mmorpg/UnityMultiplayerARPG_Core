using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestUnEquipArmorMessage : INetSerializable
    {
        public string characterId;
        public short equipIndex;
        public short nonEquipIndex;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            equipIndex = reader.GetPackedShort();
            nonEquipIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.PutPackedShort(equipIndex);
            writer.PutPackedShort(nonEquipIndex);
        }
    }
}
