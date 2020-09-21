using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        [ServerRpc]
        protected void ServerSwapOrMergeItem(short fromIndex, short toIndex)
        {
#if !CLIENT_BUILD
            if (!CanDoActions() || fromIndex >= NonEquipItems.Count || toIndex >= NonEquipItems.Count)
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
#endif
        }

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
        protected void ServerAddGuildSkill(int dataId)
        {
#if !CLIENT_BUILD
            if (this.IsDead())
                return;
            CurrentGameManager.AddGuildSkill(this, dataId);
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
        protected void ServerEnterWarp(PackedUInt objectId)
        {
#if !CLIENT_BUILD
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
            buildingSaveData.CurrentHp = buildingEntity.maxHp;
            buildingSaveData.RemainsLifeTime = buildingEntity.lifeTime;
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
#endif
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

        [ServerRpc]
        protected void ServerDismantleItem(short index)
        {
#if !CLIENT_BUILD
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
        protected void ServerEnhanceSocketItem(InventoryType inventoryType, short index, int enhancerId)
        {
#if !CLIENT_BUILD
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

        #region Dealing
        [ServerRpc]
        protected void ServerSendDealingRequest(PackedUInt objectId)
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
            if (GameplayUtils.BoundsDistance(WorldBounds, targetCharacterEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CharacterIsTooFar);
                return;
            }
            DealingCharacter = targetCharacterEntity;
            targetCharacterEntity.DealingCharacter = this;
            // Send receive dealing request to player
            DealingCharacter.RequestReceiveDealingRequest(ObjectId);
#endif
        }

        protected void NetFuncReceiveDealingRequest(PackedUInt objectId)
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

        protected void NetFuncAcceptedDealingRequest(PackedUInt objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity = null;
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
        [ServerRpc]
        protected void ServerCreateParty(bool shareExp, bool shareItem)
        {
#if !CLIENT_BUILD
            CurrentGameManager.CreateParty(this, shareExp, shareItem);
#endif
        }

        [ServerRpc]
        protected void ServerChangePartyLeader(string characterId)
        {
#if !CLIENT_BUILD
            CurrentGameManager.ChangePartyLeader(this, characterId);
#endif
        }

        [ServerRpc]
        protected void ServerPartySetting(bool shareExp, bool shareItem)
        {
#if !CLIENT_BUILD
            CurrentGameManager.PartySetting(this, shareExp, shareItem);
#endif
        }

        [ServerRpc]
        protected void ServerSendPartyInvitation(PackedUInt objectId)
        {
#if !CLIENT_BUILD
            BasePlayerCharacterEntity targetCharacterEntity;
            if (!CurrentGameManager.CanSendPartyInvitation(this, objectId, out targetCharacterEntity))
                return;
            DealingCharacter = targetCharacterEntity;
            targetCharacterEntity.DealingCharacter = this;
            // Send receive party invitation request to player
            targetCharacterEntity.RequestReceivePartyInvitation(ObjectId);
#endif
        }

        [ServerRpc]
        protected void NetFuncReceivePartyInvitation(PackedUInt objectId)
        {
#if !CLIENT_BUILD
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowPartyInvitationDialog != null)
                onShowPartyInvitationDialog.Invoke(playerCharacterEntity);
#endif
        }

        [ServerRpc]
        protected void ServerAcceptPartyInvitation()
        {
#if !CLIENT_BUILD
            CurrentGameManager.AddPartyMember(DealingCharacter, this);
            StopPartyInvitation();
#endif
        }

        [ServerRpc]
        protected void ServerDeclinePartyInvitation()
        {
#if !CLIENT_BUILD
            if (DealingCharacter != null)
                CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.PartyInvitationDeclined);
            CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.PartyInvitationDeclined);
            StopPartyInvitation();
#endif
        }

        [ServerRpc]
        protected void ServerKickFromParty(string characterId)
        {
#if !CLIENT_BUILD
            CurrentGameManager.KickFromParty(this, characterId);
#endif
        }

        [ServerRpc]
        protected void ServerLeaveParty()
        {
#if !CLIENT_BUILD
            CurrentGameManager.LeaveParty(this);
#endif
        }
        #endregion

        #region Guild
        [ServerRpc]
        protected void ServerCreateGuild(string guildName)
        {
#if !CLIENT_BUILD
            CurrentGameManager.CreateGuild(this, guildName);
#endif
        }

        [ServerRpc]
        protected void ServerChangeGuildLeader(string characterId)
        {
#if !CLIENT_BUILD
            CurrentGameManager.ChangeGuildLeader(this, characterId);
#endif
        }

        [ServerRpc]
        protected void ServerSetGuildMessage(string guildMessage)
        {
#if !CLIENT_BUILD
            CurrentGameManager.SetGuildMessage(this, guildMessage);
#endif
        }

        [ServerRpc]
        protected void ServerSetGuildRole(byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
#if !CLIENT_BUILD
            CurrentGameManager.SetGuildRole(this, guildRole, name, canInvite, canKick, shareExpPercentage);
#endif
        }

        [ServerRpc]
        protected void ServerSetGuildMemberRole(string characterId, byte guildRole)
        {
#if !CLIENT_BUILD
            CurrentGameManager.SetGuildMemberRole(this, characterId, guildRole);
#endif
        }

        [ServerRpc]
        protected void ServerSendGuildInvitation(PackedUInt objectId)
        {
#if !CLIENT_BUILD
            BasePlayerCharacterEntity targetCharacterEntity;
            if (!CurrentGameManager.CanSendGuildInvitation(this, objectId, out targetCharacterEntity))
                return;
            DealingCharacter = targetCharacterEntity;
            targetCharacterEntity.DealingCharacter = this;
            // Send receive guild invitation request to player
            targetCharacterEntity.RequestReceiveGuildInvitation(ObjectId);
#endif
        }

        protected void NetFuncReceiveGuildInvitation(PackedUInt objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowGuildInvitationDialog != null)
                onShowGuildInvitationDialog.Invoke(playerCharacterEntity);
        }

        [ServerRpc]
        protected void ServerAcceptGuildInvitation()
        {
#if !CLIENT_BUILD
            CurrentGameManager.AddGuildMember(DealingCharacter, this);
            StopGuildInvitation();
#endif
        }

        [ServerRpc]
        protected void ServerDeclineGuildInvitation()
        {
#if !CLIENT_BUILD
            if (DealingCharacter != null)
                CurrentGameManager.SendServerGameMessage(DealingCharacter.ConnectionId, GameMessage.Type.GuildInvitationDeclined);
            CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.GuildInvitationDeclined);
            StopGuildInvitation();
#endif
        }

        [ServerRpc]
        protected void ServerKickFromGuild(string characterId)
        {
#if !CLIENT_BUILD
            CurrentGameManager.KickFromGuild(this, characterId);
#endif
        }

        [ServerRpc]
        protected void ServerLeaveGuild()
        {
#if !CLIENT_BUILD
            CurrentGameManager.LeaveGuild(this);
#endif
        }
        #endregion

        #region Storage
        [ServerRpc]
        protected void ServerMoveItemToStorage(short nonEquipIndex, short amount, short storageItemIndex)
        {
#if !CLIENT_BUILD
            if (this.IsDead() || nonEquipIndex >= nonEquipItems.Count)
                return;
            CurrentGameManager.MoveItemToStorage(this, CurrentStorageId, nonEquipIndex, amount, storageItemIndex);
#endif
        }

        [ServerRpc]
        protected void ServerMoveItemFromStorage(short storageItemIndex, short amount, short nonEquipIndex)
        {
#if !CLIENT_BUILD
            if (this.IsDead() || storageItemIndex >= storageItems.Length)
                return;
            CurrentGameManager.MoveItemFromStorage(this, CurrentStorageId, storageItemIndex, amount, nonEquipIndex);
#endif
        }

        [ServerRpc]
        protected void ServerSwapOrMergeStorageItem(short fromIndex, short toIndex)
        {
#if !CLIENT_BUILD
            if (this.IsDead() || fromIndex >= storageItems.Length || toIndex >= storageItems.Length)
                return;
            CurrentGameManager.SwapOrMergeStorageItem(this, CurrentStorageId, fromIndex, toIndex);
#endif
        }
        #endregion

        #region Banking
        [ServerRpc]
        protected void ServerDepositGold(int amount)
        {
#if !CLIENT_BUILD
            CurrentGameManager.DepositGold(this, amount);
#endif
        }

        [ServerRpc]
        protected void ServerWithdrawGold(int amount)
        {
#if !CLIENT_BUILD
            CurrentGameManager.WithdrawGold(this, amount);
#endif
        }

        [ServerRpc]
        protected void ServerDepositGuildGold(int amount)
        {
#if !CLIENT_BUILD
            CurrentGameManager.DepositGuildGold(this, amount);
#endif
        }

        [ServerRpc]
        protected void ServerWithdrawGuildGold(int amount)
        {
#if !CLIENT_BUILD
            CurrentGameManager.WithdrawGuildGold(this, amount);
#endif
        }

        [ServerRpc]
        protected void ServerOpenStorage(PackedUInt objectId, string password)
        {
#if !CLIENT_BUILD
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
#endif
        }

        [ServerRpc]
        protected void ServerCloseStorage()
        {
#if !CLIENT_BUILD
            CurrentGameManager.CloseStorage(this);
            CurrentStorageId = StorageId.Empty;
#endif
        }
        #endregion

        #region Building Entities
        [ServerRpc]
        protected void ServerOpenDoor(PackedUInt objectId, string password)
        {
#if !CLIENT_BUILD
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
#endif
        }

        [ServerRpc]
        protected void ServerCloseDoor(PackedUInt objectId)
        {
#if !CLIENT_BUILD
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
#endif
        }

        [ServerRpc]
        protected void ServerTurnOnCampFire(PackedUInt objectId)
        {
#if !CLIENT_BUILD
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
#endif
        }

        [ServerRpc]
        protected void ServerTurnOffCampFire(PackedUInt objectId)
        {
#if !CLIENT_BUILD
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
#endif
        }

        [ServerRpc]
        protected void ServerCraftItemByWorkbench(PackedUInt objectId, int dataId)
        {
#if !CLIENT_BUILD
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
#endif
        }
        #endregion

        #region Social
        [ServerRpc]
        protected void ServerFindCharacters(string characterName)
        {
#if !CLIENT_BUILD
            CurrentGameManager.FindCharacters(this, characterName);
#endif
        }

        [ServerRpc]
        protected void ServerAddFriend(string friendCharacterId)
        {
#if !CLIENT_BUILD
            CurrentGameManager.AddFriend(this, friendCharacterId);
#endif
        }

        [ServerRpc]
        protected void ServerRemoveFriend(string friendCharacterId)
        {
#if !CLIENT_BUILD
            CurrentGameManager.RemoveFriend(this, friendCharacterId);
#endif
        }

        [ServerRpc]
        protected void ServerGetFriends()
        {
#if !CLIENT_BUILD
            CurrentGameManager.GetFriends(this);
#endif
        }
        #endregion

        #region Building Locking
        [ServerRpc]
        protected void ServerSetBuildingPassword(PackedUInt objectId, string password)
        {
#if !CLIENT_BUILD
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
#endif
        }

        [ServerRpc]
        protected void ServerLockBuilding(PackedUInt objectId)
        {
#if !CLIENT_BUILD
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
#endif
        }

        [ServerRpc]
        protected void ServerUnlockBuilding(PackedUInt objectId)
        {
#if !CLIENT_BUILD
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
