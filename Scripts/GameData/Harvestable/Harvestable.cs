using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.HARVESTABLE_FILE, menuName = GameDataMenuConsts.HARVESTABLE_MENU, order = GameDataMenuConsts.HARVESTABLE_ORDER)]
    public partial class Harvestable : BaseGameData
    {
        [Category("Harvestable Settings")]
        public HarvestEffectiveness[] harvestEffectivenesses;
        public SkillHarvestEffectiveness[] skillHarvestEffectivenesses;
        [Tooltip("Ex. if this is 10 when damage to harvestable entity = 2, character will receives 20 exp")]
        public int expPerDamage;

        [System.NonSerialized]
        private Dictionary<WeaponType, HarvestEffectiveness> cacheHarvestEffectivenesses;
        public Dictionary<WeaponType, HarvestEffectiveness> CacheHarvestEffectivenesses
        {
            get
            {
                InitCaches();
                return cacheHarvestEffectivenesses;
            }
        }

        [System.NonSerialized]
        private Dictionary<WeaponType, WeightedRandomizer<ItemDropByWeight>> cacheHarvestItems;
        public Dictionary<WeaponType, WeightedRandomizer<ItemDropByWeight>> CacheHarvestItems
        {
            get
            {
                InitCaches();
                return cacheHarvestItems;
            }
        }

        [System.NonSerialized]
        private Dictionary<BaseSkill, SkillHarvestEffectiveness> cacheSkillHarvestEffectivenesses;
        public Dictionary<BaseSkill, SkillHarvestEffectiveness> CacheSkillHarvestEffectivenesses
        {
            get
            {
                InitCaches();
                return cacheSkillHarvestEffectivenesses;
            }
        }

        [System.NonSerialized]
        private Dictionary<BaseSkill, WeightedRandomizer<ItemDropByWeight>> cacheSkillHarvestItems;
        public Dictionary<BaseSkill, WeightedRandomizer<ItemDropByWeight>> CacheSkillHarvestItems
        {
            get
            {
                InitCaches();
                return cacheSkillHarvestItems;
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
            if (cacheSkillHarvestEffectivenesses == null || cacheSkillHarvestItems == null)
            {
                cacheSkillHarvestEffectivenesses = new Dictionary<BaseSkill, SkillHarvestEffectiveness>();
                cacheSkillHarvestItems = new Dictionary<BaseSkill, WeightedRandomizer<ItemDropByWeight>>();
                foreach (SkillHarvestEffectiveness skillHarvestEffectiveness in skillHarvestEffectivenesses)
                {
                    if (skillHarvestEffectiveness.skill != null && skillHarvestEffectiveness.damageEffectiveness > 0)
                    {
                        cacheSkillHarvestEffectivenesses[skillHarvestEffectiveness.skill] = skillHarvestEffectiveness;
                        Dictionary<ItemDropByWeight, int> harvestItems = new Dictionary<ItemDropByWeight, int>();
                        foreach (ItemDropByWeight item in skillHarvestEffectiveness.items)
                        {
                            if (item.item == null || item.amountPerDamage <= 0 || item.randomWeight <= 0)
                                continue;
                            harvestItems[item] = item.randomWeight;
                        }
                        cacheSkillHarvestItems[skillHarvestEffectiveness.skill] = WeightedRandomizer.From(harvestItems);
                    }
                }
            }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            if (harvestEffectivenesses != null && harvestEffectivenesses.Length > 0)
            {
                foreach (HarvestEffectiveness harvestEffectiveness in harvestEffectivenesses)
                {
                    GameInstance.AddItems(harvestEffectiveness.items);
                }
            }
            if (skillHarvestEffectivenesses != null && skillHarvestEffectivenesses.Length > 0)
            {
                foreach (SkillHarvestEffectiveness skillHarvestEffectiveness in skillHarvestEffectivenesses)
                {
                    GameInstance.AddSkills(skillHarvestEffectiveness.skill);
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
        [ArrayElementTitle("item")]
        public ItemDropByWeight[] items;
    }

    [System.Serializable]
    public struct SkillHarvestEffectiveness
    {
        public BaseSkill skill;
        [Tooltip("This will multiply with harvest damage amount")]
        [Range(0.1f, 5f)]
        public float damageEffectiveness;
        [ArrayElementTitle("item")]
        public ItemDropByWeight[] items;
    }
}
