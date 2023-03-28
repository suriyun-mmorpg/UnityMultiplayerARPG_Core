using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        /// <summary>
        /// This will be called at server to order character to pickup selected item
        /// </summary>
        /// <param name="objectId"></param>
        [ServerRpc]
        protected virtual void ServerPickupItem(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!CanPickUpItem())
                return;

            ItemDropEntity itemDropEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out itemDropEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(itemDropEntity, CurrentGameInstance.pickUpItemDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (!itemDropEntity.IsAbleToLoot(this))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_NOT_ABLE_TO_LOOT);
                return;
            }

            if (this.IncreasingItemsWillOverwhelming(itemDropEntity.DropItems))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_WILL_OVERWHELMING);
                return;
            }

            this.IncreaseItems(itemDropEntity.DropItems, (characterItem) =>
            {
                GameInstance.ServerGameMessageHandlers.NotifyRewardItem(ConnectionId, itemDropEntity.GivenType, characterItem.dataId, characterItem.amount);
            });
            this.FillEmptySlots();
            itemDropEntity.PickedUp();
            // Do something with buffs when use item
            SkillAndBuffComponent.OnPickupItem();
#endif
        }

        /// <summary>
        /// This will be called at server to order character to pickup selected item from items container
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="itemsContainerIndex"></param>
        [ServerRpc]
        protected virtual void ServerPickupItemFromContainer(uint objectId, int itemsContainerIndex, int amount)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!CanPickUpItem())
                return;

            ItemsContainerEntity itemsContainerEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out itemsContainerEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(itemsContainerEntity, CurrentGameInstance.pickUpItemDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (!itemsContainerEntity.IsAbleToLoot(this))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_NOT_ABLE_TO_LOOT);
                return;
            }

            if (itemsContainerIndex < 0 || itemsContainerIndex >= itemsContainerEntity.Items.Count)
                return;

            CharacterItem pickingItem = itemsContainerEntity.Items[itemsContainerIndex].Clone();
            if (amount < 0)
                amount = pickingItem.amount;
            pickingItem.amount = amount;
            if (this.IncreasingItemsWillOverwhelming(pickingItem.dataId, pickingItem.amount))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_WILL_OVERWHELMING);
                return;
            }

            this.IncreaseItems(pickingItem, (characterItem) =>
            {
                GameInstance.ServerGameMessageHandlers.NotifyRewardItem(ConnectionId, itemsContainerEntity.GivenType, characterItem.dataId, characterItem.amount);
            });
            itemsContainerEntity.Items.DecreaseItemsByIndex(itemsContainerIndex, amount, false, true);
            itemsContainerEntity.PickedUp();
            this.FillEmptySlots();
#endif
        }

        /// <summary>
        /// This will be called at server to order character to pickup all items from items container
        /// </summary>
        /// <param name="objectId"></param>
        [ServerRpc]
        protected virtual void ServerPickupAllItemsFromContainer(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!CanPickUpItem())
                return;

            ItemsContainerEntity itemsContainerEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out itemsContainerEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(itemsContainerEntity, CurrentGameInstance.pickUpItemDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (!itemsContainerEntity.IsAbleToLoot(this))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_NOT_ABLE_TO_LOOT);
                return;
            }

            while (itemsContainerEntity.Items.Count > 0)
            {
                CharacterItem pickingItem = itemsContainerEntity.Items[0];
                if (this.IncreasingItemsWillOverwhelming(pickingItem.dataId, pickingItem.amount))
                {
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_WILL_OVERWHELMING);
                    break;
                }

                this.IncreaseItems(pickingItem, (characterItem) =>
                {
                    GameInstance.ServerGameMessageHandlers.NotifyRewardItem(ConnectionId, itemsContainerEntity.GivenType, characterItem.dataId, characterItem.amount);
                });
                itemsContainerEntity.Items.RemoveAt(0);
            }
            itemsContainerEntity.PickedUp();
            this.FillEmptySlots();
#endif
        }

        /// <summary>
        /// This will be called at server to order character to pickup nearby items
        /// </summary>
        [ServerRpc]
        protected virtual void ServerPickupNearbyItems()
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!CanPickUpItem())
                return;
            List<ItemDropEntity> itemDropEntities = FindGameEntitiesInDistance<ItemDropEntity>(CurrentGameInstance.pickUpItemDistance, CurrentGameInstance.itemDropLayer.Mask);
            foreach (ItemDropEntity itemDropEntity in itemDropEntities)
            {
                ServerPickupItem(itemDropEntity.ObjectId);
            }
#endif
        }

        /// <summary>
        /// This will be called at server to order character to drop items
        /// </summary>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        [ServerRpc]
        protected virtual void ServerDropItem(int index, int amount)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (amount <= 0 || !CanDoActions() || index >= NonEquipItems.Count)
                return;

            CharacterItem nonEquipItem = nonEquipItems[index];
            if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
                return;

            if (!this.DecreaseItemsByIndex(index, amount, false))
                return;

            this.FillEmptySlots();

            switch (CurrentGameInstance.playerDropItemMode)
            {
                case PlayerDropItemMode.DropOnGround:
                    // Drop item to the ground
                    CharacterItem dropData = nonEquipItem.Clone();
                    dropData.amount = amount;
                    if (CurrentGameInstance.canPickupItemsWhichDropsByPlayersImmediately)
                        ItemDropEntity.DropItem(this, RewardGivenType.PlayerDrop, dropData, new string[0]);
                    else
                        ItemDropEntity.DropItem(this, RewardGivenType.PlayerDrop, dropData, new string[] { Id });
                    break;
            }
#endif
        }

        [AllRpc]
        protected virtual void AllOnDead()
        {
            if (IsOwnerClient)
            {
                ReloadComponent.CancelReload();
                AttackComponent.CancelAttack();
                UseSkillComponent.CancelSkill();
                ClearActionStates();
            }
            if (onDead != null)
                onDead.Invoke();
        }

        [AllRpc]
        protected virtual void AllOnRespawn()
        {
            if (IsOwnerClient)
                ClearActionStates();
            if (onRespawn != null)
                onRespawn.Invoke();
        }

        [AllRpc]
        protected virtual void AllOnLevelUp()
        {
            CharacterModel.InstantiateEffect(CurrentGameInstance.LevelUpEffect);
            if (onLevelUp != null)
                onLevelUp.Invoke();
        }

        [ServerRpc]
        protected virtual void ServerUnSummon(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            int index = this.IndexOfSummon(objectId);
            if (index < 0)
                return;

            CharacterSummon summon = Summons[index];
            if (summon.type != SummonType.PetItem &&
                summon.type != SummonType.Custom)
                return;

            Summons.RemoveAt(index);
            summon.UnSummon(this);
#endif
        }
    }
}
