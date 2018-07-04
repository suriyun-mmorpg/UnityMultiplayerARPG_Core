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

    public class DealingCharacterItems : List<DealingCharacterItem> { }

    public class NetFieldDealingCharacterItems : LiteNetLibNetField<DealingCharacterItems>
    {
        public override void Deserialize(NetDataReader reader)
        {
            var newValue = new DealingCharacterItems();
            var count = reader.GetInt();
            for (var i = 0; i < count; ++i)
            {
                var dealingItem = new DealingCharacterItem();
                dealingItem.nonEquipIndex = reader.GetInt();
                dealingItem.dataId = reader.GetInt();
                dealingItem.level = reader.GetShort();
                dealingItem.amount = reader.GetShort();
                dealingItem.durability = reader.GetFloat();
                newValue.Add(dealingItem);
            }
            Value = newValue;
        }

        public override void Serialize(NetDataWriter writer)
        {
            writer.Put(Value.Count);
            foreach (var dealingItem in Value)
            {
                writer.Put(dealingItem.nonEquipIndex);
                writer.Put(dealingItem.dataId);
                writer.Put(dealingItem.level);
                writer.Put(dealingItem.amount);
                writer.Put(dealingItem.durability);
            }
        }

        public override bool IsValueChanged(DealingCharacterItems newValue)
        {
            return true;
        }
    }
}
