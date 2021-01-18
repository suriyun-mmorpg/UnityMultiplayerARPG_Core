using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class WorkbenchEntity : BuildingEntity
    {
        [Header("Workbench Settings")]
        public ItemCraft[] itemCrafts;
        public override bool Activatable { get { return true; } }

        private Dictionary<int, ItemCraft> cacheItemCrafts;
        public Dictionary<int, ItemCraft> CacheItemCrafts
        {
            get
            {
                if (cacheItemCrafts == null)
                {
                    cacheItemCrafts = new Dictionary<int, ItemCraft>();
                    foreach (ItemCraft itemCraft in itemCrafts)
                    {
                        if (itemCraft.CraftingItem == null)
                            continue;
                        cacheItemCrafts[itemCraft.CraftingItem.DataId] = itemCraft;
                    }
                }
                return cacheItemCrafts;
            }
        }

        public void CraftItem(BasePlayerCharacterEntity playerCharacterEntity, int dataId)
        {
            ItemCraft itemCraft;
            if (!CacheItemCrafts.TryGetValue(dataId, out itemCraft))
                return;

            UITextKeys gameMessage;
            if (!itemCraft.CanCraft(playerCharacterEntity, out gameMessage))
                GameInstance.ServerGameMessageHandlers.SendGameMessage(playerCharacterEntity.ConnectionId, gameMessage);
            else
                itemCraft.CraftItem(playerCharacterEntity);
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            if (CacheItemCrafts.Count > 0)
            {
                List<BaseItem> items = new List<BaseItem>();
                foreach (ItemCraft itemCraft in CacheItemCrafts.Values)
                {
                    items.Add(itemCraft.CraftingItem);
                    foreach (BaseItem item in itemCraft.CacheCraftRequirements.Keys)
                    {
                        items.Add(item);
                    }
                }
                GameInstance.AddItems(items);
            }
        }
    }
}
