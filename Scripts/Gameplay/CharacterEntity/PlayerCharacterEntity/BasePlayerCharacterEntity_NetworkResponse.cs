using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        [ServerRpc]
        protected void ServerAddAttribute(int dataId)
        {
#if !CLIENT_BUILD
            if (this.IsDead())
                return;
            GameMessage.Type gameMessageType;
            if (this.AddAttribute(out gameMessageType, dataId))
                StatPoint -= 1;
            else
                CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
#endif
        }

        [ServerRpc]
        protected void ServerAddSkill(int dataId)
        {
#if !CLIENT_BUILD
            if (this.IsDead())
                return;
            GameMessage.Type gameMessageType;
            if (this.AddSkill(out gameMessageType, dataId))
                SkillPoint -= 1;
            else
                CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
#endif
        }

        [ServerRpc]
        protected void ServerUseGuildSkill(int dataId)
        {
#if !CLIENT_BUILD
            if (this.IsDead())
                return;

            GuildSkill guildSkill;
            if (!GameInstance.GuildSkills.TryGetValue(dataId, out guildSkill) || guildSkill.GetSkillType() != GuildSkillType.Active)
                return;

            GuildData guild;
            if (GuildId <= 0 || !GameInstance.ServerGuildHandlers.TryGetGuild(GuildId, out guild))
                return;

            short level = guild.GetSkillLevel(dataId);
            if (level <= 0)
                return;

            if (this.IndexOfSkillUsage(dataId, SkillUsageType.GuildSkill) >= 0)
                return;

            // Apply guild skill
            CharacterSkillUsage newSkillUsage = CharacterSkillUsage.Create(SkillUsageType.GuildSkill, dataId);
            newSkillUsage.Use(this, level);
            skillUsages.Add(newSkillUsage);
            ApplyBuff(dataId, BuffType.GuildSkillBuff, level, this);
#endif
        }

        [ServerRpc]
        protected void ServerRespawn()
        {
#if !CLIENT_BUILD
            Respawn();
#endif
        }

        [ServerRpc]
        protected void ServerAssignHotkey(string hotkeyId, HotkeyType type, string relateId)
        {
#if !CLIENT_BUILD
            CharacterHotkey characterHotkey = new CharacterHotkey();
            characterHotkey.hotkeyId = hotkeyId;
            characterHotkey.type = type;
            characterHotkey.relateId = relateId;
            int hotkeyIndex = this.IndexOfHotkey(hotkeyId);
            if (hotkeyIndex >= 0)
                hotkeys[hotkeyIndex] = characterHotkey;
            else
                hotkeys.Add(characterHotkey);
#endif
        }

        [ServerRpc]
        protected void ServerEnterWarp(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            WarpPortalEntity warpPortalEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out warpPortalEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(warpPortalEntity, CurrentGameInstance.conversationDistance))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }

            warpPortalEntity.EnterWarp(this);
#endif
        }

        [ServerRpc]
        protected void ServerConstructBuilding(short itemIndex, Vector3 position, Quaternion rotation, uint parentObjectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions() ||
                itemIndex >= NonEquipItems.Count)
                return;

            BuildingEntity buildingEntity;
            CharacterItem nonEquipItem = NonEquipItems[itemIndex];
            if (nonEquipItem.IsEmptySlot() ||
                nonEquipItem.GetBuildingItem() == null ||
                nonEquipItem.GetBuildingItem().BuildingEntity == null ||
                !GameInstance.BuildingEntities.TryGetValue(nonEquipItem.GetBuildingItem().BuildingEntity.EntityId, out buildingEntity) ||
                !this.DecreaseItemsByIndex(itemIndex, 1))
                return;

            this.FillEmptySlots();
            BuildingSaveData buildingSaveData = new BuildingSaveData();
            buildingSaveData.Id = GenericUtils.GetUniqueId();
            buildingSaveData.ParentId = string.Empty;
            BuildingEntity parentBuildingEntity;
            if (Manager.TryGetEntityByObjectId(parentObjectId, out parentBuildingEntity))
                buildingSaveData.ParentId = parentBuildingEntity.Id;
            buildingSaveData.EntityId = buildingEntity.EntityId;
            buildingSaveData.CurrentHp = buildingEntity.MaxHp;
            buildingSaveData.RemainsLifeTime = buildingEntity.LifeTime;
            buildingSaveData.Position = position;
            buildingSaveData.Rotation = rotation;
            buildingSaveData.CreatorId = Id;
            buildingSaveData.CreatorName = CharacterName;
            CurrentGameManager.CreateBuildingEntity(buildingSaveData, false);
#endif
        }

        [ServerRpc]
        protected void ServerDestroyBuilding(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out buildingEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(buildingEntity, CurrentGameInstance.conversationDistance))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }

            if (!buildingEntity.IsCreator(this))
            {
                // Character is not the creator
                return;
            }

            CurrentGameManager.DestroyBuildingEntity(buildingEntity.Id);
#endif
        }

        private bool VerifyDismantleItem(short index, short amount, List<CharacterItem> simulatingNonEquipItems, out int returningGold, out List<ItemAmount> returningItems)
        {
            returningGold = 0;
            returningItems = new List<ItemAmount>();

            // Found item or not?
            CharacterItem nonEquipItem = nonEquipItems[index];
            if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotEnoughItems);
                return false;
            }

            if (!CurrentGameInstance.dismantleFilter.Filter(nonEquipItem))
            {
                return false;
            }

            // Simulate data before applies
            if (!simulatingNonEquipItems.DecreaseItemsByIndex(index, amount, CurrentGameInstance.IsLimitInventorySlot))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotEnoughItems);
                return false;
            }

            // Character can receives all items or not?
            returningItems.AddRange(BaseItem.GetDismantleReturnItems(nonEquipItem, amount));
            if (simulatingNonEquipItems.IncreasingItemsWillOverwhelming(
                returningItems,
                true,
                this.GetCaches().LimitItemWeight,
                this.GetCaches().TotalItemWeight,
                CurrentGameInstance.IsLimitInventorySlot,
                this.GetCaches().LimitItemSlot))
            {
                returningItems.Clear();
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotCarryAnymore);
                return false;
            }

            simulatingNonEquipItems.IncreaseItems(returningItems);
            returningGold = nonEquipItem.GetItem().DismantleReturnGold * amount;
            return true;
        }

        [ServerRpc]
        protected void ServerDismantleItem(short index, short amount)
        {
#if !CLIENT_BUILD
            if (this.IsDead())
                return;

            if (index >= nonEquipItems.Count)
                return;

            int returningGold;
            List<ItemAmount> returningItems;

            List<CharacterItem> simulatingNonEquipItems = nonEquipItems.Clone();
            if (!VerifyDismantleItem(index, amount, simulatingNonEquipItems, out returningGold, out returningItems))
                return;

            Gold = Gold.Increase(returningGold);
            this.DecreaseItemsByIndex(index, amount);
            this.IncreaseItems(returningItems);
            this.FillEmptySlots();
#endif
        }

        [ServerRpc]
        protected void ServerDismantleItems(short[] selectedIndexes)
        {
#if !CLIENT_BUILD
            if (this.IsDead())
                return;
            List<short> indexes = new List<short>(selectedIndexes);
            indexes.Sort();
            Dictionary<short, short> indexAmountPairs = new Dictionary<short, short>();
            List<CharacterItem> simulatingNonEquipItems = nonEquipItems.Clone();
            int returningGold = 0;
            List<ItemAmount> returningItems = new List<ItemAmount>();
            short tempIndex;
            short tempAmount;
            int tempReturningGold;
            List<ItemAmount> tempReturningItems;
            for (int i = indexes.Count - 1; i >= 0; --i)
            {
                tempIndex = indexes[i];
                if (indexAmountPairs.ContainsKey(tempIndex))
                    continue;
                if (tempIndex >= nonEquipItems.Count)
                    continue;
                tempAmount = nonEquipItems[tempIndex].amount;
                if (!VerifyDismantleItem(tempIndex, tempAmount, simulatingNonEquipItems, out tempReturningGold, out tempReturningItems))
                    return;
                returningGold += tempReturningGold;
                returningItems.AddRange(tempReturningItems);
                indexAmountPairs.Add(tempIndex, tempAmount);
            }
            Gold = Gold.Increase(returningGold);
            indexes.Clear();
            indexes.AddRange(indexAmountPairs.Keys);
            indexes.Sort();
            for (int i = indexes.Count - 1; i >= 0; --i)
            {
                this.DecreaseItemsByIndex(indexes[i], indexAmountPairs[indexes[i]]);
            }
            this.IncreaseItems(returningItems);
            this.FillEmptySlots();
#endif
        }

        [ServerRpc]
        protected void ServerRefineItem(InventoryType inventoryType, short index)
        {
#if !CLIENT_BUILD
            if (this.IsDead())
                return;

            GameMessage.Type gameMessageType;
            switch (inventoryType)
            {
                case InventoryType.NonEquipItems:
                    BaseItem.RefineNonEquipItem(this, index, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipItems:
                    BaseItem.RefineEquipItem(this, index, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponRight:
                    BaseItem.RefineRightHandItem(this, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponLeft:
                    BaseItem.RefineLeftHandItem(this, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
            }
#endif
        }

        [ServerRpc]
        protected void ServerEnhanceSocketItem(InventoryType inventoryType, short index, int enhancerId, short socketIndex)
        {
#if !CLIENT_BUILD
            if (this.IsDead())
                return;

            GameMessage.Type gameMessageType;
            switch (inventoryType)
            {
                case InventoryType.NonEquipItems:
                    BaseItem.EnhanceSocketNonEquipItem(this, index, enhancerId, socketIndex, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipItems:
                    BaseItem.EnhanceSocketEquipItem(this, index, enhancerId, socketIndex, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponRight:
                    BaseItem.EnhanceSocketRightHandItem(this, enhancerId, socketIndex, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponLeft:
                    BaseItem.EnhanceSocketLeftHandItem(this, enhancerId, socketIndex, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
            }
#endif
        }

        [ServerRpc]
        protected void ServerRemoveEnhancerFromItem(InventoryType inventoryType, short index, short socketIndex)
        {
#if !CLIENT_BUILD
            if (this.IsDead())
                return;

            GameMessage.Type gameMessageType;
            if (!CurrentGameInstance.enhancerRemoval.CanRemove(this, out gameMessageType))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                return;
            }
            bool returnEnhancer = CurrentGameInstance.enhancerRemoval.ReturnEnhancerItem;
            switch (inventoryType)
            {
                case InventoryType.NonEquipItems:
                    BaseItem.RemoveEnhancerFromNonEquipItem(this, index, socketIndex, returnEnhancer, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipItems:
                    BaseItem.RemoveEnhancerFromEquipItem(this, index, socketIndex, returnEnhancer, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponRight:
                    BaseItem.RemoveEnhancerFromRightHandItem(this, socketIndex, returnEnhancer, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponLeft:
                    BaseItem.RemoveEnhancerFromLeftHandItem(this, socketIndex, returnEnhancer, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
            }
#endif
        }

        [ServerRpc]
        protected void ServerRepairItem(InventoryType inventoryType, short index)
        {
#if !CLIENT_BUILD
            if (this.IsDead())
                return;

            GameMessage.Type gameMessageType;
            switch (inventoryType)
            {
                case InventoryType.NonEquipItems:
                    BaseItem.RepairNonEquipItem(this, index, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipItems:
                    BaseItem.RepairEquipItem(this, index, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponRight:
                    BaseItem.RepairRightHandItem(this, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponLeft:
                    BaseItem.RepairLeftHandItem(this, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
            }
#endif
        }

        [ServerRpc]
        protected void ServerRepairEquipItems()
        {
#if !CLIENT_BUILD
            if (this.IsDead())
                return;

            bool success = false;
            GameMessage.Type gameMessageType;
            BaseItem.RepairRightHandItem(this, out gameMessageType);
            success = success || gameMessageType == GameMessage.Type.RepairSuccess;
            BaseItem.RepairLeftHandItem(this, out gameMessageType);
            success = success || gameMessageType == GameMessage.Type.RepairSuccess;
            for (int i = 0; i < EquipItems.Count; ++i)
            {
                BaseItem.RepairEquipItem(this, i, out gameMessageType);
                success = success || gameMessageType == GameMessage.Type.RepairSuccess;
            }
            // Will send messages to inform that it can repair an items when any item can be repaired
            if (success)
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.RepairSuccess);
            else
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotRepair);
#endif
        }

        #region Dealing
        [ServerRpc]
        protected void ServerSendDealingRequest(uint objectId)
        {
#if !CLIENT_BUILD
            BasePlayerCharacterEntity targetCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out targetCharacterEntity))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotFoundCharacter);
                return;
            }
            if (targetCharacterEntity.DealingCharacter != null)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsInAnotherDeal);
                return;
            }
            if (!IsGameEntityInDistance(targetCharacterEntity, CurrentGameInstance.conversationDistance))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }
            DealingCharacter = targetCharacterEntity;
            targetCharacterEntity.DealingCharacter = this;
            // Send receive dealing request to player
            DealingCharacter.CallOwnerReceiveDealingRequest(ObjectId);
#endif
        }

        [TargetRpc]
        protected void TargetReceiveDealingRequest(uint objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowDealingRequestDialog != null)
                onShowDealingRequestDialog.Invoke(playerCharacterEntity);
        }

        [ServerRpc]
        protected void ServerAcceptDealingRequest()
        {
#if !CLIENT_BUILD
            if (DealingCharacter == null)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotAcceptDealingRequest);
                StopDealing();
                return;
            }
            if (!IsGameEntityInDistance(DealingCharacter, CurrentGameInstance.conversationDistance))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                StopDealing();
                return;
            }
            // Set dealing state/data for co player character entity
            DealingCharacter.ClearDealingData();
            DealingCharacter.DealingState = DealingState.Dealing;
            DealingCharacter.CallOwnerAcceptedDealingRequest(ObjectId);
            // Set dealing state/data for player character entity
            ClearDealingData();
            DealingState = DealingState.Dealing;
            CallOwnerAcceptedDealingRequest(DealingCharacter.ObjectId);
#endif
        }

        [ServerRpc]
        protected void ServerDeclineDealingRequest()
        {
#if !CLIENT_BUILD
            if (DealingCharacter != null)
                CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.DealingRequestDeclined);
            CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.DealingRequestDeclined);
            StopDealing();
#endif
        }

        [TargetRpc]
        protected void TargetAcceptedDealingRequest(uint objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowDealingDialog != null)
                onShowDealingDialog.Invoke(playerCharacterEntity);
        }

        [ServerRpc]
        protected void ServerSetDealingItem(short itemIndex, short amount)
        {
#if !CLIENT_BUILD
            if (DealingState != DealingState.Dealing)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.InvalidDealingState);
                return;
            }

            if (itemIndex >= nonEquipItems.Count)
                return;

            DealingCharacterItems dealingItems = DealingItems;
            for (int i = dealingItems.Count - 1; i >= 0; --i)
            {
                if (itemIndex == dealingItems[i].nonEquipIndex)
                {
                    dealingItems.RemoveAt(i);
                    break;
                }
            }
            CharacterItem characterItem = nonEquipItems[itemIndex].Clone();
            characterItem.amount = amount;
            DealingCharacterItem dealingItem = new DealingCharacterItem();
            dealingItem.nonEquipIndex = itemIndex;
            dealingItem.characterItem = characterItem;
            dealingItems.Add(dealingItem);
            // Update to clients
            DealingItems = dealingItems;
#endif
        }

        [ServerRpc]
        protected void ServerSetDealingGold(int gold)
        {
#if !CLIENT_BUILD
            if (DealingState != DealingState.Dealing)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.InvalidDealingState);
                return;
            }
            if (gold > Gold)
                gold = Gold;
            if (gold < 0)
                gold = 0;
            DealingGold = gold;
#endif
        }

        [ServerRpc]
        protected void ServerLockDealing()
        {
#if !CLIENT_BUILD
            if (DealingState != DealingState.Dealing)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.InvalidDealingState);
                return;
            }
            DealingState = DealingState.LockDealing;
#endif
        }

        [ServerRpc]
        protected void ServerConfirmDealing()
        {
#if !CLIENT_BUILD
            if (DealingState != DealingState.LockDealing || !(DealingCharacter.DealingState == DealingState.LockDealing || DealingCharacter.DealingState == DealingState.ConfirmDealing))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.InvalidDealingState);
                return;
            }
            DealingState = DealingState.ConfirmDealing;
            if (DealingState == DealingState.ConfirmDealing && DealingCharacter.DealingState == DealingState.ConfirmDealing)
            {
                if (ExchangingDealingItemsWillOverwhelming())
                {
                    CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.AnotherCharacterCannotCarryAnymore);
                    CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.CannotCarryAnymore);
                }
                else if (DealingCharacter.ExchangingDealingItemsWillOverwhelming())
                {
                    CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotCarryAnymore);
                    CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.AnotherCharacterCannotCarryAnymore);
                }
                else
                {
                    ExchangeDealingItemsAndGold();
                    DealingCharacter.ExchangeDealingItemsAndGold();
                }
                StopDealing();
            }
#endif
        }

        [ServerRpc]
        protected void ServerCancelDealing()
        {
#if !CLIENT_BUILD
            if (DealingCharacter != null)
                CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.DealingCanceled);
            CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.DealingCanceled);
            StopDealing();
#endif
        }

        [TargetRpc]
        protected void TargetUpdateDealingState(DealingState dealingState)
        {
            if (onUpdateDealingState != null)
                onUpdateDealingState.Invoke(dealingState);
        }

        [TargetRpc]
        protected void TargetUpdateAnotherDealingState(DealingState dealingState)
        {
            if (onUpdateAnotherDealingState != null)
                onUpdateAnotherDealingState.Invoke(dealingState);
        }

        [TargetRpc]
        protected void TargetUpdateDealingGold(int gold)
        {
            if (onUpdateDealingGold != null)
                onUpdateDealingGold.Invoke(gold);
        }

        [TargetRpc]
        protected void TargetUpdateAnotherDealingGold(int gold)
        {
            if (onUpdateAnotherDealingGold != null)
                onUpdateAnotherDealingGold.Invoke(gold);
        }

        [TargetRpc]
        protected void TargetUpdateDealingItems(DealingCharacterItems items)
        {
            if (onUpdateDealingItems != null)
                onUpdateDealingItems.Invoke(items);
        }

        [TargetRpc]
        protected void TargetUpdateAnotherDealingItems(DealingCharacterItems items)
        {
            if (onUpdateAnotherDealingItems != null)
                onUpdateAnotherDealingItems.Invoke(items);
        }
        #endregion

        #region Banking
        [ServerRpc]
        protected void ServerOpenStorage(uint objectId, string password)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            StorageEntity storageEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out storageEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(storageEntity, CurrentGameInstance.conversationDistance))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }

            if (storageEntity.Lockable && storageEntity.IsLocked && !storageEntity.LockPassword.Equals(password))
            {
                // Wrong password
                return;
            }

            OpenStorage(StorageType.Building, storageEntity);
#endif
        }

        [ServerRpc]
        protected void ServerCloseStorage()
        {
#if !CLIENT_BUILD
            GameInstance.ServerStorageHandlers.CloseStorage(this);
            CurrentStorageId = StorageId.Empty;
#endif
        }
        #endregion

        #region Building Entities
        [ServerRpc]
        protected void ServerOpenDoor(uint objectId, string password)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            DoorEntity doorEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out doorEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(doorEntity, CurrentGameInstance.conversationDistance))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }

            if (doorEntity.Lockable && doorEntity.IsLocked && !doorEntity.LockPassword.Equals(password))
            {
                // Wrong password
                return;
            }

            doorEntity.IsOpen = true;
#endif
        }

        [ServerRpc]
        protected void ServerCloseDoor(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            DoorEntity doorEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out doorEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(doorEntity, CurrentGameInstance.conversationDistance))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }

            doorEntity.IsOpen = false;
#endif
        }

        [ServerRpc]
        protected void ServerTurnOnCampFire(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            CampFireEntity campfireEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out campfireEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(campfireEntity, CurrentGameInstance.conversationDistance))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }

            campfireEntity.TurnOn();
#endif
        }

        [ServerRpc]
        protected void ServerTurnOffCampFire(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            CampFireEntity campfireEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out campfireEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(campfireEntity, CurrentGameInstance.conversationDistance))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }

            campfireEntity.TurnOff();
#endif
        }

        [ServerRpc]
        protected void ServerCraftItemByWorkbench(uint objectId, int dataId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            WorkbenchEntity workbenchEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out workbenchEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(workbenchEntity, CurrentGameInstance.conversationDistance))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }

            workbenchEntity.CraftItem(this, dataId);
#endif
        }
        #endregion

        #region Building Locking
        [ServerRpc]
        protected void ServerSetBuildingPassword(uint objectId, string password)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out buildingEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(buildingEntity, CurrentGameInstance.conversationDistance))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }

            if (!buildingEntity.IsCreator(this))
            {
                // Character is not the creator
                return;
            }

            if (!buildingEntity.Lockable)
            {
                // It's not lockable building
                return;
            }

            buildingEntity.LockPassword = password;
            buildingEntity.IsLocked = true;
#endif
        }

        [ServerRpc]
        protected void ServerLockBuilding(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out buildingEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(buildingEntity, CurrentGameInstance.conversationDistance))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }

            if (!buildingEntity.IsCreator(this))
            {
                // Character is not the creator
                return;
            }

            if (!buildingEntity.Lockable)
            {
                // It's not lockable building
                return;
            }

            buildingEntity.IsLocked = true;
#endif
        }

        [ServerRpc]
        protected void ServerUnlockBuilding(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out buildingEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(buildingEntity, CurrentGameInstance.conversationDistance))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }

            if (!buildingEntity.IsCreator(this))
            {
                // Character is not the creator
                return;
            }

            if (!buildingEntity.Lockable)
            {
                // It's not lockable building
                return;
            }

            buildingEntity.IsLocked = false;
#endif
        }
        #endregion

        public void StopDealing()
        {
            if (DealingCharacter == null)
            {
                ClearDealingData();
                return;
            }
            // Set dealing state/data for co player character entity
            DealingCharacter.ClearDealingData();
            DealingCharacter.DealingCharacter = null;
            // Set dealing state/data for player character entity
            ClearDealingData();
            DealingCharacter = null;
        }

        public void StopPartyInvitation()
        {
            if (DealingCharacter != null)
                DealingCharacter.DealingCharacter = null;
            DealingCharacter = null;
        }

        public void StopGuildInvitation()
        {
            if (DealingCharacter != null)
                DealingCharacter.DealingCharacter = null;
            DealingCharacter = null;
        }

        public void OpenStorage(StorageType type, BaseGameEntity targetEntity)
        {
            string ownerId;
            switch (type)
            {
                case StorageType.Player:
                    ownerId = UserId;
                    break;
                case StorageType.Guild:
                    if (GuildId <= 0)
                        return;
                    ownerId = GuildId.ToString();
                    break;
                case StorageType.Building:
                    if (!(targetEntity is BuildingEntity)) 
                        return;
                    ownerId = (targetEntity as BuildingEntity).Id;
                    break;
                default:
                    return;
            }

            StorageId storageId = new StorageId(type, ownerId);
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(this, storageId))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }

            if (!CurrentStorageId.Equals(storageId))
            {
                GameInstance.ServerStorageHandlers.CloseStorage(this);
                CurrentStorageId = storageId;
                GameInstance.ServerStorageHandlers.OpenStorage(this);
            }
        }
    }
}
