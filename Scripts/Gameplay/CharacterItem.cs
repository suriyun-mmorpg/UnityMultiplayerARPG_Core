using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public struct CharacterItem
{
    // Use id as primary key
    public string id;
    public string itemId;
    public int level;
    public int amount;
    // TODO: I want to add random item bonus

    public Item Item
    {
        get { return GameInstance.Items.ContainsKey(itemId) ? GameInstance.Items[itemId] : null; }
    }

    public EquipmentItem EquipmentItem
    {
        get { return Item != null ? Item as EquipmentItem : null; }
    }
}

public class NetFieldCharacterItem : LiteNetLibNetField<CharacterItem>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterItem();
        newValue.id = reader.GetString();
        newValue.itemId = reader.GetString();
        newValue.level = reader.GetInt();
        newValue.amount = reader.GetInt();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.id);
        writer.Put(Value.itemId);
        writer.Put(Value.level);
        writer.Put(Value.amount);
    }

    public override bool IsValueChanged(CharacterItem newValue)
    {
        return !newValue.Equals(Value);
    }
}

[System.Serializable]
public class SyncListCharacterItem : LiteNetLibSyncList<NetFieldCharacterItem, CharacterItem> { }
