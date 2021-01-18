using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        [AllRpc]
        protected void AllPlayAttackAnimation(bool isLeftHand, byte animationIndex, int randomSeed)
        {
            AttackRoutine(isLeftHand, animationIndex, randomSeed).Forget();
        }

        [AllRpc]
        protected void AllPlayUseSkillAnimation(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel)
        {
            PlayUseSkillFunction(isLeftHand, animationIndex, skillDataId, skillLevel, null);
        }

        [AllRpc]
        protected void AllPlayUseSkillAnimationWithAimPosition(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel, Vector3 aimPosition)
        {
            PlayUseSkillFunction(isLeftHand, animationIndex, skillDataId, skillLevel, aimPosition);
        }

        protected virtual void PlayUseSkillFunction(bool isLeftHand, byte animationIndex, int skillDataId, short skillLevel, Vector3? aimPosition)
        {
            BaseSkill skill;
            if (GameInstance.Skills.TryGetValue(skillDataId, out skill) && skillLevel > 0)
            {
                UseSkillRoutine(isLeftHand, animationIndex, skill, skillLevel, aimPosition).Forget();
            }
            else
            {
                ClearActionStates();
            }
        }

        [AllRpc]
        protected void AllPlayReloadAnimation(bool isLeftHand, short reloadingAmmoAmount)
        {
            ReloadRoutine(isLeftHand, reloadingAmmoAmount).Forget();
        }

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
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }

            if (!itemDropEntity.IsAbleToLoot(this))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, GameMessage.Type.NotAbleToLoot);
                return;
            }

            if (this.IncreasingItemsWillOverwhelming(itemDropEntity.DropItems))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, GameMessage.Type.CannotCarryAnymore);
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
            // Drop item to the ground
            CharacterItem dropData = nonEquipItem.Clone();
            dropData.amount = amount;
            ItemDropEntity.DropItem(this, dropData, new uint[] { ObjectId });
#endif
        }

        [AllRpc]
        protected virtual void AllOnDead()
        {
            if (IsOwnerClient)
            {
                CancelReload();
                CancelAttack();
                CancelSkill();
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
            CharacterModel.InstantiateEffect(CurrentGameInstance.levelUpEffect);
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
            if (summon.type != SummonType.Pet)
                return;

            Summons.RemoveAt(index);
            summon.UnSummon(this);
#endif
        }
    }
}
