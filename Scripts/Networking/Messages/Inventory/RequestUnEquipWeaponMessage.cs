using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestUnEquipWeaponMessage : INetSerializable
    {
        public byte equipWeaponSet;
        public bool isLeftHand;
        public short nonEquipIndex;

        public void Deserialize(NetDataReader reader)
        {
            equipWeaponSet = reader.GetByte();
            isLeftHand = reader.GetBool();
            nonEquipIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(equipWeaponSet);
            writer.Put(isLeftHand);
            writer.PutPackedShort(nonEquipIndex);
        }
    }
}
