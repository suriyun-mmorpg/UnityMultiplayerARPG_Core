using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class QueuedWorkbenchEntity : BuildingEntity, ICraftingQueueSource
    {
        [Header("Workbench Settings")]
        [SerializeField]
        private ItemCraftFormula[] itemCraftFormulas;
        [SerializeField]
        private int maxQueueSize = 5;
        [SerializeField]
        private float craftingDistance = 5f;
        private SyncListCraftingQueueItem queueItems = new SyncListCraftingQueueItem();
        public override bool Activatable { get { return true; } }

        private Dictionary<int, ItemCraftFormula> cacheItemCraftFormulas;
        public Dictionary<int, ItemCraftFormula> CacheItemCraftFormulas
        {
            get
            {
                if (cacheItemCraftFormulas == null)
                {
                    cacheItemCraftFormulas = new Dictionary<int, ItemCraftFormula>();
                    foreach (ItemCraftFormula itemCraftFormula in itemCraftFormulas)
                    {
                        if (itemCraftFormula.ItemCraft.CraftingItem == null)
                            continue;
                        cacheItemCraftFormulas[itemCraftFormula.DataId] = itemCraftFormula;
                    }
                }
                return cacheItemCraftFormulas;
            }
        }

        public SyncListCraftingQueueItem QueueItems
        {
            get { return queueItems; }
        }

        public int MaxQueueSize
        {
            get { return maxQueueSize; }
        }

        public bool CanCraft
        {
            get { return !this.IsDead(); }
        }

        public float TimeCounter { get; set; }

        public int SourceId
        {
            get { return Identity.HashAssetId; }
        }

        public override sealed void OnSetup()
        {
            base.OnSetup();
            queueItems.forOwnerOnly = false;
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            this.UpdateQueue(craftingDistance, CacheTransform.position);
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            if (CacheItemCraftFormulas.Count > 0)
            {
                List<BaseItem> items = new List<BaseItem>();
                foreach (ItemCraftFormula itemCraftFormula in CacheItemCraftFormulas.Values)
                {
                    items.Add(itemCraftFormula.ItemCraft.CraftingItem);
                    items.AddRange(itemCraftFormula.ItemCraft.CacheCraftRequirements.Keys);
                }
                GameInstance.AddItems(items);
                GameInstance.AddItemCraftFormulas(SourceId, CacheItemCraftFormulas.Values);
            }
        }
    }
}
