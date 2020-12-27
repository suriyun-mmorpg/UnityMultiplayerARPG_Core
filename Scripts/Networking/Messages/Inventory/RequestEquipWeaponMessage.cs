using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestEquipWeaponMessage : INetSerializable
    {
        public short nonEquipIndex;
        public byte equipWeaponSet;
        public bool isLeftHand;

        public void Deserialize(NetDataReader reader)
        {
            nonEquipIndex = reader.GetPackedShort();
            equipWeaponSet = reader.GetByte();
            isLeftHand = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedShort(nonEquipIndex);
            writer.Put(equipWeaponSet);
            writer.Put(isLeftHand);
        }
    }
}
