using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestUnEquipArmorMessage : INetSerializable
    {
        public string characterId;
        public byte equipSlotIndex;
        public short nonEquipIndex;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            equipSlotIndex = reader.GetByte();
            nonEquipIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.Put(equipSlotIndex);
            writer.PutPackedShort(nonEquipIndex);
        }
    }
}
