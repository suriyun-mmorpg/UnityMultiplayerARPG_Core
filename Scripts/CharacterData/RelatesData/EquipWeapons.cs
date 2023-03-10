using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class EquipWeapons : INetSerializable
    {
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
            writer.Put(rightHand);
            // Left hand
            writer.Put(leftHand);
        }

        public void Deserialize(NetDataReader reader)
        {
            Validate();
            // Right hand
            rightHand = reader.Get<CharacterItem>();
            // Left hand
            leftHand = reader.Get<CharacterItem>();
        }

        public void Deserialize(NetDataReader reader, LiteNetLibElement element)
        {
            Deserialize(reader);
        }

        public EquipWeapons Clone(bool generateNewId = false)
        {
            return new EquipWeapons()
            {
                rightHand = rightHand.Clone(generateNewId),
                leftHand = leftHand.Clone(generateNewId),
            };
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
