using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestUnEquipArmorMessage : INetSerializable
    {
        public short equipIndex;
        public short nonEquipIndex;

        public void Deserialize(NetDataReader reader)
        {
            equipIndex = reader.GetPackedShort();
            nonEquipIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedShort(equipIndex);
            writer.PutPackedShort(nonEquipIndex);
        }
    }
}
