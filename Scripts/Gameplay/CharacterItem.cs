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
    private string dirtyItemId;
    private int dirtyLevel;
    private Item cacheItem;
    private BaseEquipmentItem cacheEquipmentItem;
    private BaseDefendItem cacheDefendItem;
    private ArmorItem cacheArmorItem;
    private WeaponItem cacheWeaponItem;
    private ShieldItem cacheShieldItem;

    private void MakeCache()
    {
        if (string.IsNullOrEmpty(itemId))
            return;
        if (string.IsNullOrEmpty(dirtyItemId) || dirtyItemId.Equals(itemId) || level != dirtyLevel)
        {
            dirtyItemId = itemId;
            dirtyLevel = level;
            if (cacheItem == null)
                cacheItem = GameInstance.Items.TryGetValue(itemId, out cacheItem) ? cacheItem : null;
            if (cacheItem != null)
            {
                cacheEquipmentItem = cacheItem as BaseEquipmentItem;
                cacheDefendItem = cacheItem as BaseDefendItem;
                cacheArmorItem = cacheItem as ArmorItem;
                cacheWeaponItem = cacheItem as WeaponItem;
                cacheShieldItem = cacheItem as ShieldItem;
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

    public BaseEquipmentItem GetEquipmentItem()
    {
        MakeCache();
        return cacheEquipmentItem;
    }

    public BaseDefendItem GetDefendItem()
    {
        MakeCache();
        return cacheDefendItem;
    }

    public ArmorItem GetArmorItem()
    {
        MakeCache();
        return cacheArmorItem;
    }

    public WeaponItem GetWeaponItem()
    {
        MakeCache();
        return cacheWeaponItem;
    }

    public ShieldItem GetShieldItem()
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
public class SyncListCharacterItem : LiteNetLibSyncList<NetFieldCharacterItem, CharacterItem> { }
