using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class PlayerCharacterCraftingComponent : BaseGameEntityComponent<BasePlayerCharacterEntity>
    {
        [SerializeField]
        private int maxQueueSize = 8;
        private SyncListCraftingQueueItem craftingQueueItems = new SyncListCraftingQueueItem();
        private float timeCounter;

        public IList<CraftingQueueItem> CraftingItems
        {
            get { return craftingQueueItems; }
        }

        public override sealed void OnSetup()
        {
            base.OnSetup();
            craftingQueueItems.forOwnerOnly = true;
        }

        public override sealed void EntityUpdate()
        {
            base.EntityUpdate();
            if (CacheEntity.IsDead())
            {
                if (craftingQueueItems.Count > 0)
                    craftingQueueItems.Clear();
                return;
            }

            if (craftingQueueItems.Count == 0)
            {
                timeCounter = 0f;
                return;
            }

            CraftingQueueItem craftingItem = craftingQueueItems[0];
            ItemCraftFormula formula = GameInstance.ItemCraftFormulas[craftingItem.dataId];
            UITextKeys errorMessage;
            if (!formula.ItemCraft.CanCraft(CacheEntity, out errorMessage))
            {
                timeCounter = 0f;
                craftingQueueItems.RemoveAt(0);
                GameInstance.ServerGameMessageHandlers.SendGameMessage(CacheEntity.ConnectionId, errorMessage);
                return;
            }

            timeCounter += Time.unscaledDeltaTime;
            if (timeCounter >= 1f)
            {
                craftingItem.craftRemainsDuration -= timeCounter;
                timeCounter = 0f;
                if (craftingItem.craftRemainsDuration <= 0f)
                {
                    // Reduce items and add crafting item
                    formula.ItemCraft.CraftItem(CacheEntity);
                    // Reduce amount
                    if (craftingItem.amount > 1)
                    {
                        --craftingItem.amount;
                        craftingItem.craftRemainsDuration = formula.CraftDuration;
                        craftingQueueItems[0] = craftingItem;
                    }
                    else
                    {
                        craftingQueueItems.RemoveAt(0);
                    }
                }
                else
                {
                    // Update remains duration
                    craftingQueueItems[0] = craftingItem;
                }
            }
        }

        public void AppendCraftingQueueItem(int dataId, short amount)
        {
            RPC(RpcAppendCraftingQueueItem, dataId, amount);
        }

        public void ChangeCraftingQueueItem(int index, short amount)
        {
            RPC(RpcChangeCraftingQueueItem, index, amount);
        }

        public void CancelCraftingQueueItem(int index)
        {
            RPC(RpcCancelCraftingQueueItem, index);
        }

        [ServerRpc]
        private void RpcAppendCraftingQueueItem(int dataId, short amount)
        {
            if (CacheEntity.IsDead())
                return;
            ItemCraftFormula itemCraftFormula;
            if (!GameInstance.ItemCraftFormulas.TryGetValue(dataId, out itemCraftFormula))
                return;
            if (craftingQueueItems.Count >= maxQueueSize)
                return;
            craftingQueueItems.Add(new CraftingQueueItem()
            {
                dataId = dataId,
                amount = amount,
                craftRemainsDuration = itemCraftFormula.CraftDuration,
            });
        }

        [ServerRpc]
        private void RpcChangeCraftingQueueItem(int index, short amount)
        {
            if (CacheEntity.IsDead())
                return;
            if (index < 0 || index >= craftingQueueItems.Count)
                return;
            if (amount <= 0)
            {
                craftingQueueItems.RemoveAt(index);
                return;
            }
            CraftingQueueItem craftingItem = craftingQueueItems[index];
            craftingItem.amount = amount;
            craftingQueueItems[index] = craftingItem;
        }

        [ServerRpc]
        private void RpcCancelCraftingQueueItem(int index)
        {
            if (CacheEntity.IsDead())
                return;
            if (index < 0 || index >= craftingQueueItems.Count)
                return;
            craftingQueueItems.RemoveAt(index);
        }
    }
}
