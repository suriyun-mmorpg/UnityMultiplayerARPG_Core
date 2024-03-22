using System.Collections.Generic;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial struct CharacterItem
    {
        public const byte CURRENT_VERSION = 2;

        public BaseItem GetItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetItem(this);
        }

        public IUsableItem GetUsableItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetUsableItem(this);
        }

        public IEquipmentItem GetEquipmentItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetEquipmentItem(this);
        }

        public IDefendEquipmentItem GetDefendItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetDefendItem(this);
        }

        public IArmorItem GetArmorItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetArmorItem(this);
        }

        public IWeaponItem GetWeaponItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetWeaponItem(this);
        }

        public IShieldItem GetShieldItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetShieldItem(this);
        }

        public IPotionItem GetPotionItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetPotionItem(this);
        }

        public IAmmoItem GetAmmoItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetAmmoItem(this);
        }

        public IBuildingItem GetBuildingItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetBuildingItem(this);
        }

        public IPetItem GetPetItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetPetItem(this);
        }

        public ISocketEnhancerItem GetSocketEnhancerItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetSocketEnhancerItem(this);
        }

        public IMountItem GetMountItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetMountItem(this);
        }

        public ISkillItem GetSkillItem()
        {
            return CharacterRelatesDataCacheManager.CharacterItems.GetSkillItem(this);
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
            return CharacterRelatesDataCacheManager.CharacterItems.GetBuff(this);
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
