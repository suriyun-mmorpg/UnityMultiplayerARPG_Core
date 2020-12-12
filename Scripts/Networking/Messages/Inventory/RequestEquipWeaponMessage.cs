using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestEquipWeaponMessage : INetSerializable
    {
        public string characterId;
        public short nonEquipIndex;
        public byte equipWeaponSet;
        public bool isLeftHand;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            nonEquipIndex = reader.GetPackedShort();
            equipWeaponSet = reader.GetByte();
            isLeftHand = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.PutPackedShort(nonEquipIndex);
            writer.Put(equipWeaponSet);
            writer.Put(isLeftHand);
        }
    }
}
