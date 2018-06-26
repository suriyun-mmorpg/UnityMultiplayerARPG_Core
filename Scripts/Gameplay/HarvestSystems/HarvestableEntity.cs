using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

[System.Serializable]
public struct HarvestEffectiveness
{
    public WeaponType weaponType;
    public float damageEffectiveness;
    public ItemDropByWeight[] items;
}

[RequireComponent(typeof(CapsuleCollider))]
public sealed class HarvestableEntity : DamageableNetworkEntity
{
    public HarvestEffectiveness[] harvestEffectivenesses;
    public int maxHp = 100;
    
    private float deadTime;

    public override string Title
    {
        get { return title; }
    }

    private CapsuleCollider cacheCapsuleCollider;
    public CapsuleCollider CacheCapsuleCollider
    {
        get
        {
            if (cacheCapsuleCollider == null)
                cacheCapsuleCollider = GetComponent<CapsuleCollider>();
            return cacheCapsuleCollider;
        }
    }
    private Dictionary<WeaponType, HarvestEffectiveness> cacheHarvestEffectivenesses;
    private Dictionary<WeaponType, WeightedRandomizer<ItemDropByWeight>> cacheHarvestItems;
    
    public Dictionary<WeaponType, HarvestEffectiveness> CacheHarvestEffectivenesses
    {
        get
        {
            InitCaches();
            return cacheHarvestEffectivenesses;
        }
    }

    public Dictionary<WeaponType, WeightedRandomizer<ItemDropByWeight>> CacheHarvestItems
    {
        get
        {
            InitCaches();
            return cacheHarvestItems;
        }
    }

    private void InitCaches()
    {
        if (cacheHarvestEffectivenesses == null || cacheHarvestItems == null)
        {
            cacheHarvestEffectivenesses = new Dictionary<WeaponType, HarvestEffectiveness>();
            cacheHarvestItems = new Dictionary<WeaponType, WeightedRandomizer<ItemDropByWeight>>();
            foreach (var harvestEffectiveness in harvestEffectivenesses)
            {
                if (harvestEffectiveness.weaponType != null && harvestEffectiveness.damageEffectiveness > 0)
                {
                    cacheHarvestEffectivenesses[harvestEffectiveness.weaponType] = harvestEffectiveness;
                    var harvestItems = new Dictionary<ItemDropByWeight, int>();
                    foreach (var item in harvestEffectiveness.items)
                    {
                        if (item.item == null || item.amount <= 0 || item.weight <= 0)
                            continue;
                        harvestItems[item] = item.weight;
                    }
                    cacheHarvestItems[harvestEffectiveness.weaponType] = WeightedRandomizer.From(harvestItems);
                }
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        gameObject.tag = gameInstance.harvestableTag;
        gameObject.layer = gameInstance.harvestableLayer;
    }

    public override void ReceiveDamage(BaseCharacterEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, int hitEffectsId)
    {
        if (!IsServer || CurrentHp <= 0 || weapon == null)
            return;

        var weaponItem = weapon.GetWeaponItem();
        HarvestEffectiveness harvestEffectiveness;
        WeightedRandomizer<ItemDropByWeight> itemRandomizer;
        if (CacheHarvestEffectivenesses.TryGetValue(weaponItem.weaponType, out harvestEffectiveness) &&
            CacheHarvestItems.TryGetValue(weaponItem.weaponType, out itemRandomizer))
        {
            var totalDamage = (int)(weaponItem.harvestDamageAmount.GetAmount(weapon.level).Random() * harvestEffectiveness.damageEffectiveness);
            var receivingItem = itemRandomizer.TakeOne();
            var dataId = receivingItem.item.DataId;
            var amount = (short)(receivingItem.amount * totalDamage);
            if (!attacker.IncreasingItemsWillOverwhelming(dataId, amount))
                attacker.IncreaseItems(dataId, 1, amount);
            CurrentHp -= totalDamage;
            if (CurrentHp <= 0)
                NetworkDestroy();
        }
    }
}
