using System.Collections.Generic;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class CharacterItem
    {
        [System.NonSerialized]
        private int _dirtyDataId;
        [System.NonSerialized]
        private int _dirtyLevel;
        [System.NonSerialized]
        private int _dirtyRandomSeed;

        [System.NonSerialized]
        private BaseItem _cacheItem;
        [System.NonSerialized]
        private IUsableItem _cacheUsableItem;
        [System.NonSerialized]
        private IEquipmentItem _cacheEquipmentItem;
        [System.NonSerialized]
        private IDefendEquipmentItem _cacheDefendItem;
        [System.NonSerialized]
        private IArmorItem _cacheArmorItem;
        [System.NonSerialized]
        private IWeaponItem _cacheWeaponItem;
        [System.NonSerialized]
        private IShieldItem _cacheShieldItem;
        [System.NonSerialized]
        private IPotionItem _cachePotionItem;
        [System.NonSerialized]
        private IAmmoItem _cacheAmmoItem;
        [System.NonSerialized]
        private IBuildingItem _cacheBuildingItem;
        [System.NonSerialized]
        private IPetItem _cachePetItem;
        [System.NonSerialized]
        private ISocketEnhancerItem _cacheSocketEnhancerItem;
        [System.NonSerialized]
        private IMountItem _cacheMountItem;
        [System.NonSerialized]
        private ISkillItem _cacheSkillItem;
        [System.NonSerialized]
        private CalculatedItemBuff _cacheBuff = new CalculatedItemBuff();

        private void MakeCache()
        {
            if (_dirtyDataId == dataId && _dirtyLevel == level && _dirtyRandomSeed == randomSeed)
                return;
            _dirtyDataId = dataId;
            _dirtyLevel = level;
            _dirtyRandomSeed = randomSeed;
            _cacheItem = null;
            _cacheUsableItem = null;
            _cacheEquipmentItem = null;
            _cacheDefendItem = null;
            _cacheArmorItem = null;
            _cacheWeaponItem = null;
            _cacheShieldItem = null;
            _cachePotionItem = null;
            _cacheAmmoItem = null;
            _cacheBuildingItem = null;
            _cachePetItem = null;
            _cacheSocketEnhancerItem = null;
            _cacheMountItem = null;
            _cacheSkillItem = null;
            if (GameInstance.Items.TryGetValue(dataId, out _cacheItem) && _cacheItem != null)
            {
                if (_cacheItem.IsUsable())
                    _cacheUsableItem = _cacheItem as IUsableItem;
                if (_cacheItem.IsEquipment())
                    _cacheEquipmentItem = _cacheItem as IEquipmentItem;
                if (_cacheItem.IsDefendEquipment())
                    _cacheDefendItem = _cacheItem as IDefendEquipmentItem;
                if (_cacheItem.IsArmor())
                    _cacheArmorItem = _cacheItem as IArmorItem;
                if (_cacheItem.IsWeapon())
                    _cacheWeaponItem = _cacheItem as IWeaponItem;
                if (_cacheItem.IsShield())
                    _cacheShieldItem = _cacheItem as IShieldItem;
                if (_cacheItem.IsPotion())
                    _cachePotionItem = _cacheItem as IPotionItem;
                if (_cacheItem.IsAmmo())
                    _cacheAmmoItem = _cacheItem as IAmmoItem;
                if (_cacheItem.IsBuilding())
                    _cacheBuildingItem = _cacheItem as IBuildingItem;
                if (_cacheItem.IsPet())
                    _cachePetItem = _cacheItem as IPetItem;
                if (_cacheItem.IsSocketEnhancer())
                    _cacheSocketEnhancerItem = _cacheItem as ISocketEnhancerItem;
                if (_cacheItem.IsMount())
                    _cacheMountItem = _cacheItem as IMountItem;
                if (_cacheItem.IsSkill())
                    _cacheSkillItem = _cacheItem as ISkillItem;
            }
            _cacheBuff.Build(_cacheEquipmentItem, level, randomSeed);
        }

        public BaseItem GetItem()
        {
            MakeCache();
            return _cacheItem;
        }

        public IUsableItem GetUsableItem()
        {
            MakeCache();
            return _cacheUsableItem;
        }

        public IEquipmentItem GetEquipmentItem()
        {
            MakeCache();
            return _cacheEquipmentItem;
        }

        public IDefendEquipmentItem GetDefendItem()
        {
            MakeCache();
            return _cacheDefendItem;
        }

        public IArmorItem GetArmorItem()
        {
            MakeCache();
            return _cacheArmorItem;
        }

        public IWeaponItem GetWeaponItem()
        {
            MakeCache();
            return _cacheWeaponItem;
        }

        public IShieldItem GetShieldItem()
        {
            MakeCache();
            return _cacheShieldItem;
        }

        public IPotionItem GetPotionItem()
        {
            MakeCache();
            return _cachePotionItem;
        }

        public IAmmoItem GetAmmoItem()
        {
            MakeCache();
            return _cacheAmmoItem;
        }

        public IBuildingItem GetBuildingItem()
        {
            MakeCache();
            return _cacheBuildingItem;
        }

        public IPetItem GetPetItem()
        {
            MakeCache();
            return _cachePetItem;
        }

        public ISocketEnhancerItem GetSocketEnhancerItem()
        {
            MakeCache();
            return _cacheSocketEnhancerItem;
        }

        public IMountItem GetMountItem()
        {
            MakeCache();
            return _cacheMountItem;
        }

        public ISkillItem GetSkillItem()
        {
            MakeCache();
            return _cacheSkillItem;
        }

        public int GetMaxStack()
        {
            return GetItem() == null ? 0 : GetItem().MaxStack;
        }

        public float GetMaxDurability()
        {
            return GetEquipmentItem() == null ? 0f : GetEquipmentItem().MaxDurability;
        }

        public bool IsFull()
        {
            return amount == GetMaxStack();
        }

        public bool IsBroken()
        {
            return GetMaxDurability() > 0 && durability <= 0;
        }

        public bool IsLocked()
        {
            return lockRemainsDuration > 0;
        }

        public bool IsAmmoEmpty()
        {
            IWeaponItem item = GetWeaponItem();
            if (item != null && item.AmmoCapacity > 0)
                return ammo == 0;
            return false;
        }

        public bool IsAmmoFull()
        {
            IWeaponItem item = GetWeaponItem();
            if (item != null && item.AmmoCapacity > 0)
                return ammo >= item.AmmoCapacity;
            return true;
        }

        public bool HasAmmoToReload(ICharacterData character)
        {
            IWeaponItem item = GetWeaponItem();
            if (item != null)
                return character.CountAmmos(item.WeaponType.RequireAmmoType) > 0;
            return false;
        }

        public void Lock(float duration)
        {
            lockRemainsDuration = duration;
        }

        public bool ShouldRemove(long currentTime)
        {
            return expireTime > 0 && expireTime < currentTime;
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
            IDefendEquipmentItem item = GetDefendItem();
            if (item == null)
                return new KeyValuePair<DamageElement, float>();
            return item.GetArmorAmount(level, GetEquipmentStatsRate());
        }

        public KeyValuePair<DamageElement, MinMaxFloat> GetDamageAmount(ICharacterData characterData)
        {
            IWeaponItem item = GetWeaponItem();
            if (item == null)
                return new KeyValuePair<DamageElement, MinMaxFloat>();
            return item.GetDamageAmount(level, GetEquipmentStatsRate(), characterData);
        }

        public KeyValuePair<DamageElement, MinMaxFloat> GetPureDamageAmount()
        {
            IWeaponItem item = GetWeaponItem();
            if (item == null)
                return new KeyValuePair<DamageElement, MinMaxFloat>();
            return item.GetDamageAmount(level, GetEquipmentStatsRate(), 1f);
        }

        public float GetWeaponDamageBattlePoints()
        {
            if (GetWeaponItem() == null)
                return 0f;
            KeyValuePair<DamageElement, MinMaxFloat> kv = GetPureDamageAmount();
            DamageElement tempDamageElement = kv.Key;
            if (tempDamageElement == null)
                tempDamageElement = GameInstance.Singleton.DefaultDamageElement;
            MinMaxFloat amount = kv.Value;
            return tempDamageElement.DamageBattlePointScore * (amount.min + amount.max) * 0.5f;
        }

        public CalculatedItemBuff GetBuff()
        {
            MakeCache();
            return _cacheBuff;
        }

        public CharacterStats GetSocketsIncreaseStats()
        {
            if (GetEquipmentItem() == null || Sockets.Count == 0)
                return CharacterStats.Empty;
            CharacterStats result = new CharacterStats();
            BaseItem tempEnhancer;
            foreach (int socketId in Sockets)
            {
                if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
                    result += (tempEnhancer as ISocketEnhancerItem).SocketEnhanceEffect.stats;
            }
            return result;
        }

        public CharacterStats GetSocketsIncreaseStatsRate()
        {
            if (GetEquipmentItem() == null || Sockets.Count == 0)
                return CharacterStats.Empty;
            CharacterStats result = new CharacterStats();
            BaseItem tempEnhancer;
            foreach (int socketId in Sockets)
            {
                if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
                    result += (tempEnhancer as ISocketEnhancerItem).SocketEnhanceEffect.statsRate;
            }
            return result;
        }

        public Dictionary<Attribute, float> GetSocketsIncreaseAttributes()
        {
            if (GetEquipmentItem() == null || Sockets.Count == 0)
                return null;
            Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
            BaseItem tempEnhancer;
            foreach (int socketId in Sockets)
            {
                if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
                    result = GameDataHelpers.CombineAttributes((tempEnhancer as ISocketEnhancerItem).SocketEnhanceEffect.attributes, result, 1f);
            }
            return result;
        }

        public Dictionary<Attribute, float> GetSocketsIncreaseAttributesRate()
        {
            if (GetEquipmentItem() == null || Sockets.Count == 0)
                return null;
            Dictionary<Attribute, float> result = new Dictionary<Attribute, float>();
            BaseItem tempEnhancer;
            foreach (int socketId in Sockets)
            {
                if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
                    result = GameDataHelpers.CombineAttributes((tempEnhancer as ISocketEnhancerItem).SocketEnhanceEffect.attributesRate, result, 1f);
            }
            return result;
        }

        public Dictionary<DamageElement, float> GetSocketsIncreaseResistances()
        {
            if (GetEquipmentItem() == null || Sockets.Count == 0)
                return null;
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            BaseItem tempEnhancer;
            foreach (int socketId in Sockets)
            {
                if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
                    result = GameDataHelpers.CombineResistances((tempEnhancer as ISocketEnhancerItem).SocketEnhanceEffect.resistances, result, 1f);
            }
            return result;
        }

        public Dictionary<DamageElement, float> GetSocketsIncreaseArmors()
        {
            if (GetEquipmentItem() == null || Sockets.Count == 0)
                return null;
            Dictionary<DamageElement, float> result = new Dictionary<DamageElement, float>();
            BaseItem tempEnhancer;
            foreach (int socketId in Sockets)
            {
                if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
                    result = GameDataHelpers.CombineArmors((tempEnhancer as ISocketEnhancerItem).SocketEnhanceEffect.armors, result, 1f);
            }
            return result;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetSocketsIncreaseDamages()
        {
            if (GetEquipmentItem() == null || Sockets.Count == 0)
                return null;
            Dictionary<DamageElement, MinMaxFloat> result = new Dictionary<DamageElement, MinMaxFloat>();
            BaseItem tempEnhancer;
            foreach (int socketId in Sockets)
            {
                if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
                    result = GameDataHelpers.CombineDamages((tempEnhancer as ISocketEnhancerItem).SocketEnhanceEffect.damages, result, 1f);
            }
            return result;
        }

        public Dictionary<BaseSkill, int> GetSocketsIncreaseSkills()
        {
            if (GetEquipmentItem() == null || Sockets.Count == 0)
                return null;
            Dictionary<BaseSkill, int> result = new Dictionary<BaseSkill, int>();
            BaseItem tempEnhancer;
            foreach (int socketId in Sockets)
            {
                if (GameInstance.Items.TryGetValue(socketId, out tempEnhancer))
                    result = GameDataHelpers.CombineSkills((tempEnhancer as ISocketEnhancerItem).SocketEnhanceEffect.skills, result, 1f);
            }
            return result;
        }

        public static CharacterItem Create(BaseItem item, int level = 1, int amount = 1, int? randomSeed = null)
        {
            return Create(item.DataId, level, amount, randomSeed);
        }

        public static CharacterItem Create(int dataId, int level = 1, int amount = 1, int? randomSeed = null)
        {
            CharacterItem newItem = new CharacterItem();
            newItem.id = GenericUtils.GetUniqueId();
            newItem.dataId = dataId;
            if (level <= 0)
                level = 1;
            newItem.level = level;
            newItem.amount = amount;
            newItem.durability = 0f;
            newItem.exp = 0;
            newItem.lockRemainsDuration = 0f;
            newItem.ammo = 0;
            if (GameInstance.Items.TryGetValue(dataId, out BaseItem tempItem))
            {
                if (tempItem.IsEquipment())
                {
                    newItem.durability = (tempItem as IEquipmentItem).MaxDurability;
                    newItem.lockRemainsDuration = tempItem.LockDuration;
                    if (randomSeed.HasValue)
                        newItem.randomSeed = randomSeed.Value;
                    else
                        newItem.randomSeed = GenericUtils.RandomInt(int.MinValue, int.MaxValue);
                }
                if (tempItem.ExpireDuration > 0)
                {
                    newItem.expireTime = System.DateTimeOffset.Now.ToUnixTimeSeconds() + (tempItem.ExpireDuration * 60 * 60);
                }
            }
            return newItem;
        }

        public static CharacterItem CreateEmptySlot()
        {
            return Create(0, 1, 0);
        }
    }

    [System.Serializable]
    public class SyncListCharacterItem : LiteNetLibSyncList<CharacterItem>
    {
    }
}
