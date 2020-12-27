using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestEquipArmorMessage : INetSerializable
    {
        public short nonEquipIndex;
        public byte equipSlotIndex;

        public void Deserialize(NetDataReader reader)
        {
            nonEquipIndex = reader.GetPackedShort();
            equipSlotIndex = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedShort(nonEquipIndex);
            writer.Put(equipSlotIndex);
        }
    }
}
