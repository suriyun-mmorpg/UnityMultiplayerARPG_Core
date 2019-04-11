using System.Collections.Generic;
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
    public short ammo;
    public List<int> sockets = new List<int>();
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
    [System.NonSerialized]
    private Item cacheSocketEnhancer;

    public List<int> Sockets
    {
        get
        {
            if (sockets == null)
                sockets = new List<int>();
            return sockets;
        }
    }

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
            cacheSocketEnhancer = null;
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
                if (cacheItem.IsSocketEnhancer())
                    cacheSocketEnhancer = cacheItem;
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

    public Item GetSocketEnhancerItem()
    {
        MakeCache();
        return cacheSocketEnhancer;
    }

    public short GetMaxStack()
    {
        return GetItem() == null ? (short)0 : GetItem().maxStack;
    }

    public float GetMaxDurability()
    {
        return GetItem() == null ? 0f : GetItem().maxDurability;
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

    public Dictionary<Attribute, short> GetIncreaseAttributes()
    {
        if (GetEquipmentItem() == null)
            return null;
        return GetEquipmentItem().GetIncreaseAttributes(level, GetEquipmentBonusRate());
    }

    public Dictionary<DamageElement, float> GetIncreaseResistances()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return null;
        return GetEquipmentItem().GetIncreaseResistances(level, GetEquipmentBonusRate());
    }

    public Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return null;
        return GetEquipmentItem().GetIncreaseDamages(level, GetEquipmentBonusRate());
    }

    public Dictionary<Skill, short> GetIncreaseSkills()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return null;
        return GetEquipmentItem().GetIncreaseSkills();
    }

    public CharacterStats GetIncreaseStats()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return CharacterStats.Empty;
        return GetEquipmentItem().GetIncreaseStats(level, GetEquipmentBonusRate());
    }

    public Dictionary<Attribute, short> GetSocketsIncreaseAttributes()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return null;
        Dictionary<Attribute, short> result = new Dictionary<Attribute, short>();
        Item tempEnhancer;
        foreach (int socketId in Sockets)
        {
            if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
            {
                // Level for increase stats always 1
                result = GameDataHelpers.CombineAttributes(tempEnhancer.socketEnhanceEffect.attributes, result, 1f);
            }
        }
        return result;
    }

    public Dictionary<DamageElement, float> GetSocketsIncreaseResistances()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return null;
        Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
        Item tempEnhancer;
        foreach (int socketId in Sockets)
        {
            if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
            {
                // Level for increase stats always 1
                result = GameDataHelpers.CombineResistances(tempEnhancer.socketEnhanceEffect.resistances, result, 1f);
            }
        }
        return result;
    }

    public Dictionary<DamageElement, MinMaxFloat> GetSocketsIncreaseDamages()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return null;
        Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
        Item tempEnhancer;
        foreach (int socketId in Sockets)
        {
            if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
            {
                // Level for increase stats always 1
                result = GameDataHelpers.CombineDamages(tempEnhancer.socketEnhanceEffect.damages, result, 1f);
            }
        }
        return result;
    }

    public Dictionary<Skill, short> GetSocketsIncreaseSkills()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return null;
        Dictionary<Skill, short> result = new Dictionary<Skill, short>();
        Item tempEnhancer;
        foreach (int socketId in Sockets)
        {
            if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
            {
                // Level for increase stats always 1
                result = GameDataHelpers.CombineSkills(tempEnhancer.socketEnhanceEffect.skills, result);
            }
        }
        return result;
    }

    public CharacterStats GetSocketsIncreaseStats()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return CharacterStats.Empty;
        CharacterStats result = new CharacterStats();
        Item tempEnhancer;
        foreach (int socketId in Sockets)
        {
            if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
            {
                // Level for increase stats always 1
                result += tempEnhancer.socketEnhanceEffect.stats;
            }
        }
        return result;
    }

    public CharacterItem Clone()
    {
        CharacterItem cloneItem = new CharacterItem();
        cloneItem.dataId = dataId;
        cloneItem.level = level;
        cloneItem.amount = amount;
        cloneItem.durability = durability;
        cloneItem.exp = exp;
        cloneItem.lockRemainsDuration = lockRemainsDuration;
        return cloneItem;
    }

    public static CharacterItem Create(Item item, short level = 1, short amount = 1)
    {
        return Create(item.DataId, level, amount);
    }

    public static CharacterItem Create(int dataId, short level = 1, short amount = 1)
    {
        CharacterItem newItem = new CharacterItem();
        newItem.dataId = dataId;
        newItem.level = level;
        newItem.amount = amount;
        newItem.durability = 0f;
        newItem.exp = 0;
        newItem.lockRemainsDuration = 0f;
        newItem.ammo = 0;
        Item tempItem = null;
        if (GameInstance.Items.TryGetValue(dataId, out tempItem))
        {
            newItem.durability = tempItem.maxDurability;
            newItem.lockRemainsDuration = tempItem.lockDuration;
        }
        return newItem;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(dataId);
        writer.Put(level);
        writer.Put(amount);
        writer.Put(lockRemainsDuration);
        // Put only needed data
        if (GetEquipmentItem() != null)
        {
            writer.Put(durability);
            writer.Put(exp);
            if (GetWeaponItem() != null)
            {
                writer.Put(ammo);
                byte socketCount = (byte)Sockets.Count;
                writer.Put(socketCount);
                if (socketCount > 0)
                {
                    foreach (int socketDataId in Sockets)
                    {
                        writer.Put(socketDataId);
                    }
                }
            }
        }
        if (GetPetItem() != null)
        {
            writer.Put(exp);
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        dataId = reader.GetInt();
        level = reader.GetShort();
        amount = reader.GetShort();
        lockRemainsDuration = reader.GetFloat();
        // Read only needed data
        if (GetEquipmentItem() != null)
        {
            durability = reader.GetFloat();
            exp = reader.GetInt();
            if (GetWeaponItem() != null)
            {
                ammo = reader.GetShort();
                int socketCount = reader.GetByte();
                Sockets.Clear();
                for (int i = 0; i < socketCount; ++i)
                {
                    Sockets.Add(reader.GetInt());
                }
            }
        }
        if (GetPetItem() != null)
        {
            exp = reader.GetInt();
        }
    }
}

[System.Serializable]
public class SyncListCharacterItem : LiteNetLibSyncList<CharacterItem>
{
}
