using System.Collections.Generic;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class DealingCharacterItem : CharacterItem
    {
        public int nonEquipIndex;
    }

    public class DealingCharacterItems : List<DealingCharacterItem>, INetSerializable
    {
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Count);
            foreach (DealingCharacterItem dealingItem in this)
            {
                writer.Put(dealingItem.nonEquipIndex);
                writer.Put(dealingItem.dataId);
                writer.Put(dealingItem.level);
                writer.Put(dealingItem.amount);
                writer.Put(dealingItem.durability);
                writer.Put(dealingItem.exp);
                writer.Put(dealingItem.lockRemainsDuration);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            int count = reader.GetInt();
            Clear();
            for (int i = 0; i < count; ++i)
            {
                DealingCharacterItem dealingItem = new DealingCharacterItem();
                dealingItem.nonEquipIndex = reader.GetInt();
                dealingItem.dataId = reader.GetInt();
                dealingItem.level = reader.GetShort();
                dealingItem.amount = reader.GetShort();
                dealingItem.durability = reader.GetFloat();
                dealingItem.exp = reader.GetInt();
                dealingItem.lockRemainsDuration = reader.GetFloat();
                Add(dealingItem);
            }
        }
    }
}
