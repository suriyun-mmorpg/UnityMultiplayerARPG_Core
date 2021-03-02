using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class PlayerCharacterCraftingComponent : BaseGameEntityComponent<BasePlayerCharacterEntity>
    {
        [SerializeField]
        private int maxQueueSize = 5;
        private SyncListCraftingQueueItem queueItems = new SyncListCraftingQueueItem();
        private float timeCounter;

        public SyncListCraftingQueueItem QueueItems
        {
            get { return queueItems; }
        }

        public override sealed void OnSetup()
        {
            base.OnSetup();
            queueItems.forOwnerOnly = true;
        }

        public override sealed void EntityUpdate()
        {
            base.EntityUpdate();
            if (Entity.IsDead())
            {
                if (queueItems.Count > 0)
                    queueItems.Clear();
                return;
            }

            if (queueItems.Count == 0)
            {
                timeCounter = 0f;
                return;
            }

            CraftingQueueItem craftingItem = queueItems[0];
            ItemCraftFormula formula = GameInstance.ItemCraftFormulas[craftingItem.dataId];
            UITextKeys errorMessage;
            if (!formula.ItemCraft.CanCraft(Entity, out errorMessage))
            {
                timeCounter = 0f;
                queueItems.RemoveAt(0);
                GameInstance.ServerGameMessageHandlers.SendGameMessage(Entity.ConnectionId, errorMessage);
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
                    formula.ItemCraft.CraftItem(Entity);
                    // Reduce amount
                    if (craftingItem.amount > 1)
                    {
                        --craftingItem.amount;
                        craftingItem.craftRemainsDuration = formula.CraftDuration;
                        queueItems[0] = craftingItem;
                    }
                    else
                    {
                        queueItems.RemoveAt(0);
                    }
                }
                else
                {
                    // Update remains duration
                    queueItems[0] = craftingItem;
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
            if (Entity.IsDead())
                return;
            ItemCraftFormula itemCraftFormula;
            if (!GameInstance.ItemCraftFormulas.TryGetValue(dataId, out itemCraftFormula))
                return;
            if (queueItems.Count >= maxQueueSize)
                return;
            queueItems.Add(new CraftingQueueItem()
            {
                dataId = dataId,
                amount = amount,
                craftRemainsDuration = itemCraftFormula.CraftDuration,
            });
        }

        [ServerRpc]
        private void RpcChangeCraftingQueueItem(int index, short amount)
        {
            if (Entity.IsDead())
                return;
            if (index < 0 || index >= queueItems.Count)
                return;
            if (amount <= 0)
            {
                queueItems.RemoveAt(index);
                return;
            }
            CraftingQueueItem craftingItem = queueItems[index];
            craftingItem.amount = amount;
            queueItems[index] = craftingItem;
        }

        [ServerRpc]
        private void RpcCancelCraftingQueueItem(int index)
        {
            if (Entity.IsDead())
                return;
            if (index < 0 || index >= queueItems.Count)
                return;
            queueItems.RemoveAt(index);
        }
    }
}
