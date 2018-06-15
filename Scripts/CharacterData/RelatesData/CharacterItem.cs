using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;

[System.Serializable]
public class CharacterItem
{
    public static readonly CharacterItem Empty = new CharacterItem();
    public int dataId;
    public short level;
    public short amount;
    // TODO: I want to add random item bonus
    [System.NonSerialized]
    private int dirtyDataId;
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
    [System.NonSerialized]
    private Item cachePotionItem;
    [System.NonSerialized]
    private Item cacheAmmoItem;

    private void MakeCache()
    {
        if (!GameInstance.Items.ContainsKey(dataId))
        {
            cacheItem = null;
            cacheEquipmentItem = null;
            cacheDefendItem = null;
            cacheArmorItem = null;
            cacheWeaponItem = null;
            cacheShieldItem = null;
            cachePotionItem = null;
            cacheAmmoItem = null;
            return;
        }
        if (dirtyDataId != dataId)
        {
            dirtyDataId = dataId;
            cacheItem = null;
            cacheEquipmentItem = null;
            cacheDefendItem = null;
            cacheArmorItem = null;
            cacheWeaponItem = null;
            cacheShieldItem = null;
            cachePotionItem = null;
            cacheAmmoItem = null;
            cacheItem = GameInstance.Items.TryGetValue(dataId, out cacheItem) ? cacheItem : null;
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
                if (cacheItem.IsPotion())
                    cachePotionItem = cacheItem;
                if (cacheItem.IsAmmo())
                    cacheAmmoItem = cacheItem;
            }
        }
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

    public Item GetPotionItem()
    {
        MakeCache();
        return cachePotionItem;
    }

    public Item GetAmmoItem()
    {
        MakeCache();
        return cacheAmmoItem;
    }

    public short GetMaxStack()
    {
        var item = GetItem();
        return item == null ? (short)0 : item.maxStack;
    }

    public bool IsValid()
    {
        return !this.IsEmpty() && GetItem() != null && amount > 0;
    }

    public bool IsFull()
    {
        return amount == GetMaxStack();
    }

    public bool CanEquip(ICharacterData character)
    {
        return GetEquipmentItem().CanEquip(character, level);
    }

    public static CharacterItem Create(Item item, short level = 1, short amount = 1)
    {
        return Create(item.HashId, level, amount);
    }

    public static CharacterItem Create(int dataId, short level = 1, short amount = 1)
    {
        var newItem = new CharacterItem();
        newItem.dataId = dataId;
        newItem.level = level;
        newItem.amount = amount;
        return newItem;
    }
}

public class NetFieldCharacterItem : LiteNetLibNetField<CharacterItem>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterItem();
        newValue.dataId = reader.GetInt();
        newValue.level = reader.GetShort();
        newValue.amount = reader.GetShort();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.dataId);
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
}
