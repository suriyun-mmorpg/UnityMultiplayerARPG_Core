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

    private string dirtyItemId;
    private int dirtyLevel;
    private Item cacheItem;
    private EquipmentItem cacheEquipmentItem;
    private WeaponItem cacheWeaponItem;
    private ShieldItem cacheShieldItem;
    private readonly Dictionary<DamageElement, DamageAmount> cacheDamageElementAmountPairs = new Dictionary<DamageElement, DamageAmount>();

    private bool IsDirty()
    {
        return string.IsNullOrEmpty(dirtyItemId) ||
            !dirtyItemId.Equals(itemId) ||
            dirtyLevel != level;
    }

    private void MakeCache()
    {
        if (!IsDirty())
            return;

        dirtyItemId = itemId;
        dirtyLevel = level;
        var gameInstance = GameInstance.Singleton;
        cacheItem = GameInstance.Items.ContainsKey(itemId) ? GameInstance.Items[itemId] : null;
        cacheEquipmentItem = cacheItem != null ? cacheItem as EquipmentItem : null;
        cacheWeaponItem = cacheItem != null ? cacheItem as WeaponItem : null;
        cacheShieldItem = cacheItem != null ? cacheItem as ShieldItem : null;
        cacheDamageElementAmountPairs.Clear();
        if (cacheWeaponItem != null)
        {
            var damageAttributes = cacheWeaponItem.damageAttributes;
            foreach (var damageAttribute in damageAttributes)
            {
                var element = damageAttribute.damageElement;
                if (element == null)
                    element = gameInstance.DefaultDamageElement;
                if (!cacheDamageElementAmountPairs.ContainsKey(element))
                    cacheDamageElementAmountPairs[element] = damageAttribute.damageAmount + damageAttribute.damageAmountIncreaseEachLevel * level;
            }
        }
    }

    public Item GetItem()
    {
        return cacheItem;
    }

    public EquipmentItem GetEquipmentItem()
    {
        return cacheEquipmentItem;
    }

    public WeaponItem GetWeaponItem()
    {
        return cacheWeaponItem;
    }

    public ShieldItem GetShieldItem()
    {
        return cacheShieldItem;
    }

    public Dictionary<DamageElement, DamageAmount> GetDamageElementAmountPairs()
    {
        MakeCache();
        return cacheDamageElementAmountPairs;
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

    public CharacterStats GetStats()
    {
        var equipmentItem = GetEquipmentItem();
        if (equipmentItem == null)
            return new CharacterStats();
        return equipmentItem.baseStats + equipmentItem.statsIncreaseEachLevel * level;
    }

    public CharacterStatsPercentage GetStatsPercentage()
    {
        var equipmentItem = GetEquipmentItem();
        if (equipmentItem == null)
            return new CharacterStatsPercentage();
        return equipmentItem.baseStatsPercentage + equipmentItem.statsPercentageIncreaseEachLevel * level;
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
