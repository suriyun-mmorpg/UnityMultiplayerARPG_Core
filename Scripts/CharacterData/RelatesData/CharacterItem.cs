using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

[System.Serializable]
public class CharacterItem : INetSerializableWithElement
{
    public static readonly CharacterItem Empty = new CharacterItem();
    public string id;
    public int dataId;
    public short level;
    public short amount;
    public byte equipSlotIndex;
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
    private Item cacheSocketEnhancerItem;
    [System.NonSerialized]
    private Item cacheMountItem;
    [System.NonSerialized]
    private Item cacheAttributeIncreaseItem;
    [System.NonSerialized]
    private Item cacheAttributeResetItem;
    [System.NonSerialized]
    private Item cacheSkillItem;
    [System.NonSerialized]
    private Item cacheSkillLearnItem;
    [System.NonSerialized]
    private Item cacheSkillResetItem;

    [System.NonSerialized]
    private LiteNetLibElement element;
    public LiteNetLibElement Element
    {
        get { return element; }
        set { element = value; }
    }

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
            cacheSocketEnhancerItem = null;
            cacheMountItem = null;
            cacheAttributeIncreaseItem = null;
            cacheAttributeResetItem = null;
            cacheSkillItem = null;
            cacheSkillLearnItem = null;
            cacheSkillResetItem = null;
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
                    cacheSocketEnhancerItem = cacheItem;
                if (cacheItem.IsMount())
                    cacheMountItem = cacheItem;
                if (cacheItem.IsAttributeIncrease())
                    cacheAttributeIncreaseItem = cacheItem;
                if (cacheItem.IsAttributeReset())
                    cacheAttributeResetItem = cacheItem;
                if (cacheItem.IsSkill())
                    cacheSkillItem = cacheItem;
                if (cacheItem.IsSkillLearn())
                    cacheSkillLearnItem = cacheItem;
                if (cacheItem.IsSkillReset())
                    cacheSkillResetItem = cacheItem;
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
        return cacheSocketEnhancerItem;
    }

    public Item GetMountItem()
    {
        MakeCache();
        return cacheMountItem;
    }

    public Item GetAttributeIncreaseItem()
    {
        MakeCache();
        return cacheAttributeIncreaseItem;
    }

    public Item GetAttributeResetItem()
    {
        MakeCache();
        return cacheAttributeResetItem;
    }

    public Item GetSkillItem()
    {
        MakeCache();
        return cacheSkillItem;
    }

    public Item GetSkillLearnItem()
    {
        MakeCache();
        return cacheSkillLearnItem;
    }

    public Item GetSkillResetItem()
    {
        MakeCache();
        return cacheSkillResetItem;
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

    public bool IsAmmoEmpty()
    {
        Item item = GetWeaponItem();
        if (item != null)
        {
            if (item.ammoCapacity > 0)
                return ammo == 0;
        }
        return false;
    }

    public bool IsAmmoFull()
    {
        Item item = GetWeaponItem();
        if (item != null)
        {
            if (item.ammoCapacity > 0)
                return ammo >= item.ammoCapacity;
        }
        return true;
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

    public float GetEquipmentStatsRate()
    {
        return GameInstance.Singleton.GameplayRule.GetEquipmentStatsRate(this);
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

    public KeyValuePair<DamageElement, float> GetArmorAmount()
    {
        if (GetDefendItem() == null)
            return new KeyValuePair<DamageElement, float>();
        return GetDefendItem().GetArmorAmount(level, GetEquipmentStatsRate());
    }

    public KeyValuePair<DamageElement, MinMaxFloat> GetDamageAmount(ICharacterData characterData)
    {
        if (GetWeaponItem() == null)
            return new KeyValuePair<DamageElement, MinMaxFloat>();
        return GetWeaponItem().GetDamageAmount(level, GetEquipmentStatsRate(), characterData);
    }

    public CharacterStats GetIncreaseStats()
    {
        if (GetEquipmentItem() == null)
            return CharacterStats.Empty;
        return GetEquipmentItem().GetIncreaseStats(level);
    }

    public CharacterStats GetIncreaseStatsRate()
    {
        if (GetEquipmentItem() == null)
            return CharacterStats.Empty;
        return GetEquipmentItem().GetIncreaseStatsRate(level);
    }

    public Dictionary<Attribute, float> GetIncreaseAttributes()
    {
        if (GetEquipmentItem() == null)
            return null;
        return GetEquipmentItem().GetIncreaseAttributes(level);
    }

    public Dictionary<Attribute, float> GetIncreaseAttributesRate()
    {
        if (GetEquipmentItem() == null)
            return null;
        return GetEquipmentItem().GetIncreaseAttributesRate(level);
    }

    public Dictionary<DamageElement, float> GetIncreaseResistances()
    {
        if (GetEquipmentItem() == null)
            return null;
        return GetEquipmentItem().GetIncreaseResistances(level);
    }

    public Dictionary<DamageElement, float> GetIncreaseArmors()
    {
        if (GetEquipmentItem() == null)
            return null;
        return GetEquipmentItem().GetIncreaseArmors(level);
    }

    public Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages()
    {
        if (GetEquipmentItem() == null)
            return null;
        return GetEquipmentItem().GetIncreaseDamages(level);
    }

    public Dictionary<BaseSkill, short> GetIncreaseSkills()
    {
        if (GetEquipmentItem() == null)
            return null;
        return GetEquipmentItem().GetIncreaseSkills();
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
                result += tempEnhancer.socketEnhanceEffect.stats;
        }
        return result;
    }

    public CharacterStats GetSocketsIncreaseStatsRate()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return CharacterStats.Empty;
        CharacterStats result = new CharacterStats();
        Item tempEnhancer;
        foreach (int socketId in Sockets)
        {
            if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
                result += tempEnhancer.socketEnhanceEffect.statsRate;
        }
        return result;
    }

    public Dictionary<Attribute, float> GetSocketsIncreaseAttributes()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return null;
        Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
        Item tempEnhancer;
        foreach (int socketId in Sockets)
        {
            if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
                result = GameDataHelpers.CombineAttributes(tempEnhancer.socketEnhanceEffect.attributes, result, 1f);
        }
        return result;
    }

    public Dictionary<Attribute, float> GetSocketsIncreaseAttributesRate()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return null;
        Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
        Item tempEnhancer;
        foreach (int socketId in Sockets)
        {
            if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
                result = GameDataHelpers.CombineAttributes(tempEnhancer.socketEnhanceEffect.attributesRate, result, 1f);
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
                result = GameDataHelpers.CombineResistances(tempEnhancer.socketEnhanceEffect.resistances, result, 1f);
        }
        return result;
    }

    public Dictionary<DamageElement, float> GetSocketsIncreaseArmors()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return null;
        Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
        Item tempEnhancer;
        foreach (int socketId in Sockets)
        {
            if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
                result = GameDataHelpers.CombineArmors(tempEnhancer.socketEnhanceEffect.armors, result, 1f);
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
                result = GameDataHelpers.CombineDamages(tempEnhancer.socketEnhanceEffect.damages, result, 1f);
        }
        return result;
    }

    public Dictionary<BaseSkill, short> GetSocketsIncreaseSkills()
    {
        if (GetEquipmentItem() == null || Sockets.Count == 0)
            return null;
        Dictionary<BaseSkill, short> result = new Dictionary<BaseSkill, short>();
        Item tempEnhancer;
        foreach (int socketId in Sockets)
        {
            if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
                result = GameDataHelpers.CombineSkills(tempEnhancer.socketEnhanceEffect.skills, result);
        }
        return result;
    }

    public CharacterItem Clone()
    {
        CharacterItem cloneItem = new CharacterItem();
        cloneItem.id = id;
        cloneItem.dataId = dataId;
        cloneItem.level = level;
        cloneItem.amount = amount;
        cloneItem.equipSlotIndex = equipSlotIndex;
        cloneItem.sockets = sockets;
        cloneItem.durability = durability;
        cloneItem.exp = exp;
        cloneItem.lockRemainsDuration = lockRemainsDuration;
        cloneItem.ammo = ammo;
        cloneItem.sockets = new List<int>(sockets);
        return cloneItem;
    }

    public static CharacterItem Create(Item item, short level = 1, short amount = 1)
    {
        return Create(item.DataId, level, amount);
    }

    public static CharacterItem Create(int dataId, short level = 1, short amount = 1)
    {
        CharacterItem newItem = new CharacterItem();
        newItem.id = GenericUtils.GetUniqueId();
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

    public static CharacterItem CreateEmptySlot()
    {
        return Create(0, 1, 0);
    }

    public void Serialize(NetDataWriter writer)
    {
        Serialize(writer, Element == null || Element.SendingConnectionId == Element.ConnectionId);
    }

    public void Serialize(NetDataWriter writer, bool isOwnerClient)
    {
        writer.Put(isOwnerClient);
        if (isOwnerClient)
        {
            writer.Put(id);
        }

        writer.Put(dataId);
        writer.Put(level);
        writer.Put(amount);
        writer.Put(equipSlotIndex);

        if (isOwnerClient)
        {
            writer.Put(lockRemainsDuration);
        }

        // Put only needed data
        if (GetEquipmentItem() != null)
        {
            if (isOwnerClient)
            {
                writer.Put(durability);
                writer.Put(exp);
            }

            if (GetWeaponItem() != null)
            {
                // Only weapons have an ammo
                writer.Put(ammo);
            }

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

        if (GetPetItem() != null)
        {
            if (isOwnerClient)
            {
                writer.Put(exp);
            }
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        bool isOwnerClient = reader.GetBool();
        if (isOwnerClient)
        {
            id = reader.GetString();
        }

        dataId = reader.GetInt();
        level = reader.GetShort();
        amount = reader.GetShort();
        equipSlotIndex = reader.GetByte();

        if (isOwnerClient)
        {
            lockRemainsDuration = reader.GetFloat();
        }

        // Read only needed data
        if (GetEquipmentItem() != null)
        {
            if (isOwnerClient)
            {
                durability = reader.GetFloat();
                exp = reader.GetInt();
            }

            if (GetWeaponItem() != null)
            {
                // Only weapons have an ammo
                ammo = reader.GetShort();
            }

            byte socketCount = reader.GetByte();
            Sockets.Clear();
            for (byte i = 0; i < socketCount; ++i)
            {
                Sockets.Add(reader.GetInt());
            }
        }

        if (GetPetItem() != null)
        {
            if (isOwnerClient)
            {
                exp = reader.GetInt();
            }
        }
    }
}

[System.Serializable]
public class SyncListCharacterItem : LiteNetLibSyncList<CharacterItem>
{
}
