using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct HarvestEffectiveness
    {
        public WeaponType weaponType;
        [Tooltip("This will multiply with harvest damage amount")]
        [Range(0.1f, 5f)]
        public float damageEffectiveness;
        public ItemDropByWeight[] items;
    }

    public sealed class HarvestableEntity : DamageableNetworkEntity
    {
        public HarvestEffectiveness[] harvestEffectivenesses;
        public int maxHp = 100;
        public float colliderDetectionRadius = 2f;
        public float respawnDuration = 5f;

        #region Public data
        [HideInInspector]
        public HarvestableSpawnArea spawnArea;
        [HideInInspector]
        public Vector3 spawnPosition;
        #endregion

        private Dictionary<WeaponType, HarvestEffectiveness> cacheHarvestEffectivenesses;
        public Dictionary<WeaponType, HarvestEffectiveness> CacheHarvestEffectivenesses
        {
            get
            {
                InitCaches();
                return cacheHarvestEffectivenesses;
            }
        }

        private Dictionary<WeaponType, WeightedRandomizer<ItemDropByWeight>> cacheHarvestItems;
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
                {
                    CurrentHp = 0;
                    NetworkDestroy();
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(CacheTransform.position, colliderDetectionRadius);
        }
    }
}
