using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        /// <summary>
        /// This will be called at server to order character to pickup selected thing
        /// </summary>
        /// <param name="objectId"></param>
        [ServerRpc]
        protected virtual void CmdPickup(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!CanPickup())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out IPickupActivatableEntity itemDropEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(itemDropEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (!itemDropEntity.ProceedPickingUpAtServer(this, out UITextKeys message))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, message);
                return;
            }

            // Do something with buffs when pickup something
            SkillAndBuffComponent.OnPickupItem();
#endif
        }

        /// <summary>
        /// This will be called at server to order character to pickup selected item from items container
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="itemsContainerIndex"></param>
        [ServerRpc]
        protected virtual void CmdPickupItemFromContainer(uint objectId, int itemsContainerIndex, int amount)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!CanPickup())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out ItemsContainerEntity itemsContainerEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(itemsContainerEntity))
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
        protected virtual void CmdPickupAllItemsFromContainer(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!CanPickup())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out ItemsContainerEntity itemsContainerEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(itemsContainerEntity))
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
        protected virtual void CmdPickupNearbyItems()
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!CanPickup())
                return;
            List<ItemDropEntity> itemDropEntities = FindGameEntitiesInDistance<ItemDropEntity>(CurrentGameInstance.pickUpItemDistance, CurrentGameInstance.itemDropLayer.Mask);
            foreach (ItemDropEntity itemDropEntity in itemDropEntities)
            {
                CmdPickup(itemDropEntity.ObjectId);
            }
#endif
        }

        /// <summary>
        /// This will be called at server to order character to drop items
        /// </summary>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        [ServerRpc]
        protected virtual void CmdDropItem(int index, int amount)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (amount <= 0 || !CanDoActions() || index >= NonEquipItems.Count)
                return;

            CharacterItem nonEquipItem = NonEquipItems[index];
            if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
                return;

            if (nonEquipItem.GetItem().RestrictDropping)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_ITEM_DROPPING_RESTRICTED);
                return;
            }

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
                        ItemDropEntity.Drop(this, RewardGivenType.PlayerDrop, dropData, new string[0]);
                    else
                        ItemDropEntity.Drop(this, RewardGivenType.PlayerDrop, dropData, new string[] { Id });
                    break;
            }
#endif
        }

        [AllRpc]
        protected virtual void RpcOnDead()
        {
            if (IsOwnerClient)
            {
                AttackComponent.CancelAttack();
                UseSkillComponent.CancelSkill();
                ReloadComponent.CancelReload();
                ClearActionStates();
            }
            if (onDead != null)
                onDead.Invoke();
        }

        [AllRpc]
        protected virtual void RpcOnRespawn()
        {
            if (IsOwnerClient)
                ClearActionStates();
            if (onRespawn != null)
                onRespawn.Invoke();
        }

        [AllRpc]
        protected virtual void RpcOnLevelUp()
        {
            CharacterModel.InstantiateEffect(CurrentGameInstance.LevelUpEffect);
            if (onLevelUp != null)
                onLevelUp.Invoke();
        }

        [ServerRpc]
        protected virtual void CmdUnSummon(uint objectId)
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
