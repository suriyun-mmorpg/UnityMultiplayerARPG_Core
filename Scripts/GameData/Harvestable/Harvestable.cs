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
        private Dictionary<WeaponType, HarvestEffectiveness> _cacheHarvestEffectivenesses;
        public Dictionary<WeaponType, HarvestEffectiveness> CacheHarvestEffectivenesses
        {
            get
            {
                InitCaches();
                return _cacheHarvestEffectivenesses;
            }
        }

        [System.NonSerialized]
        private Dictionary<WeaponType, WeightedRandomizer<ItemDropForHarvestable>> _cacheHarvestItems;
        public Dictionary<WeaponType, WeightedRandomizer<ItemDropForHarvestable>> CacheHarvestItems
        {
            get
            {
                InitCaches();
                return _cacheHarvestItems;
            }
        }

        [System.NonSerialized]
        private Dictionary<BaseSkill, SkillHarvestEffectiveness> _cacheSkillHarvestEffectivenesses;
        public Dictionary<BaseSkill, SkillHarvestEffectiveness> CacheSkillHarvestEffectivenesses
        {
            get
            {
                InitCaches();
                return _cacheSkillHarvestEffectivenesses;
            }
        }

        [System.NonSerialized]
        private Dictionary<BaseSkill, WeightedRandomizer<ItemDropForHarvestable>> _cacheSkillHarvestItems;
        public Dictionary<BaseSkill, WeightedRandomizer<ItemDropForHarvestable>> CacheSkillHarvestItems
        {
            get
            {
                InitCaches();
                return _cacheSkillHarvestItems;
            }
        }

        private void InitCaches()
        {
            if (_cacheHarvestEffectivenesses == null || _cacheHarvestItems == null)
            {
                _cacheHarvestEffectivenesses = new Dictionary<WeaponType, HarvestEffectiveness>();
                _cacheHarvestItems = new Dictionary<WeaponType, WeightedRandomizer<ItemDropForHarvestable>>();
                foreach (HarvestEffectiveness harvestEffectiveness in harvestEffectivenesses)
                {
                    if (harvestEffectiveness.weaponType != null && harvestEffectiveness.damageEffectiveness > 0)
                    {
                        _cacheHarvestEffectivenesses[harvestEffectiveness.weaponType] = harvestEffectiveness;
                        Dictionary<ItemDropForHarvestable, int> harvestItems = new Dictionary<ItemDropForHarvestable, int>();
                        foreach (ItemDropForHarvestable item in harvestEffectiveness.items)
                        {
                            if (item.item == null || item.amountPerDamage <= 0 || item.randomWeight <= 0)
                                continue;
                            harvestItems[item] = item.randomWeight;
                        }
                        _cacheHarvestItems[harvestEffectiveness.weaponType] = WeightedRandomizer.From(harvestItems);
                    }
                }
            }
            if (_cacheSkillHarvestEffectivenesses == null || _cacheSkillHarvestItems == null)
            {
                _cacheSkillHarvestEffectivenesses = new Dictionary<BaseSkill, SkillHarvestEffectiveness>();
                _cacheSkillHarvestItems = new Dictionary<BaseSkill, WeightedRandomizer<ItemDropForHarvestable>>();
                foreach (SkillHarvestEffectiveness skillHarvestEffectiveness in skillHarvestEffectivenesses)
                {
                    if (skillHarvestEffectiveness.skill != null && skillHarvestEffectiveness.damageEffectiveness > 0)
                    {
                        _cacheSkillHarvestEffectivenesses[skillHarvestEffectiveness.skill] = skillHarvestEffectiveness;
                        Dictionary<ItemDropForHarvestable, int> harvestItems = new Dictionary<ItemDropForHarvestable, int>();
                        foreach (ItemDropForHarvestable item in skillHarvestEffectiveness.items)
                        {
                            if (item.item == null || item.amountPerDamage <= 0 || item.randomWeight <= 0)
                                continue;
                            harvestItems[item] = item.randomWeight;
                        }
                        _cacheSkillHarvestItems[skillHarvestEffectiveness.skill] = WeightedRandomizer.From(harvestItems);
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
        public ItemDropForHarvestable[] items;
    }

    [System.Serializable]
    public struct SkillHarvestEffectiveness
    {
        public BaseSkill skill;
        [Tooltip("This will multiply with harvest damage amount")]
        [Range(0.1f, 5f)]
        public float damageEffectiveness;
        [ArrayElementTitle("item")]
        public ItemDropForHarvestable[] items;
    }
}
