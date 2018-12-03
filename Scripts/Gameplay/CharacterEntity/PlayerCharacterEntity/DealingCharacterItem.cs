using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;

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
            foreach (var dealingItem in this)
            {
                writer.Put(dealingItem.nonEquipIndex);
                writer.Put(dealingItem.dataId);
                writer.Put(dealingItem.level);
                writer.Put(dealingItem.amount);
                writer.Put(dealingItem.durability);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            var count = reader.GetInt();
            Clear();
            for (var i = 0; i < count; ++i)
            {
                var dealingItem = new DealingCharacterItem();
                dealingItem.nonEquipIndex = reader.GetInt();
                dealingItem.dataId = reader.GetInt();
                dealingItem.level = reader.GetShort();
                dealingItem.amount = reader.GetShort();
                dealingItem.durability = reader.GetFloat();
                Add(dealingItem);
            }
        }
    }
}
