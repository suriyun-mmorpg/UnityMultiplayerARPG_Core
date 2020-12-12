using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestEquipArmorMessage : INetSerializable
    {
        public string characterId;
        public short nonEquipIndex;
        public byte equipSlotIndex;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            nonEquipIndex = reader.GetPackedShort();
            equipSlotIndex = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.PutPackedShort(nonEquipIndex);
            writer.Put(equipSlotIndex);
        }
    }
}
