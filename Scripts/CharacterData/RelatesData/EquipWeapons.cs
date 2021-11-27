using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class EquipWeapons : INetSerializable
    {
        public CharacterItem rightHand;
        public CharacterItem leftHand;

        public EquipWeapons()
        {
            rightHand = new CharacterItem();
            leftHand = new CharacterItem();
        }

        private void Validate()
        {
            if (rightHand == null)
                rightHand = new CharacterItem();

            if (leftHand == null)
                leftHand = new CharacterItem();
        }

        public void Serialize(NetDataWriter writer)
        {
            Validate();
            // Right hand
            rightHand.Serialize(writer);
            // Left hand
            leftHand.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            Validate();
            // Right hand
            rightHand.Deserialize(reader);
            // Left hand
            leftHand.Deserialize(reader);
        }

        public void Deserialize(NetDataReader reader, LiteNetLibElement element)
        {
            Deserialize(reader);
        }
    }

    [System.Serializable]
    public class SyncFieldEquipWeapons : LiteNetLibSyncField<EquipWeapons>
    {
        protected override bool IsValueChanged(EquipWeapons newValue)
        {
            return true;
        }
    }


    [System.Serializable]
    public class SyncListEquipWeapons : LiteNetLibSyncList<EquipWeapons>
    {
    }
}
