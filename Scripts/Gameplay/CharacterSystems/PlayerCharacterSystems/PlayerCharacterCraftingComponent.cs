using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class PlayerCharacterCraftingComponent : BaseGameEntityComponent<BasePlayerCharacterEntity>
    {
        [SerializeField]
        private int maxQueueSize = 8;
        private SyncListCraftingItem craftingItems = new SyncListCraftingItem();
        private float timeCounter;

        public IList<CraftingItem> CraftingItems
        {
            get { return craftingItems; }
        }

        public override sealed void OnSetup()
        {
            base.OnSetup();
            craftingItems.forOwnerOnly = true;
        }

        public override sealed void EntityUpdate()
        {
            base.EntityUpdate();
            if (CacheEntity.IsDead())
            {
                if (craftingItems.Count > 0)
                    craftingItems.Clear();
                return;
            }

            if (craftingItems.Count == 0)
            {
                timeCounter = 0f;
                return;
            }

            CraftingItem craftingItem = craftingItems[0];
            ItemCraftFormula formula = GameInstance.ItemCraftFormulas[craftingItem.dataId];
            UITextKeys errorMessage;
            if (!formula.ItemCraft.CanCraft(CacheEntity, out errorMessage))
            {
                timeCounter = 0f;
                craftingItems.RemoveAt(0);
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
                        craftingItems[0] = craftingItem;
                    }
                    else
                    {
                        craftingItems.RemoveAt(0);
                    }
                }
                else
                {
                    // Update remains duration
                    craftingItems[0] = craftingItem;
                }
            }
        }

        public void AppendCraftingQueue(int dataId, short amount)
        {
            RPC(RpcAppendCraftingQueue, dataId, amount);
        }

        public void ChangeCraftingQueue(int index, short amount)
        {
            RPC(RpcChangeCraftingQueue, index, amount);
        }

        [ServerRpc]
        private void RpcAppendCraftingQueue(int dataId, short amount)
        {
            if (CacheEntity.IsDead())
                return;
            ItemCraftFormula itemCraftFormula;
            if (!GameInstance.ItemCraftFormulas.TryGetValue(dataId, out itemCraftFormula))
                return;
            if (craftingItems.Count >= maxQueueSize)
                return;
            craftingItems.Add(new CraftingItem()
            {
                dataId = dataId,
                amount = amount,
                craftRemainsDuration = itemCraftFormula.CraftDuration,
            });
        }

        [ServerRpc]
        private void RpcChangeCraftingQueue(int index, short amount)
        {
            if (CacheEntity.IsDead())
                return;
            if (index < 0 || index >= craftingItems.Count)
                return;
            if (amount <= 0)
            {
                craftingItems.RemoveAt(index);
                return;
            }
            CraftingItem craftingItem = craftingItems[index];
            craftingItem.amount = amount;
            craftingItems[index] = craftingItem;
        }
    }
}
