using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public class CharacterItem
{
    // Use id as primary key
    public string id;
    public string itemId;
    public bool isSubWeapon;
    public int level;
    public int amount;
    // TODO: I want to add random item bonus

    public Item GetItem()
    {
        return GameInstance.Items.ContainsKey(itemId) ? GameInstance.Items[itemId] : null;
    }

    public EquipmentItem GetEquipmentItem()
    {
        var item = GetItem();
        return item != null ? item as EquipmentItem : null;
    }

    public WeaponItem GetWeaponItem()
    {
        var item = GetItem();
        return item != null ? item as WeaponItem : null;
    }

    public ShieldItem GetShieldItem()
    {
        var item = GetItem();
        return item != null ? item as ShieldItem : null;
    }

    public int GetMaxStack()
    {
        var item = GetItem();
        return item == null ? 0 : item.maxStack;
    }

    public bool IsValid()
    {
        return GetItem() != null && amount > 0;
    }

    public bool IsFull()
    {
        return amount == GetMaxStack();
    }

    public bool CanEquip(ICharacterData character)
    {
        return GetEquipmentItem().CanEquip(character, level);
    }

    public CharacterStats GetStats()
    {
        return GetEquipmentItem().GetStats(level);
    }

    public void Empty()
    {
        id = "";
        itemId = "";
        isSubWeapon = false;
        level = 1;
        amount = 0;
    }

    public static CharacterItem MakeCharaterItem(Item item, int level)
    {
        var newItem = new CharacterItem();
        newItem.itemId = item.Id;
        newItem.level = level;
        newItem.amount = 1;
        return newItem;
    }
}

public class NetFieldCharacterItem : LiteNetLibNetField<CharacterItem>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterItem();
        newValue.itemId = reader.GetString();
        newValue.isSubWeapon = reader.GetBool();
        newValue.level = reader.GetInt();
        newValue.amount = reader.GetInt();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        if (Value == null)
            Value = new CharacterItem();
        writer.Put(Value.itemId);
        writer.Put(Value.isSubWeapon);
        writer.Put(Value.level);
        writer.Put(Value.amount);
    }

    public override bool IsValueChanged(CharacterItem newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterItem : LiteNetLibSyncList<NetFieldCharacterItem, CharacterItem> { }
