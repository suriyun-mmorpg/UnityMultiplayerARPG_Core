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

    public WeaponItem WeaponItem
    {
        get { return Item != null ? Item as WeaponItem : null; }
    }

    public ShieldItem ShieldItem
    {
        get { return Item != null ? Item as ShieldItem : null; }
    }

    public int MaxStack
    {
        get { return Item.maxStack; }
    }

    public bool IsValid
    {
        get { return !string.IsNullOrEmpty(id) && Item != null && amount > 0; }
    }

    public bool IsFull
    {
        get { return amount == MaxStack; }
    }

    public CharacterStats Stats
    {
        get
        {
            var equipmentItem = EquipmentItem;
            if (equipmentItem == null)
                return new CharacterStats();
            return equipmentItem.baseStats + equipmentItem.statsIncreaseEachLevel * level;
        }
    }

    public CharacterStatsPercentage StatsPercentage
    {
        get
        {
            var equipmentItem = EquipmentItem;
            if (equipmentItem == null)
                return new CharacterStatsPercentage();
            return equipmentItem.statsPercentageIncreaseEachLevel * level;
        }
    }

    public void Empty()
    {
        id = "";
        itemId = "";
        level = 0;
        amount = 0;
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
