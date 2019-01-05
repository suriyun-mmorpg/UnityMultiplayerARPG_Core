using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

[System.Serializable]
public class CharacterItem : INetSerializable
{
    public static readonly CharacterItem Empty = new CharacterItem();
    public int dataId;
    public short level;
    public short amount;
    public float durability;
    public int exp;
    public float lockRemainsDuration;
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
    [System.NonSerialized]
    private Item cacheBuildingItem;
    [System.NonSerialized]
    private Item cachePetItem;

    private void MakeCache()
    {
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
            cacheBuildingItem = null;
            cachePetItem = null;
            if (GameInstance.Items.TryGetValue(dataId, out cacheItem) && cacheItem != null)
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
                if (cacheItem.IsBuilding())
                    cacheBuildingItem = cacheItem;
                if (cacheItem.IsPet())
                    cachePetItem = cacheItem;
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

    public Item GetBuildingItem()
    {
        MakeCache();
        return cacheBuildingItem;
    }

    public Item GetPetItem()
    {
        MakeCache();
        return cachePetItem;
    }

    public short GetMaxStack()
    {
        return GetItem() == null ? (short)0 : GetItem().maxStack;
    }

    public float GetMaxDurability()
    {
        return GetItem() == null ? 0f : GetItem().maxDurability;
    }

    public bool IsValid()
    {
        return !this.IsEmpty() && GetItem() != null && amount > 0;
    }

    public bool IsFull()
    {
        return amount == GetMaxStack();
    }

    public bool IsBroken()
    {
        return GetMaxDurability() > 0 && durability <= 0;
    }

    public bool IsLock()
    {
        return lockRemainsDuration > 0;
    }

    public void Lock(float duration)
    {
        lockRemainsDuration = duration;
    }

    public bool ShouldRemove()
    {
        // TODO: have expire date to remove
        return false;
    }

    public void Update(float deltaTime)
    {
        lockRemainsDuration -= deltaTime;
    }

    public float GetEquipmentBonusRate()
    {
        return GameInstance.Singleton.GameplayRule.GetEquipmentBonusRate(this);
    }

    public bool CanEquip(ICharacterData character)
    {
        return GetEquipmentItem().CanEquip(character, level);
    }

    public int GetNextLevelExp()
    {
        if (GetPetItem() == null || level <= 0)
            return 0;
        int[] expTree = GameInstance.Singleton.ExpTree;
        if (level > expTree.Length)
            return 0;
        return expTree[level - 1];
    }

    public static CharacterItem Create(Item item, short level = 1, short amount = 1)
    {
        return Create(item.DataId, level, amount);
    }

    public static CharacterItem Create(int dataId, short level = 1, short amount = 1)
    {
        CharacterItem newItem = new CharacterItem();
        Item tempItem = null;
        newItem.dataId = dataId;
        newItem.level = level;
        newItem.amount = amount;
        newItem.durability = GameInstance.Items.TryGetValue(dataId, out tempItem) ? tempItem.maxDurability : 0;
        return newItem;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(dataId);
        writer.Put(level);
        writer.Put(amount);
        writer.Put(durability);
        writer.Put(exp);
        writer.Put(lockRemainsDuration);
    }

    public void Deserialize(NetDataReader reader)
    {
        dataId = reader.GetInt();
        level = reader.GetShort();
        amount = reader.GetShort();
        durability = reader.GetFloat();
        exp = reader.GetInt();
        lockRemainsDuration = reader.GetFloat();
    }
}

[System.Serializable]
public class SyncListCharacterItem : LiteNetLibSyncList<CharacterItem>
{
}
