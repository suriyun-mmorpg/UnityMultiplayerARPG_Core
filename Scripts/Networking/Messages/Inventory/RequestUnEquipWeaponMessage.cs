using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestUnEquipWeaponMessage : INetSerializable
    {
        public string characterId;
        public byte equipWeaponSet;
        public bool isLeftHand;
        public short nonEquipIndex;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            equipWeaponSet = reader.GetByte();
            isLeftHand = reader.GetBool();
            nonEquipIndex = reader.GetPackedShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.Put(equipWeaponSet);
            writer.Put(isLeftHand);
            writer.PutPackedShort(nonEquipIndex);
        }
    }
}
