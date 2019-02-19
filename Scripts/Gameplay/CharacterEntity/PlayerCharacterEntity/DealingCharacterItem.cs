using System.Collections.Generic;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public sealed class DealingCharacterItem : CharacterItem
    {
        public int nonEquipIndex;

        public override void Serialize(NetDataWriter writer)
        {
            writer.Put(nonEquipIndex);
            base.Serialize(writer);
        }

        public override void Deserialize(NetDataReader reader)
        {
            nonEquipIndex = reader.GetInt();
            base.Deserialize(reader);
        }
    }

    public class DealingCharacterItems : List<DealingCharacterItem>, INetSerializable
    {
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Count);
            foreach (DealingCharacterItem dealingItem in this)
            {
                dealingItem.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            Clear();
            int count = reader.GetInt();
            for (int i = 0; i < count; ++i)
            {
                DealingCharacterItem dealingItem = new DealingCharacterItem();
                dealingItem.Deserialize(reader);
                Add(dealingItem);
            }
        }
    }
}
