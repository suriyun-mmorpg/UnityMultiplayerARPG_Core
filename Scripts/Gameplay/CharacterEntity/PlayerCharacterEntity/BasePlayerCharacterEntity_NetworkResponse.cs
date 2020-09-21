using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        protected void ServerSwapOrMergeItem(short fromIndex, short toIndex)
        {
            if (!CanDoActions() ||
                fromIndex >= NonEquipItems.Count ||
                toIndex >= NonEquipItems.Count)
                return;

            CharacterItem fromItem = NonEquipItems[fromIndex];
            CharacterItem toItem = NonEquipItems[toIndex];

            if (fromItem.dataId.Equals(toItem.dataId) && !fromItem.IsFull() && !toItem.IsFull())
            {
                // Merge if same id and not full
                short maxStack = toItem.GetMaxStack();
                if (toItem.amount + fromItem.amount <= maxStack)
                {
                    toItem.amount += fromItem.amount;
                    if (CurrentGameInstance.IsLimitInventorySlot)
                        NonEquipItems[fromIndex] = CharacterItem.Empty;
                    else
                        NonEquipItems.RemoveAt(fromIndex);
                    NonEquipItems[toIndex] = toItem;
                    this.FillEmptySlots();
                }
                else
                {
                    short remains = (short)(toItem.amount + fromItem.amount - maxStack);
                    toItem.amount = maxStack;
                    fromItem.amount = remains;
                    NonEquipItems[fromIndex] = fromItem;
                    NonEquipItems[toIndex] = toItem;
                }
            }
            else
            {
                // Swap
                NonEquipItems[fromIndex] = toItem;
                NonEquipItems[toIndex] = fromItem;
            }
        }

        protected void NetFuncAddAttribute(int dataId)
        {
            if (this.IsDead())
                return;

            GameMessage.Type gameMessageType;
            if (this.AddAttribute(out gameMessageType, dataId))
                StatPoint -= 1;
            else
                CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
        }

        protected void ServerAddSkill(int dataId)
        {
            if (this.IsDead())
                return;

            GameMessage.Type gameMessageType;
            if (this.AddSkill(out gameMessageType, dataId))
                SkillPoint -= 1;
            else
                CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
        }

        protected void ServerAddGuildSkill(int dataId)
        {
            if (this.IsDead())
                return;

            CurrentGameManager.AddGuildSkill(this, dataId);
        }

        protected void ServerUseGuildSkill(int dataId)
        {
            if (this.IsDead())
                return;

            GuildSkill guildSkill;
            if (!GameInstance.GuildSkills.TryGetValue(dataId, out guildSkill) || guildSkill.GetSkillType() != GuildSkillType.Active)
                return;

            GuildData guild;
            if (GuildId <= 0 || !CurrentGameManager.TryGetGuild(GuildId, out guild))
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
        }

        protected void ServerRespawn()
        {
            Respawn();
        }

        protected void ServerAssignHotkey(string hotkeyId, HotkeyType type, string relateId)
        {
            CharacterHotkey characterHotkey = new CharacterHotkey();
            characterHotkey.hotkeyId = hotkeyId;
            characterHotkey.type = type;
            characterHotkey.relateId = relateId;
            int hotkeyIndex = this.IndexOfHotkey(hotkeyId);
            if (hotkeyIndex >= 0)
                hotkeys[hotkeyIndex] = characterHotkey;
            else
                hotkeys.Add(characterHotkey);
        }

        protected void ServerEnterWarp(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            WarpPortalEntity warpPortalEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out warpPortalEntity))
            {
                // Can't find the building
                return;
            }

            if (GameplayUtils.BoundsDistance(WorldBounds, warpPortalEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                // Too far from the warp portal
                return;
            }

            warpPortalEntity.EnterWarp(this);
        }

        protected void ServerBuild(short itemIndex, Vector3 position, Quaternion rotation, PackedUInt parentObjectId)
        {
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
            buildingSaveData.CurrentHp = buildingEntity.maxHp;
            buildingSaveData.RemainsLifeTime = buildingEntity.lifeTime;
            buildingSaveData.Position = position;
            buildingSaveData.Rotation = rotation;
            buildingSaveData.CreatorId = Id;
            buildingSaveData.CreatorName = CharacterName;
            CurrentGameManager.CreateBuildingEntity(buildingSaveData, false);
        }

        protected void ServerDestroyBuilding(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out buildingEntity))
            {
                // Can't find the building
                return;
            }

            if (GameplayUtils.BoundsDistance(WorldBounds, buildingEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                // Too far from the building
                return;
            }

            if (!buildingEntity.IsCreator(this))
            {
                // Character is not the creator
                return;
            }

            CurrentGameManager.DestroyBuildingEntity(buildingEntity.Id);
        }

        protected void NetFuncOpenBuildingStorage(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out buildingEntity))
                return;

            if (buildingEntity != null && buildingEntity is StorageEntity)
                OpenStorage(StorageType.Building, buildingEntity as StorageEntity);
        }

        protected void NetFuncShowStorage(StorageType type, uint objectId, short weightLimit, short slotLimit)
        {
            if (onShowStorage != null)
                onShowStorage.Invoke(type, objectId, weightLimit, slotLimit);
        }

        protected void ServerDismantleItem(short index)
        {
            if (this.IsDead() || index >= nonEquipItems.Count)
                return;

            CharacterItem nonEquipItem = nonEquipItems[index];
            if (nonEquipItem.IsEmptySlot() || !CurrentGameInstance.dismantleFilter.Filter(nonEquipItem))
                return;

            BaseItem item = nonEquipItems[index].GetItem();

            // Simulate data before applies
            List<CharacterItem> tempNonEquipItems = new List<CharacterItem>(nonEquipItems);
            if (!tempNonEquipItems.DecreaseItemsByIndex(index, tempNonEquipItems[index].amount, CurrentGameInstance.IsLimitInventorySlot))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotEnoughItems);
                return;
            }

            List<ItemAmount> returningItems = BaseItem.GetDismantleReturnItems(nonEquipItem);

            if (tempNonEquipItems.IncreasingItemsWillOverwhelming(
                returningItems,
                true,
                this.GetCaches().LimitItemWeight,
                this.GetCaches().TotalItemWeight,
                CurrentGameInstance.IsLimitInventorySlot,
                this.GetCaches().LimitItemSlot))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotCarryAnymore);
                return;
            }

            // Applies simulates data
            this.DecreaseItemsByIndex(index, nonEquipItem.amount);
            this.IncreaseItems(returningItems);
            this.FillEmptySlots();
            Gold += item.DismantleReturnGold * nonEquipItem.amount;
        }

        protected void ServerRefineItem(InventoryType inventoryType, short index)
        {
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
        }

        protected void ServerEnhanceSocketItem(InventoryType inventoryType, short index, int enhancerId)
        {
            if (this.IsDead())
                return;

            GameMessage.Type gameMessageType;
            switch (inventoryType)
            {
                case InventoryType.NonEquipItems:
                    BaseItem.EnhanceSocketNonEquipItem(this, index, enhancerId, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipItems:
                    BaseItem.EnhanceSocketEquipItem(this, index, enhancerId, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponRight:
                    BaseItem.EnhanceSocketRightHandItem(this, enhancerId, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
                case InventoryType.EquipWeaponLeft:
                    BaseItem.EnhanceSocketLeftHandItem(this, enhancerId, out gameMessageType);
                    CurrentGameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    break;
            }
        }

        protected void ServerRepairItem(InventoryType inventoryType, short index)
        {
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
        }

        #region Dealing
        protected void ServerSendDealingRequest(PackedUInt objectId)
        {
            BasePlayerCharacterEntity targetCharacterEntity = null;
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
            if (GameplayUtils.BoundsDistance(WorldBounds, targetCharacterEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }
            DealingCharacter = targetCharacterEntity;
            targetCharacterEntity.DealingCharacter = this;
            // Send receive dealing request to player
            DealingCharacter.RequestReceiveDealingRequest(ObjectId);
        }

        protected void NetFuncReceiveDealingRequest(PackedUInt objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity = null;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowDealingRequestDialog != null)
                onShowDealingRequestDialog.Invoke(playerCharacterEntity);
        }

        protected void ServerAcceptDealingRequest()
        {
            if (DealingCharacter == null)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotAcceptDealingRequest);
                StopDealing();
                return;
            }
            if (GameplayUtils.BoundsDistance(WorldBounds, DealingCharacter.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                StopDealing();
                return;
            }
            // Set dealing state/data for co player character entity
            DealingCharacter.ClearDealingData();
            DealingCharacter.DealingState = DealingState.Dealing;
            DealingCharacter.RequestAcceptedDealingRequest(ObjectId);
            // Set dealing state/data for player character entity
            ClearDealingData();
            DealingState = DealingState.Dealing;
            RequestAcceptedDealingRequest(DealingCharacter.ObjectId);
        }

        protected void ServerDeclineDealingRequest()
        {
            if (DealingCharacter != null)
                CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.DealingRequestDeclined);
            CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.DealingRequestDeclined);
            StopDealing();
        }

        protected void NetFuncAcceptedDealingRequest(PackedUInt objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity = null;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowDealingDialog != null)
                onShowDealingDialog.Invoke(playerCharacterEntity);
        }

        protected void ServerSetDealingItem(short itemIndex, short amount)
        {
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
        }

        protected void ServerSetDealingGold(int gold)
        {
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
        }

        protected void ServerLockDealing()
        {
            if (DealingState != DealingState.Dealing)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.InvalidDealingState);
                return;
            }
            DealingState = DealingState.LockDealing;
        }

        protected void ServerConfirmDealing()
        {
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
        }

        protected void ServerCancelDealing()
        {
            if (DealingCharacter != null)
                CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.DealingCanceled);
            CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.DealingCanceled);
            StopDealing();
        }

        protected void NetFuncUpdateDealingState(DealingState dealingState)
        {
            if (onUpdateDealingState != null)
                onUpdateDealingState.Invoke(dealingState);
        }

        protected void NetFuncUpdateAnotherDealingState(DealingState dealingState)
        {
            if (onUpdateAnotherDealingState != null)
                onUpdateAnotherDealingState.Invoke(dealingState);
        }

        protected void NetFuncUpdateDealingGold(int gold)
        {
            if (onUpdateDealingGold != null)
                onUpdateDealingGold.Invoke(gold);
        }

        protected void NetFuncUpdateAnotherDealingGold(int gold)
        {
            if (onUpdateAnotherDealingGold != null)
                onUpdateAnotherDealingGold.Invoke(gold);
        }

        protected void NetFuncUpdateDealingItems(DealingCharacterItems items)
        {
            if (onUpdateDealingItems != null)
                onUpdateDealingItems.Invoke(items);
        }

        protected void NetFuncUpdateAnotherDealingItems(DealingCharacterItems items)
        {
            if (onUpdateAnotherDealingItems != null)
                onUpdateAnotherDealingItems.Invoke(items);
        }
        #endregion

        #region Party
        protected void ServerCreateParty(bool shareExp, bool shareItem)
        {
            CurrentGameManager.CreateParty(this, shareExp, shareItem);
        }

        protected void ServerChangePartyLeader(string characterId)
        {
            CurrentGameManager.ChangePartyLeader(this, characterId);
        }

        protected void ServerPartySetting(bool shareExp, bool shareItem)
        {
            CurrentGameManager.PartySetting(this, shareExp, shareItem);
        }

        protected void ServerSendPartyInvitation(PackedUInt objectId)
        {
            BasePlayerCharacterEntity targetCharacterEntity = null;
            if (!CurrentGameManager.CanSendPartyInvitation(this, objectId, out targetCharacterEntity))
                return;
            DealingCharacter = targetCharacterEntity;
            targetCharacterEntity.DealingCharacter = this;
            // Send receive party invitation request to player
            targetCharacterEntity.RequestReceivePartyInvitation(ObjectId);
        }

        protected void NetFuncReceivePartyInvitation(PackedUInt objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity = null;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowPartyInvitationDialog != null)
                onShowPartyInvitationDialog.Invoke(playerCharacterEntity);
        }

        protected void ServerAcceptPartyInvitation()
        {
            CurrentGameManager.AddPartyMember(DealingCharacter, this);
            StopPartyInvitation();
        }

        protected void ServerDeclinePartyInvitation()
        {
            if (DealingCharacter != null)
                CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.PartyInvitationDeclined);
            CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.PartyInvitationDeclined);
            StopPartyInvitation();
        }

        protected void ServerKickFromParty(string characterId)
        {
            CurrentGameManager.KickFromParty(this, characterId);
        }

        protected void ServerLeaveParty()
        {
            CurrentGameManager.LeaveParty(this);
        }
        #endregion

        #region Guild
        protected void ServerCreateGuild(string guildName)
        {
            CurrentGameManager.CreateGuild(this, guildName);
        }

        protected void ServerChangeGuildLeader(string characterId)
        {
            CurrentGameManager.ChangeGuildLeader(this, characterId);
        }

        protected void ServerSetGuildMessage(string guildMessage)
        {
            CurrentGameManager.SetGuildMessage(this, guildMessage);
        }

        protected void ServerSetGuildRole(byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            CurrentGameManager.SetGuildRole(this, guildRole, name, canInvite, canKick, shareExpPercentage);
        }

        protected void ServerSetGuildMemberRole(string characterId, byte guildRole)
        {
            CurrentGameManager.SetGuildMemberRole(this, characterId, guildRole);
        }

        protected void ServerSendGuildInvitation(PackedUInt objectId)
        {
            BasePlayerCharacterEntity targetCharacterEntity;
            if (!CurrentGameManager.CanSendGuildInvitation(this, objectId, out targetCharacterEntity))
                return;
            DealingCharacter = targetCharacterEntity;
            targetCharacterEntity.DealingCharacter = this;
            // Send receive guild invitation request to player
            targetCharacterEntity.RequestReceiveGuildInvitation(ObjectId);
        }

        protected void NetFuncReceiveGuildInvitation(PackedUInt objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity = null;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowGuildInvitationDialog != null)
                onShowGuildInvitationDialog.Invoke(playerCharacterEntity);
        }

        protected void ServerAcceptGuildInvitation()
        {
            CurrentGameManager.AddGuildMember(DealingCharacter, this);
            StopGuildInvitation();
        }

        protected void ServerDeclineGuildInvitation()
        {
            if (DealingCharacter != null)
                CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.GuildInvitationDeclined);
            CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.GuildInvitationDeclined);
            StopGuildInvitation();
        }

        protected void ServerKickFromGuild(string characterId)
        {
            CurrentGameManager.KickFromGuild(this, characterId);
        }

        protected void ServerLeaveGuild()
        {
            CurrentGameManager.LeaveGuild(this);
        }
        #endregion

        #region Storage
        protected void ServerMoveItemToStorage(short nonEquipIndex, short amount, short storageItemIndex)
        {
            if (this.IsDead() || nonEquipIndex >= nonEquipItems.Count)
                return;

            CurrentGameManager.MoveItemToStorage(this, CurrentStorageId, nonEquipIndex, amount, storageItemIndex);
        }

        protected void ServerMoveItemFromStorage(short storageItemIndex, short amount, short nonEquipIndex)
        {
            if (this.IsDead() || storageItemIndex >= storageItems.Length)
                return;

            CurrentGameManager.MoveItemFromStorage(this, CurrentStorageId, storageItemIndex, amount, nonEquipIndex);
        }

        protected void ServerSwapOrMergeStorageItem(short fromIndex, short toIndex)
        {
            if (this.IsDead() || fromIndex >= storageItems.Length ||
                toIndex >= storageItems.Length)
                return;

            CurrentGameManager.SwapOrMergeStorageItem(this, CurrentStorageId, fromIndex, toIndex);
        }
        #endregion

        #region Banking
        protected void ServerDepositGold(int amount)
        {
            CurrentGameManager.DepositGold(this, amount);
        }

        protected void ServerWithdrawGold(int amount)
        {
            CurrentGameManager.WithdrawGold(this, amount);
        }

        protected void ServerDepositGuildGold(int amount)
        {
            CurrentGameManager.DepositGuildGold(this, amount);
        }

        protected void ServerWithdrawGuildGold(int amount)
        {
            CurrentGameManager.WithdrawGuildGold(this, amount);
        }

        protected void ServerOpenStorage(PackedUInt objectId, string password)
        {
            if (!CanDoActions())
                return;

            StorageEntity storageEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out storageEntity))
            {
                // Can't find the storage
                return;
            }

            if (GameplayUtils.BoundsDistance(WorldBounds, storageEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                // Too far from the storage
                return;
            }

            if (storageEntity.Lockable && storageEntity.IsLocked && !storageEntity.LockPassword.Equals(password))
            {
                // Wrong password
                return;
            }

            OpenStorage(StorageType.Building, storageEntity);
        }

        protected void ServerCloseStorage()
        {
            CurrentGameManager.CloseStorage(this);
            CurrentStorageId = StorageId.Empty;
        }
        #endregion

        #region Building Entities
        protected void ServerOpenDoor(PackedUInt objectId, string password)
        {
            if (!CanDoActions())
                return;

            DoorEntity doorEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out doorEntity))
            {
                // Can't find the door
                return;
            }

            if (GameplayUtils.BoundsDistance(WorldBounds, doorEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                // Too far from the door
                return;
            }

            if (doorEntity.Lockable && doorEntity.IsLocked && !doorEntity.LockPassword.Equals(password))
            {
                // Wrong password
                return;
            }

            doorEntity.IsOpen = true;
        }

        protected void ServerCloseDoor(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            DoorEntity doorEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out doorEntity))
            {
                // Can't find the door
                return;
            }

            if (GameplayUtils.BoundsDistance(WorldBounds, doorEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                // Too far from the door
                return;
            }

            doorEntity.IsOpen = false;
        }

        protected void ServerTurnOnCampFire(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            CampFireEntity campfireEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out campfireEntity))
            {
                // Can't find the door
                return;
            }

            if (GameplayUtils.BoundsDistance(WorldBounds, campfireEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                // Too far from the door
                return;
            }

            campfireEntity.TurnOn();
        }

        protected void ServerTurnOffCampFire(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            CampFireEntity campfireEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out campfireEntity))
            {
                // Can't find the door
                return;
            }

            if (GameplayUtils.BoundsDistance(WorldBounds, campfireEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                // Too far from the door
                return;
            }

            campfireEntity.TurnOff();
        }

        protected void ServerCraftItemByWorkbench(PackedUInt objectId, int dataId)
        {
            if (!CanDoActions())
                return;

            WorkbenchEntity workbenchEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out workbenchEntity))
            {
                // Can't find the workbench
                return;
            }

            if (GameplayUtils.BoundsDistance(WorldBounds, workbenchEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                // Too far from the workbench
                return;
            }

            workbenchEntity.CraftItem(this, dataId);
        }
        #endregion

        #region Social
        protected void ServerFindCharacters(string characterName)
        {
            CurrentGameManager.FindCharacters(this, characterName);
        }

        protected void ServerAddFriend(string friendCharacterId)
        {
            CurrentGameManager.AddFriend(this, friendCharacterId);
        }

        protected void ServerRemoveFriend(string friendCharacterId)
        {
            CurrentGameManager.RemoveFriend(this, friendCharacterId);
        }

        protected void ServerGetFriends()
        {
            CurrentGameManager.GetFriends(this);
        }
        #endregion

        #region Building Locking
        protected void ServerSetBuildingPassword(PackedUInt objectId, string password)
        {
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out buildingEntity))
            {
                // Can't find the building
                return;
            }

            if (GameplayUtils.BoundsDistance(WorldBounds, buildingEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                // Too far from the building
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
        }

        protected void ServerLockBuilding(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out buildingEntity))
            {
                // Can't find the building
                return;
            }

            if (GameplayUtils.BoundsDistance(WorldBounds, buildingEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                // Too far from the building
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
        }

        protected void ServerUnlockBuilding(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out buildingEntity))
            {
                // Can't find the building
                return;
            }

            if (GameplayUtils.BoundsDistance(WorldBounds, buildingEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                // Too far from the building
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
            string ownerId = UserId;
            switch (type)
            {
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
                case StorageType.None:
                    return;
            }

            StorageId storageId = new StorageId(type, ownerId);
            if (!CurrentGameManager.CanAccessStorage(this, storageId))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }

            Storage storage = CurrentGameManager.GetStorage(storageId);
            if (!CurrentStorageId.Equals(storageId))
            {
                CurrentGameManager.CloseStorage(this);
                CurrentStorageId = storageId;
                CurrentGameManager.OpenStorage(this);
                CallNetFunction(NetFuncShowStorage, ConnectionId, type, targetEntity == null ? 0 : targetEntity.ObjectId, storage.weightLimit, storage.slotLimit);
            }
        }
    }
}
