using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public struct CharacterItem
{
    public static readonly CharacterItem Empty = new CharacterItem();
    // Use id as primary key
    public string id;
    public string itemId;
    public int level;
    public int amount;
    // TODO: I want to add random item bonus
    [System.NonSerialized]
    private string dirtyItemId;
    [System.NonSerialized]
    private Item cacheItem;
    [System.NonSerialized]
    private Item cacheEquipmentItem;
    [System.NonSerialized]
    private Item cacheDefendItem;
    [System.NonSerialized]
    private Item cacheArmorItem;
    [System.NonSerialized]
    private Item cacheWeaponItem;
    [System.NonSerialized]
    private Item cacheShieldItem;

    private void MakeCache()
    {
        if (string.IsNullOrEmpty(itemId))
        {
            cacheItem = null;
            cacheEquipmentItem = null;
            cacheDefendItem = null;
            cacheArmorItem = null;
            cacheWeaponItem = null;
            cacheShieldItem = null;
            return;
        }
        if (string.IsNullOrEmpty(dirtyItemId) || !dirtyItemId.Equals(itemId))
        {
            dirtyItemId = itemId;
            cacheItem = GameInstance.Items.TryGetValue(itemId, out cacheItem) ? cacheItem : null;
            if (cacheItem != null)
            {
                if (cacheItem.IsEquipment())
                    cacheEquipmentItem = cacheItem;
                if (cacheItem.IsDefendEquipment())
                    cacheDefendItem = cacheItem;
                if (cacheItem.IsArmor())
                    cacheArmorItem = cacheItem;
                if (cacheItem.IsWeapon())
                    cacheWeaponItem = cacheItem;
                if (cacheItem.IsShield())
                    cacheShieldItem = cacheItem;
            }
        }
    }

    public bool IsEmpty()
    {
        return Equals(Empty);
    }

    public Item GetItem()
    {
        MakeCache();
        return cacheItem;
    }

    public Item GetEquipmentItem()
    {
        MakeCache();
        return cacheEquipmentItem;
    }

    public Item GetDefendItem()
    {
        MakeCache();
        return cacheDefendItem;
    }

    public Item GetArmorItem()
    {
        MakeCache();
        return cacheArmorItem;
    }

    public Item GetWeaponItem()
    {
        MakeCache();
        return cacheWeaponItem;
    }

    public Item GetShieldItem()
    {
        MakeCache();
        return cacheShieldItem;
    }

    public int GetMaxStack()
    {
        var item = GetItem();
        return item == null ? 0 : item.maxStack;
    }

    public bool IsValid()
    {
        return !IsEmpty() && GetItem() != null && amount > 0;
    }

    public bool IsFull()
    {
        return amount == GetMaxStack();
    }

    public bool CanEquip(ICharacterData character)
    {
        return GetEquipmentItem().CanEquip(character, level);
    }

    public static CharacterItem Create(Item item, int level)
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
        newValue.level = reader.GetInt();
        newValue.amount = reader.GetInt();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.itemId);
        writer.Put(Value.level);
        writer.Put(Value.amount);
    }

    public override bool IsValueChanged(CharacterItem newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterItem : LiteNetLibSyncList<NetFieldCharacterItem, CharacterItem>
{
    public int IndexOf(string itemId)
    {
        CharacterItem tempItem;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempItem = list[i];
            if (!string.IsNullOrEmpty(tempItem.itemId) &&
                tempItem.itemId.Equals(itemId))
            {
                index = i;
                break;
            }
        }
        return index;
    }
}
