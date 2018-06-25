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
    public float respawnDuration = 10f;
    public int maxHp = 100;

    [SerializeField]
    private SyncFieldBool isHidding = new SyncFieldBool();
    private float deadTime;

    public bool IsHidding { get { return isHidding.Value; } set { isHidding.Value = value; } }

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

    public override void OnSetup()
    {
        base.OnSetup();
        isHidding.sendOptions = SendOptions.ReliableUnordered;
        isHidding.forOwnerOnly = false;
        isHidding.onChange += OnIsHiddingChange;
    }

    private void OnDestroy()
    {
        isHidding.onChange -= OnIsHiddingChange;
    }

    private void OnIsHiddingChange(bool isHidding)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = !isHidding;
        }
        if (CacheCapsuleCollider != null)
            CacheCapsuleCollider.enabled = !isHidding;
    }

    protected override void Update()
    {
        base.Update();
        if (Time.unscaledTime - deadTime >= respawnDuration)
        {
            CurrentHp = maxHp;
            IsHidding = false;
        }
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
            {
                CurrentHp = 0;
                deadTime = Time.unscaledTime;
                IsHidding = true;
            }
        }
    }
}
