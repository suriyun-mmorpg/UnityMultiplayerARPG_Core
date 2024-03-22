using System.Collections.Generic;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial struct CharacterItem
    {
        public const byte CURRENT_VERSION = 2;

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
        private CalculatedItemBuff _cacheBuff/* = new CalculatedItemBuff()*/;
        [System.NonSerialized]
        private bool _recachingBuff/* = false*/;
        /*
        ~CharacterItem()
        {
            ClearCachedData();
            _cacheBuff = null;
        }
        */
        private void ClearCachedData()
        {
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
        }

        private bool IsRecaching()
        {
            return _dirtyDataId != dataId || _dirtyLevel != level || _dirtyRandomSeed != randomSeed;
        }

        private void MakeAsCached()
        {
            _dirtyDataId = dataId;
            _dirtyLevel = level;
            _dirtyRandomSeed = randomSeed;
        }

        private void MakeCache()
        {
            if (!IsRecaching())
                return;
            MakeAsCached();
            ClearCachedData();
            _recachingBuff = true;
            if (!GameInstance.Items.TryGetValue(dataId, out _cacheItem) || _cacheItem == null)
                return;
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

        public bool GetDestroyIfBroken()
        {
            return GetEquipmentItem() == null ? false : GetEquipmentItem().DestroyIfBroken;
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
                return character.CountAllAmmos(item.WeaponType.AmmoType) > 0;
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

        public KeyValuePair<DamageElement, MinMaxFloat> GetDamageAmount()
        {
            IWeaponItem item = GetWeaponItem();
            if (item == null)
                return new KeyValuePair<DamageElement, MinMaxFloat>();
            return item.GetDamageAmount(level, GetEquipmentStatsRate());
        }

        public float GetWeaponDamageBattlePoints()
        {
            if (GetWeaponItem() == null)
                return 0f;
            KeyValuePair<DamageElement, MinMaxFloat> kv = GetDamageAmount();
            DamageElement tempDamageElement = kv.Key;
            if (tempDamageElement == null)
                tempDamageElement = GameInstance.Singleton.DefaultDamageElement;
            MinMaxFloat amount = kv.Value;
            return tempDamageElement.DamageBattlePointScore * (amount.min + amount.max) * 0.5f;
        }

        public CalculatedItemBuff GetBuff()
        {
            MakeCache();
            if (_recachingBuff)
            {
                _recachingBuff = false;
                _cacheBuff.Build(_cacheEquipmentItem, level, randomSeed, version);
            }
            return _cacheBuff;
        }

        public void UpdateDurability(ICharacterData characterData, float amount)
        {
            float oldDurability = durability;
            float max = GetMaxDurability();
            bool destroying = false;
            durability += amount;
            if (durability > max)
                durability = max;
            if (durability <= 0)
            {
                durability = 0;
                if (GetDestroyIfBroken())
                    destroying = true;
            }
            if (characterData is IPlayerCharacterData playerCharacterData)
            {
                // Will write log messages only if character is player character
                GameInstance.ServerLogHandlers.LogItemDurabilityChanged(playerCharacterData, this, oldDurability, durability, destroying);
            }
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
            newItem.sockets = new List<int>();
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
                    switch (tempItem.ExpireDurationUnit)
                    {
                        case ETimeUnits.Days:
                            newItem.expireTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (tempItem.ExpireDuration * 60 * 60 * 24);
                            break;
                        case ETimeUnits.Hours:
                            newItem.expireTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (tempItem.ExpireDuration * 60 * 60);
                            break;
                        case ETimeUnits.Minutes:
                            newItem.expireTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (tempItem.ExpireDuration * 60);
                            break;
                        case ETimeUnits.Seconds:
                            newItem.expireTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + tempItem.ExpireDuration;
                            break;
                    }
                }
            }
            newItem.version = CURRENT_VERSION;
            return newItem;
        }

        public static CharacterItem CreateEmptySlot()
        {
            return Create(0, 1, 0);
        }

        public static CharacterItem CreateDefaultWeapon()
        {
            return Create(GameInstance.Singleton.DefaultWeaponItem.DataId, 1, 1, 0);
        }
    }

    [System.Serializable]
    public class SyncListCharacterItem : LiteNetLibSyncList<CharacterItem>
    {
    }
}
