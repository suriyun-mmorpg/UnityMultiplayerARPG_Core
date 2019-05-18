using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class WorkbenchEntity : BuildingEntity
    {
        [Header("Workbench data")]
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

            GameMessage.Type gameMessageType;
            if (!itemCraft.CanCraft(playerCharacterEntity, out gameMessageType))
                gameManager.SendServerGameMessage(playerCharacterEntity.ConnectionId, gameMessageType);
            else
                itemCraft.CraftItem(playerCharacterEntity);
        }
    }
}
