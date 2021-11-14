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
#if !CLIENT_BUILD
            if (!CanDoActions())
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

            this.IncreaseItems(itemDropEntity.DropItems, (dataId, level, amount) =>
            {
                GameInstance.ServerGameMessageHandlers.NotifyRewardItem(ConnectionId, dataId, amount);
            });
            this.FillEmptySlots();
            itemDropEntity.PickedUp();
#endif
        }

        /// <summary>
        /// This will be called at server to order character to pickup selected item from items container
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="itemsContainerIndex"></param>
        [ServerRpc]
        protected virtual void ServerPickupItemFromContainer(uint objectId, int itemsContainerIndex, short amount)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            ItemsContainerEntity itemsContainerEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out itemsContainerEntity))
            {
                // Can't find the entity
                return;
            }

            if (!itemsContainerEntity.IsAbleToLoot(this))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_NOT_ABLE_TO_LOOT);
                return;
            }

            if (!IsGameEntityInDistance(itemsContainerEntity, CurrentGameInstance.pickUpItemDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
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

            this.IncreaseItems(pickingItem, (dataId, level, increaseAmount) =>
            {
                GameInstance.ServerGameMessageHandlers.NotifyRewardItem(ConnectionId, dataId, increaseAmount);
            });
            itemsContainerEntity.Items.DecreaseItemsByIndex(itemsContainerIndex, amount, false);
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
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            ItemsContainerEntity itemsContainerEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out itemsContainerEntity))
            {
                // Can't find the entity
                return;
            }

            if (!itemsContainerEntity.IsAbleToLoot(this))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_NOT_ABLE_TO_LOOT);
                return;
            }

            if (!IsGameEntityInDistance(itemsContainerEntity, CurrentGameInstance.pickUpItemDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
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

                this.IncreaseItems(pickingItem, (dataId, level, amount) =>
                {
                    GameInstance.ServerGameMessageHandlers.NotifyRewardItem(ConnectionId, dataId, amount);
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
#if !CLIENT_BUILD
            if (!CanDoActions())
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
        protected virtual void ServerDropItem(short index, short amount)
        {
#if !CLIENT_BUILD
            if (!CanDoActions() ||
                index >= nonEquipItems.Count)
                return;

            CharacterItem nonEquipItem = nonEquipItems[index];
            if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
                return;

            if (!this.DecreaseItemsByIndex(index, amount))
                return;

            this.FillEmptySlots();

            switch (CurrentGameInstance.playerDropItemMode)
            {
                case PlayerDropItemMode.DropOnGround:
                    // Drop item to the ground
                    CharacterItem dropData = nonEquipItem.Clone();
                    dropData.amount = amount;
                    if (!CurrentGameInstance.canPickupItemsWhichDropsByPlayersImmediately)
                        ItemDropEntity.DropItem(this, dropData, new uint[] { ObjectId });
                    else
                        ItemDropEntity.DropItem(this, dropData, new uint[0]);
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
#if !CLIENT_BUILD
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
