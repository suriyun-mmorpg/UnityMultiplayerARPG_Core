using System.Collections.Generic;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public sealed class DealingCharacterItem : INetSerializable
    {
        public int nonEquipIndex;
        public CharacterItem characterItem;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(nonEquipIndex);
            writer.Put(characterItem);
        }

        public void Deserialize(NetDataReader reader)
        {
            nonEquipIndex = reader.GetInt();
            characterItem = reader.Get<CharacterItem>();
        }
    }

    public class DealingCharacterItems : List<DealingCharacterItem>, INetSerializable
    {
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Count);
            foreach (DealingCharacterItem dealingItem in this)
            {
                writer.Put(dealingItem);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            Clear();
            int count = reader.GetInt();
            for (int i = 0; i < count; ++i)
            {
                Add(reader.Get<DealingCharacterItem>());
            }
        }
    }
}
