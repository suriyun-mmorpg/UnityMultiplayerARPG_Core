using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Harvestable", menuName = "Create GameData/Harvestable", order = -4796)]
    public partial class Harvestable : BaseGameData
    {
        [Header("Harvestable Configs")]
        public HarvestEffectiveness[] harvestEffectivenesses;
        [Tooltip("Ex. if this is 10 when damage to harvestable entity = 2, character will receives 20 exp")]
        public int expPerDamage;

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
                foreach (HarvestEffectiveness harvestEffectiveness in harvestEffectivenesses)
                {
                    if (harvestEffectiveness.weaponType != null && harvestEffectiveness.damageEffectiveness > 0)
                    {
                        cacheHarvestEffectivenesses[harvestEffectiveness.weaponType] = harvestEffectiveness;
                        Dictionary<ItemDropByWeight, int> harvestItems = new Dictionary<ItemDropByWeight, int>();
                        foreach (ItemDropByWeight item in harvestEffectiveness.items)
                        {
                            if (item.item == null || item.amountPerDamage <= 0 || item.randomWeight <= 0)
                                continue;
                            harvestItems[item] = item.randomWeight;
                        }
                        cacheHarvestItems[harvestEffectiveness.weaponType] = WeightedRandomizer.From(harvestItems);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public struct HarvestEffectiveness
    {
        public WeaponType weaponType;
        [Tooltip("This will multiply with harvest damage amount")]
        [Range(0.1f, 5f)]
        public float damageEffectiveness;
        public ItemDropByWeight[] items;
    }
}
